using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.NodeColoring
{
    public class WelshPowellAlgo
    {
        public static GraphStepDto SolveGraph(GraphDto graph)
        {
            // Use the convertor to obtain the adjacency matrix and node ID array.
            int[][] matrix = GraphDtoConvertor.ToAdjacencyMatrix(graph);
            string[] nodes = GraphDtoConvertor.ToNodeIdArray(graph);
            int n = nodes.Length;

            // Create a Snapshots instance for visualization.
            Snapshots snapshot = new Snapshots(graph.Nodes.ToArray(), graph.Edges.ToArray());

            // Compute the degree of each node using the matrix.
            int[] degrees = new int[n];
            for (int i = 0; i < n; i++)
            {
                int degree = 0;
                for (int j = 0; j < n; j++)
                {
                    if (i != j && matrix[i][j] != 0)
                    {
                        degree++;
                    }
                }
                degrees[i] = degree;
            }

            // Create a list of node indices and sort in descending order of degree.
            List<int> sortedIndices = Enumerable.Range(0, n).ToList();
            sortedIndices.Sort((a, b) => degrees[b].CompareTo(degrees[a]));

            // Initialize color assignments: -1 means uncolored.
            int[] colorAssignment = new int[n];
            for (int i = 0; i < n; i++)
            {
                colorAssignment[i] = -1;
            }

            // Helper method: map a color index to a string.
            string GetColorString(int colorIndex)
            {
                string[] availableColors = new string[]
                {
                    "#FF0000", // red
                    "#00FF00", // green
                    "#0000FF", // blue
                    "#FFFF00", // yellow
                    "#FF00FF", // magenta
                    "#00FFFF", // cyan
                    "#800000", // maroon
                    "#008000", // dark green
                    "#000080", // navy
                    "#808000"  // olive
                };
                return availableColors[colorIndex % availableColors.Length];
            }

            int currentColor = 0;
            // Process nodes in descending order of degree.
            foreach (int i in sortedIndices)
            {
                if (colorAssignment[i] == -1)
                {
                    // Assign the current color to node i.
                    colorAssignment[i] = currentColor;
                    snapshot.ColorNode(i, GetColorString(currentColor));

                    // For every other uncolored node, if it is not adjacent to any node
                    // already colored with currentColor, assign the same color.
                    foreach (int j in sortedIndices)
                    {
                        if (colorAssignment[j] == -1 && !IsAdjacentToColor(j, currentColor, colorAssignment, matrix, n))
                        {
                            colorAssignment[j] = currentColor;
                            snapshot.ColorNode(j, GetColorString(currentColor));
                        }
                    }
                    currentColor++;
                }
            }

            // Finalize visualization: mark all nodes with a unified final result color.
            for (int i = 0; i < n; i++)
            {
                snapshot.ColorNode(i, Constants.ColorResult);
            }

            // Build the minimal result graph.
            // For vertex coloring, we output the node IDs and (optionally) an empty edge list.
            ResultGraphDto resultGraph = new ResultGraphDto
            {
                NodeIds = nodes,
                EdgeIds = new string[0]
            };

            GraphStepDto stepDto = new GraphStepDto
            {
                Steps = snapshot.Steps,
                ResultGraph = resultGraph
            };

            return stepDto;
        }

        // Helper method: returns true if node j is adjacent to any node colored with 'color'.
        private static bool IsAdjacentToColor(int j, int color, int[] colorAssignment, int[][] matrix, int n)
        {
            for (int k = 0; k < n; k++)
            {
                if (colorAssignment[k] == color && matrix[j][k] != 0)
                    return true;
            }
            return false;
        }
    }
}
