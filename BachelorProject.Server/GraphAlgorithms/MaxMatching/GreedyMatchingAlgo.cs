using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.MaxMatching
{
    public class GreedyMatchingAlgo
    {
        public static GraphStepsResultDto SolveGraph(GraphDto graph, bool makeSnapshots)
        {
            string[] nodeIds = GraphDtoConvertor.ToNodeIdArray(graph);
            int n = nodeIds.Length;
            int[][] edges = GraphDtoConvertor.ToAdjacencyMatrix(graph);
            Dictionary<string, string> edgeLookup = GraphDtoConvertor.EdgeLookup(graph);

            int[] match = new int[n];
            for (int i = 0; i < n; i++)
                match[i] = -1;

            List<string> matchingEdgeIds = new List<string>();
            Snapshots snapshot = new Snapshots(graph, makeSnapshots);

            for (int i = 0; i < n; i++)
            {
                if (match[i] != -1)
                    continue;

                for (int j = i + 1; j < n; j++)
                {
                    if (match[j] != -1)
                        continue;

                    if (edges[i][j] != 0 && edges[i][j] < GraphHelpers.MAX_WEIGHT)
                    {
                        snapshot.ColorEdge(i, j, GraphHelpers.COLOR_PROCESSING);

                        match[i] = j;
                        match[j] = i;

                        snapshot.ColorNode(i, GraphHelpers.COLOR_RESULT);
                        snapshot.ColorNode(j, GraphHelpers.COLOR_RESULT);
                        snapshot.ColorEdge(i, j, GraphHelpers.COLOR_RESULT);

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

            GraphResultDto resultGraph = new GraphResultDto
            {
                NodeIds = nodeIds,
                EdgeIds = matchingEdgeIds.ToArray(),
                AlgoType = GraphHelpers.AlgoTypes.GREEDY_MATCHING
            };

            GraphStepsResultDto resultDto = new GraphStepsResultDto
            {
                Steps = snapshot.Steps,
                GraphResult = resultGraph
            };

            return resultDto;
        }
    }
}
