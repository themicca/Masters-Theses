using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.ShortestPath
{
    public class EdmondsKarpAlgo
    {
        /// <summary>
        /// Solve the max flow problem using Edmonds-Karp and return
        /// a GraphStepDto that captures snapshots of each step.
        /// </summary>
        /// <param name="graph">The CreateGraphRequestDto containing the graph data.</param>
        /// <returns>A GraphStepDto with snapshots and the final flow graph.</returns>
        public static GraphStepDto SolveGraph(GraphDto graph)
        {
            // Convert the input GraphDto into an adjacency matrix and a node ID array.
            string[] nodes = GraphDtoConvertor.ToNodeIdArray(graph);
            int[][] capacity = GraphDtoConvertor.ToAdjacencyMatrix(graph);
            string src = graph.Src.ToString();
            string target = graph.Target.ToString();

            int nodeCount = nodes.Length;
            int sourceIndex = Array.IndexOf(nodes, src);
            int sinkIndex = Array.IndexOf(nodes, target);

            if (sourceIndex == -1 || sinkIndex == -1)
                throw new ArgumentException("Source or target node not found in the graph.");

            // Initialize the flow matrix (all flows start at 0)
            int[][] flow = new int[nodeCount][];
            for (int i = 0; i < nodeCount; i++)
            {
                flow[i] = new int[nodeCount];
                for (int j = 0; j < nodeCount; j++)
                {
                    flow[i][j] = 0;
                }
            }

            // Create a Snapshots instance for recording algorithm progress.
            Snapshots snapshot = new Snapshots(graph.Nodes.ToArray(), graph.Edges.ToArray());
            snapshot.InitializeFromAdjacencyMatrix(capacity);

            // Main loop: find augmenting paths until none exist.
            while (true)
            {
                int[] parent = new int[nodeCount];
                for (int i = 0; i < nodeCount; i++)
                    parent[i] = -1;

                // Find an augmenting path via BFS in the residual graph.
                bool foundPath = BfsFindPath(capacity, flow, sourceIndex, sinkIndex, parent, snapshot, nodes);
                if (!foundPath)
                    break;

                // Determine the bottleneck capacity along the found path.
                int pathFlow = int.MaxValue;
                int v = sinkIndex;
                while (v != sourceIndex)
                {
                    int u = parent[v];
                    pathFlow = Math.Min(pathFlow, capacity[u][v] - flow[u][v]);
                    v = u;
                }

                // Augment flow along the path.
                v = sinkIndex;
                while (v != sourceIndex)
                {
                    int u = parent[v];
                    flow[u][v] += pathFlow;
                    flow[v][u] -= pathFlow; // Update reverse flow.
                    snapshot.ColorEdge(u, v, Constants.ColorProcessed);
                    v = u;
                }
                // Optionally, update node colors after each augmentation.
                for (int i = 0; i < nodeCount; i++)
                {
                    snapshot.ColorNode(i, Constants.ColorProcessed);
                }
            }

            // Finalize snapshot: mark all nodes and edges as part of the final result.
            for (int i = 0; i < nodeCount; i++)
            {
                snapshot.ColorNode(i, Constants.ColorResult);
            }
            for (int u = 0; u < nodeCount; u++)
            {
                for (int v = 0; v < nodeCount; v++)
                {
                    if (capacity[u][v] > 0)
                        snapshot.ColorEdge(u, v, Constants.ColorResult);
                }
            }

            // Build the minimal result graph: include all node IDs and, for every edge with positive flow,
            // use the snapshot's lookup to get the original edge ID.
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
                EdgeIds = flowEdgeIds.ToArray()
            };

            // Package snapshots and the minimal result graph into the final DTO.
            GraphStepDto stepDto = new GraphStepDto
            {
                Steps = snapshot.Steps,
                ResultGraph = resultGraph
            };

            return stepDto;
        }

        /// <summary>
        /// Performs a BFS in the residual graph to find an augmenting path from source to sink.
        /// Uses snapshot.ColorEdge to record progress.
        /// </summary>
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

            while (queue.Count > 0)
            {
                int u = queue.Dequeue();
                for (int v = 0; v < n; v++)
                {
                    int residual = capacity[u][v] - flow[u][v];
                    if (!visited[v] && residual > 0)
                    {
                        snapshot.ColorEdge(u, v, Constants.ColorProcessing);
                        parent[v] = u;
                        visited[v] = true;
                        queue.Enqueue(v);
                        if (v == sink)
                            return true;
                    }
                }
            }
            return visited[sink];
        }
    }
}
