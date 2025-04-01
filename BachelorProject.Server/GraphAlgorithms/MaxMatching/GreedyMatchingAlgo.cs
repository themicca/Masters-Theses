using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.MaxMatching
{
    public class GreedyMatchingAlgo
    {
        // SolveGraph runs a greedy matching algorithm for an undirected graph.
        // It returns a series of snapshots and a final graph showing the matching.
        public static GraphStepDto SolveGraph(CreateGraphRequestDto graph)
        {
            string[] nodes = graph.GraphNodes;
            int n = nodes.Length;
            int[][] edges = graph.GraphEdges; // Assumes a symmetric matrix for an undirected graph.

            // Initialize matching array; -1 indicates unmatched.
            int[] match = new int[n];
            for (int i = 0; i < n; i++)
                match[i] = -1;

            // For visualization: initialize snapshots, node colors, and edge colors.
            var steps = new List<StepState>();
            var nodeColors = new string[n];
            for (int i = 0; i < n; i++)
                nodeColors[i] = Constants.ColorBase;

            var edgeColors = new Dictionary<string, string>();
            // Only consider each undirected edge once (i < j).
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (edges[i][j] != 0 && edges[i][j] < Constants.MaxWeight)
                    {
                        string key = $"{i}->{j}";
                        edgeColors[key] = Constants.ColorBase;
                    }
                }
            }

            // Iterate over all vertex pairs to greedily add matches.
            for (int i = 0; i < n; i++)
            {
                // Skip if vertex i is already matched.
                if (match[i] != -1)
                    continue;

                for (int j = i + 1; j < n; j++)
                {
                    // Skip if vertex j is already matched.
                    if (match[j] != -1)
                        continue;

                    // If there is an edge between i and j, match them.
                    if (edges[i][j] != 0 && edges[i][j] < Constants.MaxWeight)
                    {
                        string edgeKey = $"{i}->{j}";

                        // Mark edge as processing and snapshot.
                        if (edgeColors.ContainsKey(edgeKey))
                        {
                            edgeColors[edgeKey] = Constants.ColorProcessing;
                            //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));
                        }

                        // Greedily match i and j.
                        match[i] = j;
                        match[j] = i;

                        // Update colors: mark nodes and the edge as part of the result.
                        nodeColors[i] = Constants.ColorResult;
                        nodeColors[j] = Constants.ColorResult;
                        if (edgeColors.ContainsKey(edgeKey))
                            edgeColors[edgeKey] = Constants.ColorResult;

                        //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));
                        break; // Move to the next vertex i after a match.
                    }
                }
            }

            // Prepare the final graph: include only the matched edges.
            int[][] matchingEdges = new int[n][];
            for (int i = 0; i < n; i++)
            {
                matchingEdges[i] = new int[n];
                for (int j = 0; j < n; j++)
                    matchingEdges[i][j] = 0;
            }
            for (int i = 0; i < n; i++)
            {
                if (match[i] != -1 && i < match[i])
                {
                    matchingEdges[i][match[i]] = edges[i][match[i]];
                    matchingEdges[match[i]][i] = edges[match[i]][i];
                }
            }

            // Create the final graph DTO.
            CreateGraphRequestDto finalGraph = new CreateGraphRequestDto
            {
                GraphNodes = nodes,
                GraphEdges = matchingEdges,
                GraphSrc = "",    // Not applicable for matching.
                GraphTarget = "",
                GraphDirected = false,
                GraphNodePositions = graph.GraphNodePositions
            };

            GraphStepDto resultDto = new GraphStepDto
            {
                Steps = steps,
                ResultGraph = finalGraph
            };

            return resultDto;
        }
    }
}
