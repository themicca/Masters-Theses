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

        public static string[] ToNodeIdArray(GraphDto graph)
        {
            string[] nodes = new string[graph.Nodes.Count];
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                nodes[i] = graph.Nodes[i].Id.ToString();
            }
            return nodes;
        }

        public static string[] ToEdgeIdArray(GraphDto graph)
        {
            string[] edges = new string[graph.Edges.Count];
            for (int i = 0; i < graph.Edges.Count; i++)
            {
                edges[i] = graph.Edges[i].Id.ToString();
            }
            return edges;
        }

        public static Dictionary<string, string> EdgeLookup(GraphDto graph)
        {
            Dictionary<string, string> edgeLookup = new Dictionary<string, string>();
            foreach (var edge in graph.Edges)
            {
                string key = $"{edge.SourceNodeId}->{edge.TargetNodeId}";
                if (!edgeLookup.ContainsKey(key))
                    edgeLookup[key] = edge.Id.ToString();
                if (!graph.IsDirected)
                {
                    key = $"{edge.TargetNodeId}->{edge.SourceNodeId}";
                    if (!edgeLookup.ContainsKey(key))
                        edgeLookup[key] = edge.Id.ToString();
                }
            }
            return edgeLookup;
        }
    }
}
