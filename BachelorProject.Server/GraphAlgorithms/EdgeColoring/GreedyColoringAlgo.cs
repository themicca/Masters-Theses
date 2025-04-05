using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.EdgeColoring
{
    public class GreedyColoringAlgo
    {
        public static GraphStepDto SolveGraph(GraphDto graph)
        {
            // Get an array of node IDs and build an index mapping.
            string[] nodeIds = GraphDtoConvertor.ToNodeIdArray(graph);
            int n = nodeIds.Length;
            Dictionary<string, int> nodeIndexMap = new Dictionary<string, int>();
            for (int i = 0; i < n; i++)
            {
                nodeIndexMap[nodeIds[i]] = i;
            }

            // Use the converter to get an adjacency list representation.
            // The keys are node IDs, and the values are lists of (neighbor id, weight) pairs.
            var adjList = GraphDtoConvertor.ToAdjacencyList(graph);

            // Compute maximum degree (Delta) using the adjacency list.
            int delta = adjList.Values.Max(list => list.Count);
            int maxColors = 2 * delta - 1;
            List<string> colorList = new List<string>();
            for (int i = 1; i <= maxColors; i++)
            {
                colorList.Add("C" + i);
            }

            // Local dictionary to hold edge colors.
            // Keys are in the form "i->j" (with i < j) representing an undirected edge.
            Dictionary<string, string> edgeColors = new Dictionary<string, string>();
            string GetEdgeKey(int u, int v) => u < v ? $"{u}->{v}" : $"{v}->{u}";

            // Create a Snapshots instance using the full NodeDto and EdgeDto arrays.
            Snapshots snapshot = new Snapshots(graph);

            // Process each edge in the graph via the adjacency list.
            // To avoid duplicates in an undirected graph, we only process an edge if the source's index is less than the neighbor's.
            for (int i = 0; i < n; i++)
            {
                string uId = nodeIds[i];
                if (!adjList.ContainsKey(uId))
                    continue;
                foreach (var (neighborId, weight) in adjList[uId])
                {
                    int j = nodeIndexMap[neighborId];
                    if (i >= j)
                        continue; // Process each undirected edge only once.

                    string edgeKey = GetEdgeKey(i, j);
                    if (edgeColors.ContainsKey(edgeKey))
                        continue; // Already colored.

                    // Collect colors used on edges incident to vertex i.
                    HashSet<string> usedColors = new HashSet<string>();
                    if (adjList.ContainsKey(uId))
                    {
                        foreach (var (nbr, _) in adjList[uId])
                        {
                            int k = nodeIndexMap[nbr];
                            string key = GetEdgeKey(i, k);
                            if (edgeColors.ContainsKey(key))
                                usedColors.Add(edgeColors[key]);
                        }
                    }
                    // And for vertex j.
                    string vId = nodeIds[j];
                    if (adjList.ContainsKey(vId))
                    {
                        foreach (var (nbr, _) in adjList[vId])
                        {
                            int k = nodeIndexMap[nbr];
                            string key = GetEdgeKey(j, k);
                            if (edgeColors.ContainsKey(key))
                                usedColors.Add(edgeColors[key]);
                        }
                    }

                    // Choose the smallest available color that is not used.
                    string chosenColor = colorList.FirstOrDefault(c => !usedColors.Contains(c));
                    if (chosenColor == null)
                        chosenColor = colorList.Last();

                    // Record the chosen color for this edge.
                    edgeColors[edgeKey] = chosenColor;
                    // Update snapshot visualization (using node indices).
                    snapshot.ColorEdge(i, j, chosenColor);
                }
            }

            // Finalize node colors.
            for (int i = 0; i < n; i++)
            {
                snapshot.ColorNode(i, Constants.ColorResult);
            }

            // Build the minimal result graph.
            // For each processed edge, retrieve its original edge ID from the snapshot's lookup.
            List<string> resultEdgeIds = new List<string>();
            foreach (var kvp in edgeColors)
            {
                // The key is "i->j". Parse indices.
                var parts = kvp.Key.Split("->");
                int u = int.Parse(parts[0]);
                int v = int.Parse(parts[1]);
                string edgeId = snapshot.GetEdgeId(nodeIds[u], nodeIds[v]) ?? Guid.NewGuid().ToString();
                resultEdgeIds.Add(edgeId);
            }

            ResultGraphDto resultGraph = new ResultGraphDto
            {
                NodeIds = nodeIds,
                EdgeIds = resultEdgeIds.ToArray()
            };

            // Return the step DTO with snapshots and the minimal result graph.
            GraphStepDto stepDto = new GraphStepDto
            {
                Steps = snapshot.Steps,
                ResultGraph = resultGraph
            };

            return stepDto;
        }
    }
}
