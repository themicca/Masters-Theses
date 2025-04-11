using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.EulerianGraph
{
    public class FleuryAlgo
    {
        public static GraphStepDto SolveGraph(GraphDto graph, bool makeSnapshots)
        {
            string[] nodes = GraphDtoConvertor.ToNodeIdArray(graph);
            int[][] matrix = GraphDtoConvertor.ToAdjacencyMatrix(graph);
            bool directed = graph.IsDirected;
            string src = graph.Src?.ToString()!;

            int nodesCount = nodes.Length;
            int startIndex = Array.IndexOf(nodes, src);

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

            int[][] tempEdges = new int[nodesCount][];
            for (int i = 0; i < nodesCount; i++)
            {
                tempEdges[i] = new int[nodesCount];
                for (int j = 0; j < nodesCount; j++)
                {
                    tempEdges[i][j] = matrix[i][j];
                }
            }

            Snapshots snapshot = new Snapshots(graph, makeSnapshots);

            List<int> eulerPath = new List<int>();
            FleuryUtil(startIndex, tempEdges, eulerPath, nodesCount, snapshot, nodes, directed);
            eulerPath.Reverse();

            List<string> path = eulerPath.Select(index => nodes[index]).ToList();

            HashSet<int> visitedNodes = new HashSet<int>();
            startIndex = eulerPath[^1];
            snapshot.ColorNode(startIndex, GraphHelpers.ColorResult);
            visitedNodes.Add(startIndex);

            List<string> matchingEdgeIds = new List<string>();
            for (int i = 0; i < path.Count - 1; i++)
            {
                int u = eulerPath[eulerPath.Count - 1 - i];
                int v = eulerPath[eulerPath.Count - 2 - i];

                snapshot.ColorEdge(u, v, GraphHelpers.ColorResult);

                if (!visitedNodes.Contains(v))
                {
                    snapshot.ColorNode(v, GraphHelpers.ColorResult);
                    visitedNodes.Add(v);
                }

                string? edgeId = snapshot.GetEdgeId(nodes[u], nodes[v]);
                matchingEdgeIds.Add(edgeId ?? Guid.NewGuid().ToString());
            }

            ResultGraphDto resultGraph = new ResultGraphDto
            {
                NodeIds = path.ToArray(),
                EdgeIds = matchingEdgeIds.ToArray(),
                EulerType = graph.EulerType,
                AlgoType = GraphHelpers.AlgoTypes.Fleury
            };

            GraphStepDto stepDto = new GraphStepDto
            {
                Steps = snapshot.Steps,
                ResultGraph = resultGraph
            };

            return stepDto;
        }

        private static void FleuryUtil(int u, int[][] tempEdges, List<int> eulerPath, int nodesCount,
                                        Snapshots snapshot, string[] nodes, bool directed)
        {
            for (int v = 0; v < nodesCount; v++)
            {
                if (tempEdges[u][v] > 0)
                {
                    if (IsValidNextEdge(u, v, tempEdges, nodesCount, directed))
                    {
                        RemoveEdge(u, v, tempEdges, directed);
                        snapshot.ColorEdge(u, v, GraphHelpers.ColorProcessed);
                        FleuryUtil(v, tempEdges, eulerPath, nodesCount, snapshot, nodes, directed);
                    }
                }
            }
            eulerPath.Add(u);
        }

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

        private static void RemoveEdge(int u, int v, int[][] tempEdges, bool directed)
        {
            tempEdges[u][v]--;
            if (!directed)
                tempEdges[v][u]--;
        }

        private static void AddEdge(int u, int v, int[][] tempEdges, bool directed)
        {
            tempEdges[u][v]++;
            if (!directed)
                tempEdges[v][u]++;
        }

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
