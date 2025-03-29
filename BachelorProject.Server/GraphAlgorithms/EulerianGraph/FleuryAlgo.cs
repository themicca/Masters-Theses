using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.EulerianGraph
{
    public class FleuryAlgo
    {
        public static GraphStepDto SolveGraph(CreateGraphRequestDto graph)
        {
            string[] nodes = graph.GraphNodes;
            int[][] edges = graph.GraphEdges;
            int nodesCount = nodes.Length;
            bool directed = graph.GraphDirected;
            int startIndex = Array.IndexOf(graph.GraphNodes, graph.GraphSrc);

            // Initialize node and edge colors.
            var nodeColors = new string[nodesCount];
            for (int i = 0; i < nodesCount; i++)
            {
                nodeColors[i] = Constants.ColorBase;
            }
            var edgeColors = new Dictionary<string, string>();

            // For edge coloring, if undirected we add each edge once,
            // while for directed we add all edges.
            for (int i = 0; i < nodesCount; i++)
            {
                for (int j = 0; j < nodesCount; j++)
                {
                    if (edges[i][j] != 0)
                    {
                        string key = directed ? $"{i}->{j}" : i < j ? $"{i}->{j}" : $"{j}->{i}";
                        if (!edgeColors.ContainsKey(key))
                        {
                            edgeColors[key] = Constants.ColorBase;
                        }
                    }
                }
            }

            // -------------------------------
            // 2. Check connectivity
            // -------------------------------
            // For connectivity, we use an underlying DFS that treats the graph as undirected.
            bool[] visited = new bool[nodesCount];
            DFSUtil(startIndex, edges, visited, nodesCount, directed);
            for (int i = 0; i < nodesCount; i++)
            {
                // Check only vertices with nonzero degree.
                int degree = 0;
                for (int j = 0; j < nodesCount; j++)
                {
                    if (edges[i][j] != 0)
                        degree++;
                    // In directed graphs, consider in-degree too.
                    if (directed && edges[j][i] != 0)
                        degree++;
                }
                if (degree > 0 && !visited[i])
                    throw new InvalidOperationException("Graph is not connected.");
            }

            // --------------------------------------
            // 3. Prepare a working copy of the graph
            // --------------------------------------
            int[][] tempEdges = new int[nodesCount][];
            for (int i = 0; i < nodesCount; i++)
            {
                tempEdges[i] = new int[nodesCount];
                for (int j = 0; j < nodesCount; j++)
                {
                    tempEdges[i][j] = edges[i][j];
                }
            }

            List<StepState> steps = new List<StepState>();
            List<int> eulerPath = new List<int>();

            // -------------------------------
            // 4. Run Fleury’s Algorithm
            // -------------------------------
            FleuryUtil(startIndex, tempEdges, eulerPath, nodesCount, nodeColors, edgeColors, nodes, steps, directed);

            // The recursive algorithm adds vertices in reverse order.
            eulerPath.Reverse();

            // Convert vertex indices to node names.
            List<string> path = eulerPath.Select(index => nodes[index]).ToList();

            // Build the pathEdges matrix for the Eulerian path.
            int[][] pathEdges = new int[path.Count][];
            for (int i = 0; i < path.Count; i++)
            {
                pathEdges[i] = new int[path.Count];
            }
            for (int i = 0; i < path.Count - 1; i++)
            {
                int fromIndex = Array.IndexOf(nodes, path[i]);
                int toIndex = Array.IndexOf(nodes, path[i + 1]);
                pathEdges[i][i + 1] = edges[fromIndex][toIndex];
                if (!directed)
                {
                    pathEdges[i + 1][i] = edges[toIndex][fromIndex];
                }
                string key = GetEdgeKey(fromIndex, toIndex, directed);
                if (edgeColors.ContainsKey(key))
                    edgeColors[key] = Constants.ColorResult;

                //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));
            }

            // Mark nodes along the path.
            foreach (var n in path)
            {
                int idx = Array.IndexOf(nodes, n);
                nodeColors[idx] = Constants.ColorResult;
                //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));
            }

            // -----------------------------------
            // 5. Return the final graph and steps
            // -----------------------------------
            CreateGraphRequestDto finalGraph = new CreateGraphRequestDto
            {
                GraphNodes = path.ToArray(),
                GraphEdges = pathEdges,
                GraphSrc = path.First(),
                GraphTarget = path.Last(),
                GraphDirected = directed,
                GraphNodePositions = graph.GraphNodePositions
            };

            GraphStepDto stepDto = new GraphStepDto
            {
                Steps = steps,
                FinalGraph = finalGraph
            };

            return stepDto;
        }

        // Recursive function for Fleury's algorithm.
        private static void FleuryUtil(int u, int[][] tempEdges, List<int> eulerPath, int nodesCount,
                                        string[] nodeColors, Dictionary<string, string> edgeColors, string[] nodes,
                                        List<StepState> steps, bool directed)
        {
            for (int v = 0; v < nodesCount; v++)
            {
                if (tempEdges[u][v] > 0)
                {
                    if (IsValidNextEdge(u, v, tempEdges, nodesCount, directed))
                    {
                        string key = GetEdgeKey(u, v, directed);
                        if (edgeColors.ContainsKey(key))
                        {
                            edgeColors[key] = Constants.ColorProcessing;
                            //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));
                        }
                        RemoveEdge(u, v, tempEdges, directed);
                        //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));
                        FleuryUtil(v, tempEdges, eulerPath, nodesCount, nodeColors, edgeColors, nodes, steps, directed);
                    }
                }
            }
            eulerPath.Add(u);
        }

        // Checks if edge (u,v) is a valid next edge (i.e. not a bridge unless necessary).
        private static bool IsValidNextEdge(int u, int v, int[][] tempEdges, int nodesCount, bool directed)
        {
            int count = 0;
            for (int i = 0; i < nodesCount; i++)
                if (tempEdges[u][i] > 0)
                    count++;

            if (count == 1)
                return true;

            bool[] visited = new bool[nodesCount];
            int count1 = DFSCount(u, tempEdges, visited, nodesCount, directed);

            RemoveEdge(u, v, tempEdges, directed);
            Array.Fill(visited, false);
            int count2 = DFSCount(u, tempEdges, visited, nodesCount, directed);

            AddEdge(u, v, tempEdges, directed);

            return count1 <= count2;
        }

        // Counts reachable vertices from v using DFS.
        private static int DFSCount(int v, int[][] tempEdges, bool[] visited, int nodesCount, bool directed)
        {
            visited[v] = true;
            int count = 1;
            for (int i = 0; i < nodesCount; i++)
            {
                bool edgeExists = directed ? tempEdges[v][i] > 0 || tempEdges[i][v] > 0 : tempEdges[v][i] > 0;
                if (edgeExists && !visited[i])
                    count += DFSCount(i, tempEdges, visited, nodesCount, directed);
            }
            return count;
        }

        // Removes the edge (u,v) from the temporary graph.
        private static void RemoveEdge(int u, int v, int[][] tempEdges, bool directed)
        {
            tempEdges[u][v]--;
            if (!directed)
            {
                tempEdges[v][u]--;
            }
        }

        // Restores the edge (u,v) in the temporary graph.
        private static void AddEdge(int u, int v, int[][] tempEdges, bool directed)
        {
            tempEdges[u][v]++;
            if (!directed)
            {
                tempEdges[v][u]++;
            }
        }

        // DFS utility for connectivity check.
        private static void DFSUtil(int v, int[][] edges, bool[] visited, int nodesCount, bool directed)
        {
            visited[v] = true;
            for (int i = 0; i < nodesCount; i++)
            {
                bool edgeExists = directed ? edges[v][i] != 0 || edges[i][v] != 0 : edges[v][i] != 0;
                if (edgeExists && !visited[i])
                    DFSUtil(i, edges, visited, nodesCount, directed);
            }
        }

        // Returns a consistent key for an edge.
        private static string GetEdgeKey(int u, int v, bool directed)
        {
            return directed ? $"{u}->{v}" : u < v ? $"{u}->{v}" : $"{v}->{u}";
        }
    }
}
