using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.Helpers
{
    public class GraphDtoConvertor
    {
        public static int[][] ToAdjacencyMatrix(GraphDto graph)
        {
            int n = graph.Nodes.Count;
            var indexMap = graph.Nodes.Select((node, index) => new { node.Id, Index = index })
                                .ToDictionary(x => x.Id, x => x.Index);

            int[][] matrix = new int[n][];
            for (int i = 0; i < n; i++)
            {
                matrix[i] = new int[n];
            }

            foreach (var edge in graph.Edges)
            {
                if (indexMap.TryGetValue(edge.SourceNodeId, out int i) &&
                    indexMap.TryGetValue(edge.TargetNodeId, out int j))
                {
                    matrix[i][j] = edge.Weight;

                    if (!graph.IsDirected)
                    {
                        matrix[j][i] = edge.Weight;
                    }
                }
            }

            return matrix;
        }

        // ---------- ADJACENCY LIST ----------
        public static Dictionary<string, List<(string to, int weight)>> ToAdjacencyList(GraphDto graph)
        {
            var adjList = graph.Nodes.ToDictionary(n => n.Id.ToString(), _ => new List<(string, int)>());

            foreach (var edge in graph.Edges)
            {
                adjList[edge.SourceNodeId.ToString()].Add((edge.TargetNodeId.ToString(), edge.Weight));
                if (!graph.IsDirected)
                    adjList[edge.TargetNodeId.ToString()].Add((edge.SourceNodeId.ToString(), edge.Weight));
            }

            return adjList;
        }

        // ---------- EDGE LIST ----------
        public static List<(string edgeId, string sourceId, string targetId, int weight)> ToEdgeListDetailed(GraphDto graph)
        {
            var edges = new List<(string, string, string, int)>();
            foreach (var e in graph.Edges)
            {
                edges.Add((
                    e.Id.ToString(),
                    e.SourceNodeId.ToString(),
                    e.TargetNodeId.ToString(),
                    e.Weight
                ));
            }
            return edges;
        }

        // ---------- NODE ID TO NODE MAP ----------
        public static string[] ToNodeIdArray(GraphDto graph)
        {
            string[] nodes = new string[graph.Nodes.Count];
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                nodes[i] = graph.Nodes[i].Id.ToString();
            }
            return nodes;
        }
    }
}
