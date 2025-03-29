using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.Helpers
{
    public class Snapshots
    {
        private string[] nodes;
        private int[][] edges;
        private int nodesCount;
        private string[] nodeColors;
        private Dictionary<string, string> edgeColors;

        public List<StepState> Steps { get; private set; }

        public Snapshots(CreateGraphRequestDto graph)
        {
            nodes = graph.GraphNodes;
            edges = graph.GraphEdges;
            nodesCount = graph.GraphNodes.Length;

            Steps = [];
            nodeColors = new string[nodesCount];
            for (int i = 0; i < nodesCount; i++)
            {
                nodeColors[i] = Constants.ColorBase;
            }

            edgeColors = [];

            for (int i = 0; i < nodesCount; i++)
            {
                for (int j = 0; j < nodesCount; j++)
                {
                    if (edges[i][j] != 0 && edges[i][j] < Constants.MaxWeight && i != j)
                    {
                        string key = $"{i}->{j}";
                        edgeColors[key] = Constants.ColorBase;
                    }
                }
            }
        }

        public void ColorEdge(int startIndex, int endIndex, string color)
        {
            string uvKey = $"{startIndex}->{endIndex}";
            if (edgeColors.ContainsKey(uvKey))
            {
                edgeColors[uvKey] = color;
                Steps.Add(TakeSnapshot());
            }
        }

        public void ColorNode(int nodeIndex, string color)
        {
            nodeColors[nodeIndex] = color;

            Steps.Add(TakeSnapshot());
        }

        public StepState TakeSnapshot()
        {
            var step = new StepState();

            for (int i = 0; i < nodes.Length; i++)
            {
                step.NodeColors[nodes[i]] = nodeColors[i];
            }

            foreach (var kvp in edgeColors)
            {
                string key = kvp.Key;
                string color = kvp.Value;

                var parts = key.Split("->");
                int uIndex = int.Parse(parts[0]);
                int vIndex = int.Parse(parts[1]);

                var edgeState = new EdgeState
                {
                    Source = nodes[uIndex],
                    Target = nodes[vIndex],
                    Color = color
                };
                step.Edges.Add(edgeState);
            }

            return step;
        }
    }
}
