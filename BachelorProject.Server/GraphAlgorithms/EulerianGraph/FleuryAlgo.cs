using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.EulerianGraph
{
    public class FleuryAlgo
    {
        public static GraphStepDto SolveGraph(GraphDto graph)
        {
            // Use the convertor to obtain the node ID array and the adjacency matrix.
            string[] nodes = GraphDtoConvertor.ToNodeIdArray(graph);
            int[][] matrix = GraphDtoConvertor.ToAdjacencyMatrix(graph);
            bool directed = graph.IsDirected;
            string src = graph.Src?.ToString() ?? throw new ArgumentException("Source not provided.");

            int nodesCount = nodes.Length;
            int startIndex = Array.IndexOf(nodes, src);
            if (startIndex == -1)
                throw new ArgumentException("Source node not found in node list.");

            // Connectivity check: treat the graph as undirected.
            bool[] visited = new bool[nodesCount];
            DFSUtil(startIndex, matrix, visited, nodesCount, false);
            for (int i = 0; i < nodesCount; i++)
            {
                int degree = 0;
                for (int j = 0; j < nodesCount; j++)
                {
                    if (matrix[i][j] != 0) degree++;
                    if (!directed && matrix[j][i] != 0) degree++;
                }
                if (degree > 0 && !visited[i])
                    throw new InvalidOperationException("Graph is not connected.");
            }

            // Create a working copy of the adjacency matrix.
            int[][] tempEdges = new int[nodesCount][];
            for (int i = 0; i < nodesCount; i++)
            {
                tempEdges[i] = new int[nodesCount];
                for (int j = 0; j < nodesCount; j++)
                {
                    tempEdges[i][j] = matrix[i][j];
                }
            }

            // Create a Snapshots instance for visualization.
            Snapshots snapshot = new Snapshots(graph.Nodes.ToArray(), graph.Edges.ToArray());

            // Run Fleury’s algorithm to get the Eulerian path (stored as indices).
            List<int> eulerPath = new List<int>();
            FleuryUtil(startIndex, tempEdges, eulerPath, nodesCount, snapshot, nodes, directed);
            eulerPath.Reverse();

            // Convert indices to node IDs.
            List<string> path = eulerPath.Select(index => nodes[index]).ToList();

            // Mark all nodes in the path as final.
            foreach (int idx in eulerPath)
            {
                snapshot.ColorNode(idx, Constants.ColorResult);
            }

            // Build the list of edge IDs along the Eulerian path.
            List<string> matchingEdgeIds = new List<string>();
            for (int i = 0; i < path.Count - 1; i++)
            {
                string? edgeId = snapshot.GetEdgeId(path[i], path[i + 1]);
                matchingEdgeIds.Add(edgeId ?? Guid.NewGuid().ToString());
            }

            // Build the minimal result graph.
            ResultGraphDto resultGraph = new ResultGraphDto
            {
                NodeIds = path.ToArray(),
                EdgeIds = matchingEdgeIds.ToArray()
            };

            GraphStepDto stepDto = new GraphStepDto
            {
                Steps = snapshot.Steps,
                ResultGraph = resultGraph
            };

            return stepDto;
        }

        // Recursive Fleury algorithm: traverse all edges starting from u.
        private static void FleuryUtil(int u, int[][] tempEdges, List<int> eulerPath, int nodesCount,
                                        Snapshots snapshot, string[] nodes, bool directed)
        {
            for (int v = 0; v < nodesCount; v++)
            {
                if (tempEdges[u][v] > 0)
                {
                    if (IsValidNextEdge(u, v, tempEdges, nodesCount, directed))
                    {
                        // Mark the edge as processing.
                        snapshot.ColorEdge(u, v, Constants.ColorProcessing);
                        RemoveEdge(u, v, tempEdges, directed);
                        snapshot.ColorEdge(u, v, Constants.ColorProcessed);
                        FleuryUtil(v, tempEdges, eulerPath, nodesCount, snapshot, nodes, directed);
                    }
                }
            }
            eulerPath.Add(u);
        }

        // Determines whether edge (u,v) is a valid next edge (not a bridge unless necessary).
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

        // DFS to count reachable vertices.
        private static int DFSCount(int v, int[][] tempEdges, bool[] visited, int nodesCount, bool directed)
        {
            visited[v] = true;
            int count = 1;
            for (int i = 0; i < nodesCount; i++)
            {
                bool edgeExists = directed ? (tempEdges[v][i] > 0 || tempEdges[i][v] > 0) : tempEdges[v][i] > 0;
                if (edgeExists && !visited[i])
                    count += DFSCount(i, tempEdges, visited, nodesCount, directed);
            }
            return count;
        }

        // Remove edge (u,v) from the temporary graph.
        private static void RemoveEdge(int u, int v, int[][] tempEdges, bool directed)
        {
            tempEdges[u][v]--;
            if (!directed)
                tempEdges[v][u]--;
        }

        // Restore edge (u,v) in the temporary graph.
        private static void AddEdge(int u, int v, int[][] tempEdges, bool directed)
        {
            tempEdges[u][v]++;
            if (!directed)
                tempEdges[v][u]++;
        }

        // DFS utility for connectivity check.
        private static void DFSUtil(int v, int[][] edges, bool[] visited, int nodesCount, bool directed)
        {
            visited[v] = true;
            for (int i = 0; i < nodesCount; i++)
            {
                bool edgeExists = directed ? (edges[v][i] != 0 || edges[i][v] != 0) : edges[v][i] != 0;
                if (edgeExists && !visited[i])
                    DFSUtil(i, edges, visited, nodesCount, directed);
            }
        }
    }
}
