using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.MaxMatching
{
    public class GreedyMatchingAlgo
    {
        public static GraphStepDto SolveGraph(GraphDto graph)
        {
            string[] nodeIds = GraphDtoConvertor.ToNodeIdArray(graph);
            int n = nodeIds.Length;
            int[][] edges = GraphDtoConvertor.ToAdjacencyMatrix(graph);
            Dictionary<string, string> edgeLookup = GraphDtoConvertor.EdgeLookup(graph);

            int[] match = new int[n];
            for (int i = 0; i < n; i++)
                match[i] = -1;

            List<string> matchingEdgeIds = new List<string>();
            Snapshots snapshot = new Snapshots(graph);

            for (int i = 0; i < n; i++)
            {
                if (match[i] != -1)
                    continue;

                for (int j = i + 1; j < n; j++)
                {
                    if (match[j] != -1)
                        continue;

                    if (edges[i][j] != 0 && edges[i][j] < Constants.MaxWeight)
                    {
                        snapshot.ColorEdge(i, j, Constants.ColorProcessing);

                        match[i] = j;
                        match[j] = i;

                        snapshot.ColorNode(i, Constants.ColorResult);
                        snapshot.ColorNode(j, Constants.ColorResult);
                        snapshot.ColorEdge(i, j, Constants.ColorResult);

                        break;
                    }
                }
            }

            for (int i = 0; i < n; i++)
            {
                if (match[i] != -1 && i < match[i])
                {
                    string key = $"{nodeIds[i]}->{nodeIds[match[i]]}";
                    string edgeId = edgeLookup.ContainsKey(key) ? edgeLookup[key] : Guid.NewGuid().ToString();
                    matchingEdgeIds.Add(edgeId);
                }
            }

            ResultGraphDto resultGraph = new ResultGraphDto
            {
                NodeIds = nodeIds,
                EdgeIds = matchingEdgeIds.ToArray(),
                GraphType = Constants.GraphTypes.GreedyMatching
            };

            GraphStepDto resultDto = new GraphStepDto
            {
                Steps = snapshot.Steps,
                ResultGraph = resultGraph
            };

            return resultDto;
        }
    }
}
