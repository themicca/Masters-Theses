using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.EdgeColoring
{
    public class GreedyColoringAlgo
    {
        public static GraphStepDto SolveGraph(GraphDto graph)
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
                string hexColor = ColorFromHSV(hue, 0.8, 0.8);
                colorList.Add(hexColor);
            }

            Dictionary<string, string> edgeColors = new Dictionary<string, string>();
            string GetEdgeKey(int u, int v) => u < v ? $"{u}->{v}" : $"{v}->{u}";

            Snapshots snapshot = new Snapshots(graph);

            for (int i = 0; i < n; i++)
            {
                string uId = nodeIds[i];
                if (!adjList.ContainsKey(uId))
                    continue;

                foreach (var (neighborId, weight) in adjList[uId])
                {
                    int j = nodeIndexMap[neighborId];
                    if (i >= j)
                        continue;

                    string edgeKey = GetEdgeKey(i, j);
                    if (edgeColors.ContainsKey(edgeKey))
                        continue;

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

                    string chosenColor = colorList.FirstOrDefault(c => !usedColors.Contains(c));
                    if (chosenColor == null)
                        chosenColor = colorList.Last();

                    edgeColors[edgeKey] = chosenColor;
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
                GraphType = Constants.GraphTypes.GreedyColoring
            };

            GraphStepDto stepDto = new GraphStepDto
            {
                Steps = snapshot.Steps,
                ResultGraph = resultGraph
            };

            return stepDto;
        }

        private static string ColorFromHSV(double hue, double saturation, double value)
        {
            double c = value * saturation;
            double x = c * (1 - Math.Abs((hue / 60.0) % 2 - 1));
            double m = value - c;
            double r_prime, g_prime, b_prime;

            if (hue < 60)
            {
                r_prime = c;
                g_prime = x;
                b_prime = 0;
            }
            else if (hue < 120)
            {
                r_prime = x;
                g_prime = c;
                b_prime = 0;
            }
            else if (hue < 180)
            {
                r_prime = 0;
                g_prime = c;
                b_prime = x;
            }
            else if (hue < 240)
            {
                r_prime = 0;
                g_prime = x;
                b_prime = c;
            }
            else if (hue < 300)
            {
                r_prime = x;
                g_prime = 0;
                b_prime = c;
            }
            else
            {
                r_prime = c;
                g_prime = 0;
                b_prime = x;
            }

            int r = (int)Math.Round((r_prime + m) * 255);
            int g = (int)Math.Round((g_prime + m) * 255);
            int b = (int)Math.Round((b_prime + m) * 255);
            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }
}
