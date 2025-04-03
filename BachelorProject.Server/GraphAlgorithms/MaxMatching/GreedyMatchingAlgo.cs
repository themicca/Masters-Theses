using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.MaxMatching
{
    public class GreedyMatchingAlgo
    {
        // SolveGraph runs a greedy matching algorithm for an undirected graph.
        // It returns a series of snapshots and a final graph showing the matching.
        public static GraphStepDto SolveGraph(GraphDto graph)
        {
            // Convert the graph into an adjacency matrix and node ID array.
            string[] nodeIds = GraphDtoConvertor.ToNodeIdArray(graph);
            int n = nodeIds.Length;
            int[][] edges = GraphDtoConvertor.ToAdjacencyMatrix(graph);

            // Initialize matching array; -1 indicates unmatched.
            int[] match = new int[n];
            for (int i = 0; i < n; i++)
                match[i] = -1;

            // Create a Snapshots instance for visualization.
            Snapshots snapshot = new Snapshots(graph.Nodes.ToArray(), graph.Edges.ToArray());

            // Greedily match vertices: for each vertex i, if unmatched, try to match it with a later vertex j.
            for (int i = 0; i < n; i++)
            {
                if (match[i] != -1)
                    continue;
                for (int j = i + 1; j < n; j++)
                {
                    if (match[j] != -1)
                        continue;
                    // If an edge exists between i and j, greedily match them.
                    if (edges[i][j] != 0 && edges[i][j] < Constants.MaxWeight)
                    {
                        // Visualize edge processing.
                        snapshot.ColorEdge(i, j, Constants.ColorProcessing);

                        // Set the matching.
                        match[i] = j;
                        match[j] = i;

                        // Mark the vertices and the edge as part of the result.
                        snapshot.ColorNode(i, Constants.ColorResult);
                        snapshot.ColorNode(j, Constants.ColorResult);
                        snapshot.ColorEdge(i, j, Constants.ColorResult);

                        break; // Move to the next vertex i after a match.
                    }
                }
            }

            // Build the list of matching edge IDs.
            List<string> matchingEdgeIds = new List<string>();
            for (int i = 0; i < n; i++)
            {
                // For each matched edge, include it only once.
                if (match[i] != -1 && i < match[i])
                {
                    string edgeId = snapshot.GetEdgeId(nodeIds[i], nodeIds[match[i]]) ?? Guid.NewGuid().ToString();
                    matchingEdgeIds.Add(edgeId);
                }
            }

            // Build the minimal result graph.
            ResultGraphDto resultGraph = new ResultGraphDto
            {
                NodeIds = nodeIds,
                EdgeIds = matchingEdgeIds.ToArray()
            };

            // Package snapshots and the minimal result graph into the final DTO.
            GraphStepDto resultDto = new GraphStepDto
            {
                Steps = snapshot.Steps,
                ResultGraph = resultGraph
            };

            return resultDto;
        }
    }
}
