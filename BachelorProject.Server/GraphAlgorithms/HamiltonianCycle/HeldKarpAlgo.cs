using BachelorProject.Server.GraphAlgorithms.ShortestPath;
using BachelorProject.Server.Helpers;
using BachelorProject.Server.Interfaces;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.HamiltonianCycle
{
    public class HeldKarpAlgo : AlgoBase
    {
        public static GraphStepsResultDto SolveGraph(GraphDto graph, bool makeSnapshots)
        {
            string[] nodes = GraphDtoConvertor.ToNodeIdArray(graph);
            int n = nodes.Length;
            int[][] originalMatrix = GraphDtoConvertor.ToAdjacencyMatrix(graph);
            int[][] matrix = GraphDtoConvertor.ToAdjacencyMatrix(graph);


            if (graph.Src == null)
                throw new ArgumentException("Source node not provided.");
            string src = graph.Src.ToString();
            int start = Array.IndexOf(nodes, src);
            if (start == -1)
                throw new ArgumentException("Source node not found in the node list.");

            List<Step> globalSteps = new List<Step>();

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    if (matrix[i][j] == 0)
                    {
                        GraphDto tempGraph = CloneGraphWithNewEndpoints(graph, nodes[i], nodes[j]);
                        GraphStepsResultDto dijkstraResult = DijkstraAlgo.SolveGraph(tempGraph, false);

                        int weight = (int) dijkstraResult.GraphResult.TotalWeight!;
                        matrix[i][j] = weight;
                    }
                }
            }

            int numSubsets = 1 << n;
            int[,] dp = new int[numSubsets, n];
            int[,] parent = new int[numSubsets, n];
            for (int mask = 0; mask < numSubsets; mask++)
            {
                for (int i = 0; i < n; i++)
                {
                    dp[mask, i] = int.MaxValue / 2;
                    parent[mask, i] = -1;
                }
            }
            dp[1 << start, start] = 0;

            Snapshots snapshot = new Snapshots(graph, makeSnapshots);

            for (int mask = 0; mask < numSubsets; mask++)
            {
                if ((mask & (1 << start)) == 0) continue;
                for (int u = 0; u < n; u++)
                {
                    if ((mask & (1 << u)) == 0) continue;
                    for (int v = 0; v < n; v++)
                    {
                        if ((mask & (1 << v)) != 0) continue;
                        int nextMask = mask | (1 << v);
                        int newCost = dp[mask, u] + matrix[u][v];
                        if (newCost < dp[nextMask, v])
                        {
                            dp[nextMask, v] = newCost;
                            parent[nextMask, v] = u;

                            if (originalMatrix[u][v] != 0)
                            {
                                snapshot.ColorNode(u, GraphHelpers.COLOR_PROCESSING);
                                snapshot.ColorNode(v, GraphHelpers.COLOR_PROCESSING);
                                snapshot.ColorEdge(u, v, GraphHelpers.COLOR_PROCESSING);

                                snapshot.UpdateCurrentTotalWeight(newCost);

                                snapshot.ColorNode(u, GraphHelpers.COLOR_PROCESSED);
                                snapshot.ColorNode(v, GraphHelpers.COLOR_PROCESSED);
                                snapshot.ColorEdge(u, v, GraphHelpers.COLOR_PROCESSED);
                            }
                        }
                    }
                }
            }

            int finalMask = numSubsets - 1;
            int bestCost = int.MaxValue;
            int bestEnd = -1;
            for (int i = 0; i < n; i++)
            {
                if (i == start) continue;
                int cost = dp[finalMask, i] + matrix[i][start];
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestEnd = i;
                }
            }

            List<int> pathIndices = new List<int>();
            int maskRec = finalMask;
            int cur = bestEnd;
            while (cur != start)
            {
                pathIndices.Add(cur);
                int prev = parent[maskRec, cur];
                maskRec = maskRec & ~(1 << cur);
                cur = prev;
            }
            pathIndices.Add(start);
            pathIndices.Reverse();
            pathIndices.Add(start);

            List<string> tour = new List<string>();
            foreach (int idx in pathIndices)
            {
                tour.Add(nodes[idx]);
            }

            List<string> tourEdgeIds = new List<string>();
            for (int i = 0; i < tour.Count - 1; i++)
            {
                string fromId = tour[i];
                string toId = tour[i + 1];
                string? edgeId = snapshot.GetEdgeId(fromId, toId);
                snapshot.ColorNode(pathIndices[i], GraphHelpers.COLOR_RESULT);
                snapshot.ColorEdge(fromId, toId, GraphHelpers.COLOR_RESULT);
                tourEdgeIds.Add(edgeId ?? Guid.NewGuid().ToString());
            }

            GraphResultDto resultGraph = new GraphResultDto
            {
                NodeIds = tour.ToArray(),
                EdgeIds = tourEdgeIds.ToArray(),
                TotalWeight = bestCost,
                AlgoType = GraphHelpers.AlgoTypes.HELD_KARP
            };

            snapshot.Steps.InsertRange(0, globalSteps);

            GraphStepsResultDto stepDto = new GraphStepsResultDto
            {
                Steps = snapshot.Steps,
                GraphResult = resultGraph
            };

            return stepDto;
        }

        private static GraphDto CloneGraphWithNewEndpoints(GraphDto graph, string newSrc, string newTarget)
        {
            GraphDto clone = new GraphDto
            {
                Id = graph.Id,
                Name = graph.Name,
                IsDirected = graph.IsDirected,
                Src = Guid.Parse(newSrc),
                Target = Guid.Parse(newTarget),
                Nodes = new List<NodeDto>(graph.Nodes),
                Edges = new List<EdgeDto>(graph.Edges)
            };
            return clone;
        }
    }
}
