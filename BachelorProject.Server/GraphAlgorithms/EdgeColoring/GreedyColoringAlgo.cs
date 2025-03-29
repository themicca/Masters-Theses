using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.EdgeColoring
{
    public class GreedyColoringAlgo
    {
        public static GraphStepDto SolveGraph(CreateGraphRequestDto graph)
        {
            string[] nodes = graph.GraphNodes;
            int n = nodes.Length;
            int[][] edges = graph.GraphEdges;

            // Compute the maximum degree (Delta) of the graph.
            int[] degree = new int[n];
            int delta = 0;
            for (int i = 0; i < n; i++)
            {
                int d = 0;
                for (int j = 0; j < n; j++)
                {
                    if (edges[i][j] != 0 && i != j)
                    {
                        d++;
                    }
                }
                degree[i] = d;
                if (d > delta)
                    delta = d;
            }
            // Greedy edge coloring can use at most 2*Delta - 1 colors.
            int maxColors = 2 * delta - 1;
            // Create a list of color labels ("C1", "C2", ..., "C{maxColors}").
            List<string> colorList = new List<string>();
            for (int i = 1; i <= maxColors; i++)
            {
                colorList.Add("C" + i);
            }

            // Initialize visualization steps, node colors, and edge colors.
            var steps = new List<StepState>();
            var nodeColors = new string[n];
            for (int i = 0; i < n; i++)
            {
                nodeColors[i] = Constants.ColorBase;
            }
            var edgeColors = new Dictionary<string, string>();
            // For an undirected graph, only process each edge once (i < j).
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (edges[i][j] != 0)
                    {
                        string key = GetEdgeKey(i, j);
                        edgeColors[key] = Constants.ColorBase; // uncolored state.
                    }
                }
            }
            // Capture the initial state.
            //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));

            // Process each edge in the graph in a greedy manner.
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (edges[i][j] != 0)
                    {
                        string key = GetEdgeKey(i, j);
                        // Skip if already colored.
                        if (edgeColors[key] != Constants.ColorBase)
                            continue;

                        // Get colors used on edges incident to vertex i and vertex j.
                        HashSet<string> usedColors = new HashSet<string>();
                        // Incident edges at vertex i.
                        for (int k = 0; k < n; k++)
                        {
                            if (edges[i][k] != 0)
                            {
                                string incidentKey = GetEdgeKey(i, k);
                                if (edgeColors.ContainsKey(incidentKey) && edgeColors[incidentKey] != Constants.ColorBase)
                                {
                                    usedColors.Add(edgeColors[incidentKey]);
                                }
                            }
                        }
                        // Incident edges at vertex j.
                        for (int k = 0; k < n; k++)
                        {
                            if (edges[j][k] != 0)
                            {
                                string incidentKey = GetEdgeKey(j, k);
                                if (edgeColors.ContainsKey(incidentKey) && edgeColors[incidentKey] != Constants.ColorBase)
                                {
                                    usedColors.Add(edgeColors[incidentKey]);
                                }
                            }
                        }

                        // Choose the smallest available color not used by either endpoint.
                        string chosenColor = null;
                        foreach (var color in colorList)
                        {
                            if (!usedColors.Contains(color))
                            {
                                chosenColor = color;
                                break;
                            }
                        }

                        // Assign the chosen color to the edge.
                        edgeColors[key] = chosenColor;
                        // Capture a snapshot after coloring the edge.
                        //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));
                    }
                }
            }

            // Final snapshot after processing all edges.
            //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));

            // Construct the final graph output (edge colors are stored in the edgeColors dictionary).
            CreateGraphRequestDto finalGraph = new CreateGraphRequestDto
            {
                GraphNodes = nodes,
                GraphEdges = edges,
                GraphDirected = graph.GraphDirected,
                GraphNodePositions = graph.GraphNodePositions
            };

            GraphStepDto stepDto = new GraphStepDto
            {
                Steps = steps,
                FinalGraph = finalGraph
            };

            return stepDto;
        }

        // Helper method to generate a unique key for an undirected edge.
        private static string GetEdgeKey(int u, int v)
        {
            return u < v ? $"{u}->{v}" : $"{v}->{u}";
        }
    }
}
