using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.ShortestPath
{
    public class EdmondsKarpAlgo
    {
        public static GraphStepDto SolveGraph(GraphDto graph, bool makeSnapshots)
        {
            string[] nodes = GraphDtoConvertor.ToNodeIdArray(graph);
            int[][] capacity = GraphDtoConvertor.ToAdjacencyMatrix(graph);
            string src = graph.Src.ToString()!;
            string target = graph.Target.ToString()!;

            int nodeCount = nodes.Length;
            int sourceIndex = Array.IndexOf(nodes, src);
            int sinkIndex = Array.IndexOf(nodes, target);

            if (sourceIndex == -1 || sinkIndex == -1)
                throw new ArgumentException("Source or target node not found in the graph.");

            int[][] flow = new int[nodeCount][];
            for (int i = 0; i < nodeCount; i++)
            {
                flow[i] = new int[nodeCount];
                for (int j = 0; j < nodeCount; j++)
                {
                    flow[i][j] = 0;
                }
            }

            Snapshots snapshot = new Snapshots(graph, makeSnapshots);

            while (true)
            {
                int[] parent = new int[nodeCount];
                for (int i = 0; i < nodeCount; i++)
                    parent[i] = -1;

                bool foundPath = BfsFindPath(capacity, flow, sourceIndex, sinkIndex, parent, snapshot, nodes);
                if (!foundPath)
                    break;

                int pathFlow = int.MaxValue;
                int v = sinkIndex;
                while (v != sourceIndex)
                {
                    int u = parent[v];
                    pathFlow = Math.Min(pathFlow, capacity[u][v] - flow[u][v]);
                    v = u;
                }

                v = sinkIndex;
                while (v != sourceIndex)
                {
                    int u = parent[v];
                    flow[u][v] += pathFlow;
                    flow[v][u] -= pathFlow;
                    snapshot.ColorEdge(u, v, GraphHelpers.ColorResult, flow[u][v]);
                    v = u;
                }

                int currentTotalFlow = 0;
                for (int j = 0; j < nodeCount; j++)
                {
                    currentTotalFlow += flow[sourceIndex][j];
                }
                snapshot.UpdateCurrentTotalWeight(currentTotalFlow);
            }

            int totalFlow = 0;
            for (int j = 0; j < nodeCount; j++)
            {
                totalFlow += flow[sourceIndex][j];
            }
            snapshot.UpdateCurrentTotalWeight(totalFlow);

            List<string> flowEdgeIds = new List<string>();
            for (int u = 0; u < nodeCount; u++)
            {
                for (int v = 0; v < nodeCount; v++)
                {
                    if (flow[u][v] > 0)
                    {
                        string? edgeId = snapshot.GetEdgeId(nodes[u], nodes[v]);
                        if (edgeId != null)
                            flowEdgeIds.Add(edgeId);
                    }
                }
            }
            ResultGraphDto resultGraph = new ResultGraphDto
            {
                NodeIds = nodes,
                EdgeIds = flowEdgeIds.ToArray(),
                TotalWeight = totalFlow,
                EdgeResultWeights = new(snapshot.CurrentEdgeWeights),
                GraphType = GraphHelpers.AlgoTypes.EdmondsKarp
            };

            GraphStepDto stepDto = new GraphStepDto
            {
                Steps = snapshot.Steps,
                ResultGraph = resultGraph
            };

            return stepDto;
        }

        private static bool BfsFindPath(
            int[][] capacity, int[][] flow,
            int source, int sink,
            int[] parent,
            Snapshots snapshot,
            string[] nodes)
        {
            int n = capacity.Length;
            bool[] visited = new bool[n];
            for (int i = 0; i < n; i++)
                visited[i] = false;

            Queue<int> queue = new Queue<int>();
            queue.Enqueue(source);
            visited[source] = true;
            parent[source] = -1;

            List<(int u, int v)> bfsEdges = new List<(int u, int v)>();

            while (queue.Count > 0)
            {
                int u = queue.Dequeue();
                for (int v = 0; v < n; v++)
                {
                    int residual = capacity[u][v] - flow[u][v];
                    if (!visited[v] && residual > 0)
                    {
                        bfsEdges.Add((u, v));
                        snapshot.ColorEdge(u, v, GraphHelpers.ColorProcessing);
                        parent[v] = u;
                        visited[v] = true;
                        queue.Enqueue(v);
                        if (v == sink)
                        {
                            HashSet<(int, int)> pathEdges = new HashSet<(int, int)>();

                            int temp = sink;
                            while (temp != source)
                            {
                                int u2 = parent[temp];
                                pathEdges.Add((u2, temp));
                                temp = u2;
                            }

                            foreach (var (uu, vv) in bfsEdges)
                            {
                                if (!pathEdges.Contains((uu, vv)))
                                    snapshot.ColorEdge(uu, vv, GraphHelpers.ColorDiscarded);
                            }
                            return true;
                        }
                    }
                }
            }
            return visited[sink];
        }
    }
}
