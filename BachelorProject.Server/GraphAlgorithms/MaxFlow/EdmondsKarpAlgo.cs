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
        public static GraphStepDto SolveGraph(CreateGraphRequestDto graph)
        {
            // --- 1) Parse the input graph ---
            string[] nodes = graph.GraphNodes;        // Node labels
            int[][] capacity = graph.GraphEdges;      // Capacity matrix
            string src = graph.GraphSrc;              // Source node label
            string target = graph.GraphTarget;        // Sink node label
            bool isDirected = graph.GraphDirected;

            int nodeCount = nodes.Length;
            int sourceIndex = Array.IndexOf(nodes, src);
            int sinkIndex = Array.IndexOf(nodes, target);

            if (sourceIndex == -1 || sinkIndex == -1)
                throw new ArgumentException("Source or target node not found in the node list.");

            // --- 2) Initialize flow matrix and color structures ---
            // Flow initially zero on all edges
            int[][] flow = new int[nodeCount][];
            for (int i = 0; i < nodeCount; i++)
            {
                flow[i] = new int[nodeCount];
            }

            // Track steps (snapshots) for visualization
            var steps = new List<StepState>();

            // Prepare node and edge color dictionaries
            var nodeColors = new string[nodeCount];
            for (int i = 0; i < nodeCount; i++)
            {
                nodeColors[i] = Constants.ColorBase;
            }

            // Build an edge color dictionary using "u->v" as a key
            var edgeColors = new Dictionary<string, string>();
            for (int i = 0; i < nodeCount; i++)
            {
                for (int j = 0; j < nodeCount; j++)
                {
                    if (capacity[i][j] > 0 && capacity[i][j] < Constants.MaxWeight)
                    {
                        string key = $"{i}->{j}";
                        edgeColors[key] = Constants.ColorBase;
                    }
                }
            }

            // --- 3) Edmonds-Karp Main Loop ---
            while (true)
            {
                // (a) Find an augmenting path using BFS in the residual graph
                int[] parent = new int[nodeCount];
                for (int i = 0; i < nodeCount; i++) parent[i] = -1;

                // We color nodes/edges in BFS as "processing"
                nodeColors[sourceIndex] = Constants.ColorProcessing;
                var foundPath = BfsFindPath(
                    capacity, flow,
                    sourceIndex, sinkIndex,
                    parent,
                    nodeColors, edgeColors,
                    steps, nodes
                );

                // If no augmenting path was found, we are done
                if (!foundPath) break;

                // (b) Compute bottleneck capacity along the path found
                int pathFlow = int.MaxValue;
                int v = sinkIndex;
                while (v != sourceIndex)
                {
                    int u = parent[v];
                    int residual = capacity[u][v] - flow[u][v];
                    if (residual < pathFlow)
                        pathFlow = residual;
                    v = u;
                }

                // (c) Augment flow along that path
                v = sinkIndex;
                while (v != sourceIndex)
                {
                    int u = parent[v];
                    flow[u][v] += pathFlow;
                    flow[v][u] -= pathFlow; // update reverse flow
                    v = u;
                }

                // After each augmentation, we can color nodes and edges
                // as "processed" and record a snapshot
                for (int i = 0; i < nodeCount; i++)
                {
                    if (nodeColors[i] == Constants.ColorProcessing)
                        nodeColors[i] = Constants.ColorProcessed;
                }
                foreach (var key in new List<string>(edgeColors.Keys))
                {
                    if (edgeColors[key] == Constants.ColorProcessing)
                        edgeColors[key] = Constants.ColorProcessed;
                }

                //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));
            }

            // --- 4) Build the final result graph using the flow matrix ---
            // For visualization, let's create a matrix that shows the flow on each edge.
            int[][] finalFlowMatrix = new int[nodeCount][];
            for (int i = 0; i < nodeCount; i++)
            {
                finalFlowMatrix[i] = new int[nodeCount];
                for (int j = 0; j < nodeCount; j++)
                {
                    // Only show positive flow in the final result
                    if (flow[i][j] > 0)
                        finalFlowMatrix[i][j] = flow[i][j];
                }
            }

            // Color all nodes and edges as "result" in the final snapshot
            for (int i = 0; i < nodeCount; i++)
            {
                nodeColors[i] = Constants.ColorResult;
            }
            foreach (var key in new List<string>(edgeColors.Keys))
            {
                edgeColors[key] = Constants.ColorResult;
            }
            //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));

            // Create the final graph DTO
            CreateGraphRequestDto finalGraph = new CreateGraphRequestDto
            {
                GraphNodes = nodes,
                GraphEdges = finalFlowMatrix,
                GraphSrc = src,
                GraphTarget = target,
                GraphDirected = isDirected,
                GraphNodePositions = graph.GraphNodePositions
            };

            // Package everything into GraphStepDto
            GraphStepDto stepDto = new GraphStepDto
            {
                Steps = steps,
                ResultGraph = finalGraph
            };

            return stepDto;
        }

        /// <summary>
        /// Perform a BFS on the residual graph to find a path from source to sink.
        /// Returns true if a path is found, and fills the parent[] array to reconstruct it.
        /// Also updates node/edge colors and snapshots for visualization.
        /// </summary>
        private static bool BfsFindPath(
            int[][] capacity, int[][] flow,
            int source, int sink,
            int[] parent,
            string[] nodeColors,
            Dictionary<string, string> edgeColors,
            List<StepState> steps,
            string[] nodes
        )
        {
            int n = capacity.Length;
            bool[] visited = new bool[n];
            for (int i = 0; i < n; i++)
            {
                visited[i] = false;
            }

            // Standard BFS queue
            var queue = new Queue<int>();
            queue.Enqueue(source);
            visited[source] = true;
            parent[source] = -1;

            while (queue.Count > 0)
            {
                int u = queue.Dequeue();

                // For each vertex v, check if there's residual capacity from u to v
                for (int v = 0; v < n; v++)
                {
                    int residual = capacity[u][v] - flow[u][v];
                    if (!visited[v] && residual > 0)
                    {
                        // Mark the edge u->v as "processing"
                        string edgeKey = $"{u}->{v}";
                        if (edgeColors.ContainsKey(edgeKey))
                        {
                            edgeColors[edgeKey] = Constants.ColorProcessing;
                        }

                        parent[v] = u;
                        visited[v] = true;
                        queue.Enqueue(v);

                        // Take a snapshot after discovering a new vertex/edge
                        //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));

                        // If we reached the sink, stop BFS
                        if (v == sink)
                            return true;
                    }
                }
            }

            return visited[sink];
        }
    }
}
