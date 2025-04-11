using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.NodeColoring
{
    public class WelshPowellAlgo
    {
        public static GraphStepDto SolveGraph(GraphDto graph, bool makeSnapshots)
        {
            int[][] matrix = GraphDtoConvertor.ToAdjacencyMatrix(graph);
            string[] nodes = GraphDtoConvertor.ToNodeIdArray(graph);
            int n = nodes.Length;

            Snapshots snapshot = new Snapshots(graph, makeSnapshots);

            int[] degrees = new int[n];
            for (int i = 0; i < n; i++)
            {
                int degree = 0;
                for (int j = 0; j < n; j++)
                {
                    if (i != j && matrix[i][j] != 0)
                        degree++;
                }
                degrees[i] = degree;
            }

            List<int> sortedIndices = Enumerable.Range(0, n).ToList();
            sortedIndices.Sort((a, b) => degrees[b].CompareTo(degrees[a]));

            int[] colorAssignment = new int[n];
            for (int i = 0; i < n; i++)
            {
                colorAssignment[i] = -1;
            }

            int currentColor = 0;
            foreach (int i in sortedIndices)
            {
                if (colorAssignment[i] == -1)
                {
                    colorAssignment[i] = currentColor;
                    snapshot.UpdateCurrentTotalWeight(currentColor + 1);
                    snapshot.ColorNode(i, GetColorFromIndex(currentColor, currentColor + 1));

                    foreach (int j in sortedIndices)
                    {
                        if (colorAssignment[j] == -1 && !IsAdjacentToColor(j, currentColor, colorAssignment, matrix, n))
                        {
                            colorAssignment[j] = currentColor;
                            snapshot.ColorNode(j, GetColorFromIndex(currentColor, currentColor + 1));
                        }
                    }
                    currentColor++;
                }
            }

            ResultGraphDto resultGraph = new ResultGraphDto
            {
                NodeIds = nodes,
                EdgeIds = GraphDtoConvertor.ToEdgeIdArray(graph),
                AlgoType = GraphHelpers.AlgoTypes.WelshPowell,
                TotalWeight = currentColor
            };

            GraphStepDto stepDto = new GraphStepDto
            {
                Steps = snapshot.Steps,
                ResultGraph = resultGraph
            };

            return stepDto;
        }

        private static bool IsAdjacentToColor(int j, int color, int[] colorAssignment, int[][] matrix, int n)
        {
            for (int k = 0; k < n; k++)
            {
                if (colorAssignment[k] == color && matrix[j][k] != 0)
                    return true;
            }
            return false;
        }

        private static string GetColorFromIndex(int colorIndex, int totalColors)
        {
            double hue = (360.0 * colorIndex) / totalColors;
            return GraphHelpers.ColorFromHSV(hue, 0.8, 0.8);
        }
    }
}
