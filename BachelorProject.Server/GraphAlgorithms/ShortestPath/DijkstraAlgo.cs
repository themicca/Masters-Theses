using Azure.Core;
using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.Domain;
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
            {
                if (!sptSet[v] && dist[v] <= min)
                {
                    min = dist[v];
                    min_index = v;
                }
            }
            return min_index;
        }

        public static GraphStepDto SolveGraph(GraphDto graph)
        {
            string[] nodeIds = GraphDtoConvertor.ToNodeIdArray(graph);
            int[][] edgeMatrix = GraphDtoConvertor.ToAdjacencyMatrix(graph);
            string src = graph.Src.ToString()!;

            bool targetProvided = !string.IsNullOrWhiteSpace(graph.Target?.ToString());
            string? target = targetProvided ? graph.Target.ToString() : null;

            Snapshots snapshot = new Snapshots(graph);

            int currentTotalWeight = 0;

            nodesCount = nodeIds.Length;
            int srcIndex = Array.IndexOf(nodeIds, src);
            int targetIndex = targetProvided ? Array.IndexOf(nodeIds, target) : -1;
            if (srcIndex == -1 || (targetProvided && targetIndex == -1))
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

            for (int count = 0; count < nodesCount; count++)
            {
                int u = minDistance(dist, sptSet);
                if (u == -1)
                    break;

                sptSet[u] = true;
                snapshot.ColorNode(u, Constants.ColorProcessing);

                for (int v = 0; v < nodesCount; v++)
                {
                    if (!sptSet[v] && edgeMatrix[u][v] != 0 && dist[u] < Constants.MaxWeight && dist[u] + edgeMatrix[u][v] < dist[v])
                    {
                        snapshot.ColorEdge(u, v, Constants.ColorProcessing);
                        int newDist = dist[u] + edgeMatrix[u][v];
                        if (newDist < dist[v])
                        {
                            dist[v] = newDist;
                            previous[v] = u;

                            string fromId = nodeIds[u];
                            string toId = nodeIds[v];
                            string? edgeId = snapshot.GetEdgeId(fromId, toId);
                            if (edgeId == null)
                                edgeId = Guid.NewGuid().ToString();
                            currentTotalWeight = newDist;

                            snapshot.ColorEdge(u, v, Constants.ColorProcessed);
                            snapshot.UpdateCurrentTotalWeight(currentTotalWeight);
                        }
                    }
                }
                snapshot.ColorNode(u, Constants.ColorProcessed);
            }

            int totalWeight = 0;

            List<string> resultEdgeIds = new List<string>();
            List<string> path = new List<string>();
            if (targetProvided)
            {
                int cur = targetIndex;
                while (cur != -1)
                {
                    path.Add(nodeIds[cur]);
                    cur = previous[cur];
                }
                path.Reverse();
                if (path.Count == 0 || path[0] != src)
                    throw new InvalidOperationException("No path exists between the source and target nodes.");

                totalWeight = dist[targetIndex];

                for (int i = 0; i < path.Count - 1; i++)
                {
                    string fromId = path[i];
                    string toId = path[i + 1];
                    string? edgeId = snapshot.GetEdgeId(fromId, toId);
                    resultEdgeIds.Add(edgeId ?? Guid.NewGuid().ToString());

                    int fromIndex = Array.IndexOf(nodeIds, fromId);
                    int toIndex = Array.IndexOf(nodeIds, toId);
                    snapshot.ColorEdge(fromIndex, toIndex, Constants.ColorResult);
                    snapshot.ColorNode(fromIndex, Constants.ColorResult);
                }
                int lastIndex = Array.IndexOf(nodeIds, path[path.Count - 1]);
                snapshot.ColorNode(lastIndex, Constants.ColorResult);
            }
            else
            {
                for (int i = 0; i < nodesCount; i++)
                {
                    if (i == srcIndex || previous[i] != -1)
                    {
                        path.Add(nodeIds[i]);
                        if (i != srcIndex)
                        {
                            string fromId = nodeIds[previous[i]];
                            string toId = nodeIds[i];
                            string? edgeId = snapshot.GetEdgeId(fromId, toId);
                            if (edgeId == null)
                                edgeId = Guid.NewGuid().ToString();
                            resultEdgeIds.Add(edgeId);
                            totalWeight += edgeMatrix[previous[i]][i];
                        }
                    }
                }
                for (int i = 0; i < nodesCount; i++)
                {
                    if (i == srcIndex)
                        continue;
                    if (previous[i] != -1)
                    {
                        snapshot.ColorEdge(previous[i], i, Constants.ColorResult);
                    }
                }
                for (int i = 0; i < nodesCount; i++)
                {
                    snapshot.ColorNode(i, Constants.ColorResult);
                }
            }

            ResultGraphDto resultGraph = new ResultGraphDto
            {
                NodeIds = path.ToArray(),
                EdgeIds = resultEdgeIds.ToArray(),
                TotalWeight = totalWeight,
                GraphType = Constants.GraphTypes.Dijkstra
            };

            GraphStepDto stepDto = new GraphStepDto
            {
                Steps = snapshot.Steps,
                ResultGraph = resultGraph
            };

            return stepDto;
        }
    }
}
