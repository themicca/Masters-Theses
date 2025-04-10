using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.EdgeColoring
{
    public class GreedyColoringAlgo
    {
        public static GraphStepDto SolveGraph(GraphDto graph, bool makeSnapshots)
        {
            string[] nodeIds = GraphDtoConvertor.ToNodeIdArray(graph);
            int n = nodeIds.Length;
            Dictionary<string, int> nodeIndexMap = new Dictionary<string, int>();
            for (int i = 0; i < n; i++)
            {
                nodeIndexMap[nodeIds[i]] = i;
            }

            var adjList = GraphDtoConvertor.ToAdjacencyList(graph);

            int delta = adjList.Values.Max(list => list.Count);
            int maxColors = 2 * delta - 1;

            List<string> colorList = new List<string>();
            for (int i = 0; i < maxColors; i++)
            {
                double hue = (360.0 * i) / maxColors;
                string hexColor = GraphHelpers.ColorFromHSV(hue, 0.8, 0.8);
                colorList.Add(hexColor);
            }

            Dictionary<string, string> edgeColors = new Dictionary<string, string>();
            HashSet<string> usedColors = new HashSet<string>();

            string GetEdgeKey(int u, int v) => u < v ? $"{u}->{v}" : $"{v}->{u}";
            Snapshots snapshot = new Snapshots(graph, makeSnapshots);

            for (int i = 0; i < n; i++)
            {
                string uId = nodeIds[i];
                if (!adjList.ContainsKey(uId)) continue;

                foreach (var (neighborId, _) in adjList[uId])
                {
                    int j = nodeIndexMap[neighborId];
                    if (i >= j) continue;

                    string edgeKey = GetEdgeKey(i, j);
                    if (edgeColors.ContainsKey(edgeKey)) continue;

                    HashSet<string> neighborColors = new HashSet<string>();

                    foreach (var (nbr, _) in adjList[uId])
                    {
                        int k = nodeIndexMap[nbr];
                        string key = GetEdgeKey(i, k);
                        if (edgeColors.TryGetValue(key, out var color))
                            neighborColors.Add(color);
                    }

                    foreach (var (nbr, _) in adjList[neighborId])
                    {
                        int k = nodeIndexMap[nbr];
                        string key = GetEdgeKey(j, k);
                        if (edgeColors.TryGetValue(key, out var color))
                            neighborColors.Add(color);
                    }

                    string chosenColor = colorList.FirstOrDefault(c => !neighborColors.Contains(c)) ?? colorList.Last();

                    edgeColors[edgeKey] = chosenColor;
                    usedColors.Add(chosenColor);

                    snapshot.UpdateCurrentTotalWeight(usedColors.Count);
                    snapshot.ColorEdge(i, j, chosenColor);
                }
            }

            List<string> resultEdgeIds = new List<string>();
            foreach (var kvp in edgeColors)
            {
                var parts = kvp.Key.Split("->");
                int u = int.Parse(parts[0]);
                int v = int.Parse(parts[1]);
                string edgeId = snapshot.GetEdgeId(nodeIds[u], nodeIds[v]) ?? Guid.NewGuid().ToString();
                resultEdgeIds.Add(edgeId);
            }

            ResultGraphDto resultGraph = new ResultGraphDto
            {
                NodeIds = nodeIds,
                EdgeIds = resultEdgeIds.ToArray(),
                GraphType = GraphHelpers.AlgoTypes.GreedyColoring,
                TotalWeight = usedColors.Count
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
