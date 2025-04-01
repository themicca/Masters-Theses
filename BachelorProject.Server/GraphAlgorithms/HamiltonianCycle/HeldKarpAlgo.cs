using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.HamiltonianCycle
{
    public class HeldKarpAlgo
    {
        public static GraphStepDto SolveGraph(CreateGraphRequestDto graph)
        {
            // Extract graph data from DTO
            string[] nodes = graph.GraphNodes;
            int[][] edges = graph.GraphEdges;
            string src = graph.GraphSrc;

            int n = nodes.Length;
            int start = Array.IndexOf(nodes, src);
            if (start == -1)
                throw new ArgumentException("Source node not found in the node list.");

            // Number of possible subsets (bitmask representation)
            int numSubsets = 1 << n;
            // dp[mask, i]: minimum cost to reach node 'i' having visited the set 'mask'
            int[,] dp = new int[numSubsets, n];
            // parent[mask, i]: used for path reconstruction
            int[,] parent = new int[numSubsets, n];

            // Initialize DP and parent arrays
            for (int mask = 0; mask < numSubsets; mask++)
            {
                for (int i = 0; i < n; i++)
                {
                    dp[mask, i] = int.MaxValue / 2; // avoid potential overflow
                    parent[mask, i] = -1;
                }
            }
            dp[1 << start, start] = 0;

            // Prepare visualization state: steps, node and edge colors
            var steps = new List<StepState>();
            var nodeColors = new string[n];
            for (int i = 0; i < n; i++)
                nodeColors[i] = Constants.ColorBase;

            var edgeColors = new Dictionary<string, string>();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i != j && edges[i][j] != 0 && edges[i][j] < Constants.MaxWeight)
                    {
                        string key = $"{i}->{j}";
                        edgeColors[key] = Constants.ColorBase;
                    }
                }
            }

            // Dynamic Programming: iterate over every subset (mask) that contains the start node
            for (int mask = 0; mask < numSubsets; mask++)
            {
                if ((mask & 1 << start) == 0) continue; // ensure start is included

                for (int u = 0; u < n; u++)
                {
                    // Skip if u is not in the current subset
                    if ((mask & 1 << u) == 0) continue;
                    // Try to extend the tour by visiting an unvisited vertex v
                    for (int v = 0; v < n; v++)
                    {
                        if ((mask & 1 << v) != 0) continue; // v already visited
                        int nextMask = mask | 1 << v;
                        int newCost = dp[mask, u] + edges[u][v];
                        if (newCost < dp[nextMask, v])
                        {
                            dp[nextMask, v] = newCost;
                            parent[nextMask, v] = u;

                            // Update snapshot: mark current transition as processing
                            nodeColors[u] = Constants.ColorProcessing;
                            nodeColors[v] = Constants.ColorProcessing;
                            string edgeKey = $"{u}->{v}";
                            if (edgeColors.ContainsKey(edgeKey))
                                edgeColors[edgeKey] = Constants.ColorProcessing;
                            //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));

                            // Then mark the nodes and edge as processed
                            nodeColors[u] = Constants.ColorProcessed;
                            nodeColors[v] = Constants.ColorProcessed;
                            if (edgeColors.ContainsKey(edgeKey))
                                edgeColors[edgeKey] = Constants.ColorProcessed;
                            //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));
                        }
                    }
                }
            }

            // Complete the cycle by returning to the start node.
            int finalMask = numSubsets - 1;
            int bestCost = int.MaxValue;
            int bestEnd = -1;
            for (int i = 0; i < n; i++)
            {
                if (i == start) continue;
                int cost = dp[finalMask, i] + edges[i][start];
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestEnd = i;
                }
            }
            if (bestEnd == -1)
                throw new InvalidOperationException("No Hamiltonian cycle exists.");

            // Reconstruct the optimal path
            List<int> pathIndices = new List<int>();
            int maskRec = finalMask;
            int cur = bestEnd;
            // Trace back until we reach the starting node
            while (cur != start)
            {
                pathIndices.Add(cur);
                int prev = parent[maskRec, cur];
                maskRec = maskRec & ~(1 << cur);
                cur = prev;
            }
            pathIndices.Add(start);
            pathIndices.Reverse();
            // Append the starting node at the end to complete the cycle
            pathIndices.Add(start);

            // Build final path representation for visualization and output
            List<string> pathNodeNames = new List<string>();
            foreach (int idx in pathIndices)
            {
                pathNodeNames.Add(nodes[idx]);
                nodeColors[idx] = Constants.ColorResult;
                //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));
            }
            string[] pathNodesArray = pathNodeNames.ToArray();
            int pathCount = pathNodesArray.Length;
            int[][] pathEdges = new int[pathCount][];
            for (int i = 0; i < pathCount; i++)
            {
                pathEdges[i] = new int[pathCount];
            }
            // Set edge weights along the determined path and update edge colors for visualization
            for (int i = 0; i < pathCount - 1; i++)
            {
                int from = Array.IndexOf(nodes, pathNodesArray[i]);
                int to = Array.IndexOf(nodes, pathNodesArray[i + 1]);
                pathEdges[i][i + 1] = edges[from][to];
                string edgeKey = $"{from}->{to}";
                if (edgeColors.ContainsKey(edgeKey))
                    edgeColors[edgeKey] = Constants.ColorResult;
                //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));
            }

            // Build final graph DTO containing the Hamiltonian cycle (TSP tour)
            CreateGraphRequestDto finalGraph = new CreateGraphRequestDto
            {
                GraphNodes = pathNodesArray,
                GraphEdges = pathEdges,
                GraphSrc = nodes[start],
                GraphTarget = nodes[start],
                GraphDirected = graph.GraphDirected,
                GraphNodePositions = graph.GraphNodePositions
            };

            GraphStepDto result = new GraphStepDto
            {
                Steps = steps,
                ResultGraph = finalGraph
            };

            return result;
        }
    }
}
