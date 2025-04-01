using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.NodeColoring
{
    public class WelshPowellAlgo
    {
        public static GraphStepDto SolveGraph(CreateGraphRequestDto graph)
        {
            // Get the nodes and edges from the request.
            string[] nodes = graph.GraphNodes;
            int nodesCount = nodes.Length;
            int[][] edges = graph.GraphEdges;

            // Initialize an array for visual node colors.
            // All nodes start with the base color.
            var nodeColors = new string[nodesCount];
            for (int i = 0; i < nodesCount; i++)
            {
                nodeColors[i] = Constants.ColorBase;
            }

            // Compute the degree (number of adjacent nodes) for each node.
            int[] degrees = new int[nodesCount];
            for (int i = 0; i < nodesCount; i++)
            {
                int degree = 0;
                for (int j = 0; j < nodesCount; j++)
                {
                    // Count an edge if it exists (non-zero) and not a self-loop.
                    if (i != j && edges[i][j] != 0)
                    {
                        degree++;
                    }
                }
                degrees[i] = degree;
            }

            // Create a list of node indices.
            List<int> sortedIndices = new List<int>();
            for (int i = 0; i < nodesCount; i++)
            {
                sortedIndices.Add(i);
            }

            // Sort nodes in descending order of degree.
            sortedIndices.Sort((a, b) => degrees[b].CompareTo(degrees[a]));

            // Initialize the color assignment (-1 indicates no color assigned).
            int[] colorAssignment = new int[nodesCount];
            for (int i = 0; i < nodesCount; i++)
            {
                colorAssignment[i] = -1;
            }

            // List to record snapshots of the algorithm's state.
            var steps = new List<StepState>();

            // Current color index.
            int currentColor = 0;

            // Process nodes in sorted order.
            foreach (int i in sortedIndices)
            {
                // If node i is uncolored, assign the current color.
                if (colorAssignment[i] == -1)
                {
                    colorAssignment[i] = currentColor;
                    nodeColors[i] = GetColorString(currentColor);
                    //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, null));

                    // For each other node, if it is not adjacent to any node already colored with currentColor,
                    // then assign the same color.
                    foreach (int j in sortedIndices)
                    {
                        if (colorAssignment[j] == -1 && !IsAdjacentToColored(j, currentColor, colorAssignment, edges, nodesCount))
                        {
                            colorAssignment[j] = currentColor;
                            nodeColors[j] = GetColorString(currentColor);
                            //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, null));
                        }
                    }
                    // Move on to the next color.
                    currentColor++;
                }
            }

            // Final snapshot: mark all nodes with a final result color.
            // (Alternatively, you might keep the distinct colors per assignment.)
            for (int i = 0; i < nodesCount; i++)
            {
                nodeColors[i] = Constants.ColorResult;
            }
            //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, null));

            // Build the final graph representation.
            CreateGraphRequestDto finalGraph = new CreateGraphRequestDto
            {
                GraphNodes = nodes,
                GraphEdges = edges,
                GraphDirected = graph.GraphDirected,
                GraphNodePositions = graph.GraphNodePositions
            };

            // Package the snapshots and final graph into the step DTO.
            GraphStepDto stepDto = new GraphStepDto
            {
                Steps = steps,
                ResultGraph = finalGraph
            };

            return stepDto;
        }

        // Helper method to check if node j is adjacent to any node with the given color.
        static bool IsAdjacentToColored(int j, int color, int[] colorAssignment, int[][] edges, int nodesCount)
        {
            for (int k = 0; k < nodesCount; k++)
            {
                if (colorAssignment[k] == color && edges[j][k] != 0)
                {
                    return true;
                }
            }
            return false;
        }

        // Helper function to map a color index to a string (for visualization).
        // Feel free to adjust the list of available colors as needed.
        static string GetColorString(int colorIndex)
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
    }
}
