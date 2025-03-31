using Azure.Core;
using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.ShortestPath
{
    public static class DijkstraAlgo
    {
        static int nodesCount;

        static int minDistance(int[] dist, bool[] sptSet)
        {
            int min = int.MaxValue, min_index = -1;

            for (int v = 0; v < nodesCount; v++)
                if (sptSet[v] == false && dist[v] <= min)
                {
                    min = dist[v];
                    min_index = v;
                }

            return min_index;
        }

        public static GraphStepDto SolveGraph(GraphDto graph)
        {
            string[] nodes = graph.GraphNodes;
            int[][] edges = graph.GraphEdges;
            string src = graph.GraphSrc;
            string target = graph.GraphTarget;

            nodesCount = nodes.Length;
            int srcIndex = Array.IndexOf(nodes, src);
            int targetIndex = Array.IndexOf(nodes, target);

            if (srcIndex == -1 || targetIndex == -1)
                throw new ArgumentException("Source or target node not found in the node list.");

            int[] dist = new int[nodesCount];
            bool[] sptSet = new bool[nodesCount];
            int[] previous = new int[nodesCount];

            for (int i = 0; i < nodesCount; i++)
            {
                dist[i] = int.MaxValue;
                sptSet[i] = false;
                previous[i] = -1;
            }

            dist[srcIndex] = 0;

            var snapshot = new Snapshots(graph);

            for (int count = 0; count < nodesCount; count++)
            {
                int u = minDistance(dist, sptSet);
                if (u == -1)
                    break;

                sptSet[u] = true;

                snapshot.ColorNode(u, Constants.ColorProcessing);

                for (int v = 0; v < nodesCount; v++)
                {
                    if (!sptSet[v] && edges[u][v] != 0 && dist[u] < Constants.MaxWeight && dist[u] + edges[u][v] < dist[v])
                    {
                        snapshot.ColorEdge(u, v, Constants.ColorProcessing);

                        int newDist = dist[u] + edges[u][v];
                        if (newDist < dist[v])
                        {
                            dist[v] = newDist;
                            previous[v] = u;
                            snapshot.ColorEdge(u, v, Constants.ColorProcessed);
                        }
                    }
                }
                snapshot.ColorNode(u, Constants.ColorProcessed);
            }

            List<string> path = new List<string>();
            for (int at = targetIndex; at != -1; at = previous[at])
            {
                path.Add(nodes[at]);
            }

            path.Reverse();

            if (path.Count == 0 || path[0] != src)
            {
                throw new InvalidOperationException("No path exists between the source and target nodes.");
            }

            string[] pathNodes = path.ToArray();
            int idx;
            int[][] pathEdges = new int[path.Count][];
            for (int i = 0; i < path.Count; i++)
            {
                pathEdges[i] = new int[path.Count];
            }

            for (int i = 0; i < path.Count - 1; i++)
            {
                int fromIndex = Array.IndexOf(nodes, path[i]);
                int toIndex = Array.IndexOf(nodes, path[i + 1]);
                pathEdges[i][i+1] = edges[fromIndex][toIndex];

                idx = Array.IndexOf(nodes, path[i]);
                snapshot.ColorNode(idx, Constants.ColorResult);
                string key = $"{fromIndex}->{toIndex}";

                if (!graph.GraphDirected)
                {
                    pathEdges[i + 1][i] = edges[toIndex][fromIndex];

                    string key2 = $"{toIndex}->{fromIndex}";
                }
                snapshot.ColorEdge(fromIndex, toIndex, Constants.ColorResult);
            }
            idx = Array.IndexOf(nodes, path[path.Count - 1]);
            snapshot.ColorNode(idx, Constants.ColorResult);

            string pathSource = pathNodes[0];
            string pathTarget = pathNodes[pathNodes.Length - 1];

            CreateGraphRequestDto createGraphRequestDto = new CreateGraphRequestDto{
                GraphNodes = pathNodes,
                GraphEdges = pathEdges,
                GraphSrc = pathSource,
                GraphTarget = pathTarget,
                GraphDirected = graph.GraphDirected,
                GraphNodePositions = graph.GraphNodePositions
            };

            GraphStepDto stepDto = new GraphStepDto{
                Steps = snapshot.Steps,
                FinalGraph = createGraphRequestDto
            };

            return stepDto;
        }
    }
}
