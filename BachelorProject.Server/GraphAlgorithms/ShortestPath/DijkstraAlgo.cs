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
                if (sptSet[v] == false && dist[v] <= min)
                {
                    min = dist[v];
                    min_index = v;
                }

            return min_index;
        }

        public static GraphStepDto SolveGraph(string[] nodeIds, int[][] edges, string src, string target, Snapshots snapshot)
        {
            nodesCount = nodeIds.Length;
            int srcIndex = Array.IndexOf(nodeIds, src);
            int targetIndex = Array.IndexOf(nodeIds, target);

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
                path.Add(nodeIds[at]);
            }

            path.Reverse();

            if (path.Count == 0 || path[0] != src)
            {
                throw new InvalidOperationException("No path exists between the source and target nodes.");
            }

            string[] pathNodes = path.ToArray();

            List<string> resultEdgeIds = new List<string>();
            for (int i = 0; i < pathNodes.Length - 1; i++)
            {
                string fromId = pathNodes[i];
                string toId = pathNodes[i + 1];
                // Retrieve the edge id using the snapshot's lookup method
                string? edgeId = snapshot.GetEdgeId(fromId, toId);
                resultEdgeIds.Add(edgeId ?? Guid.NewGuid().ToString());

                // Update snapshot for visualization (using original indices)
                int fromIndex = Array.IndexOf(nodeIds, fromId);
                int toIndex = Array.IndexOf(nodeIds, toId);
                snapshot.ColorEdge(fromIndex, toIndex, Constants.ColorResult);
                snapshot.ColorNode(fromIndex, Constants.ColorResult);
            }
            int lastIndex = Array.IndexOf(nodeIds, pathNodes[pathNodes.Length - 1]);
            snapshot.ColorNode(lastIndex, Constants.ColorResult);

            // Create the ResultGraphDto with just the node and edge ids
            ResultGraphDto resultGraph = new ResultGraphDto
            {
                nodeIds = pathNodes,
                edgeIds = resultEdgeIds.ToArray()
            };

            // Return the step DTO with the snapshot steps and the result graph.
            GraphStepDto stepDto = new GraphStepDto
            {
                Steps = snapshot.Steps,
                ResultGraph = resultGraph
            };

            return stepDto;
        }
    }
}
