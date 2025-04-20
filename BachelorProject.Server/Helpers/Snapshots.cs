using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.Helpers
{
    public class Snapshots
    {
        private readonly bool makeSnapshots;

        private readonly NodeDto[] nodes;
        private readonly EdgeDto[] edges;
        private readonly Dictionary<string, string> nodeColors;
        private readonly Dictionary<string, string> edgeColors;
        private readonly Dictionary<string, string> edgeLookup;
        public Dictionary<string, int?> CurrentEdgeWeights { get; private set; }
        int? currentTotalWeight = null;

        public List<Step> Steps { get; private set; }

        public Snapshots(GraphDto graph, bool makeSnapshots)
        {
            this.makeSnapshots = makeSnapshots;
            nodes = graph.Nodes.ToArray();
            edges = graph.Edges.ToArray();
            Steps = new List<Step>();
            nodeColors = new Dictionary<string, string>();
            edgeColors = new Dictionary<string, string>();
            edgeLookup = new Dictionary<string, string>();
            CurrentEdgeWeights = new Dictionary<string, int?>();

            foreach (var node in nodes)
            {
                string nodeId = node.Id.ToString();
                nodeColors[nodeId] = GraphHelpers.COLOR_UNPROCESSED_NODE;
            }

            foreach (var edge in edges)
            {
                string source = edge.SourceNodeId.ToString();
                string target = edge.TargetNodeId.ToString();
                string lookupKey = $"{source}->{target}";
                string edgeId = edge.Id.ToString();
                if (!edgeLookup.ContainsKey(lookupKey))
                {
                    edgeLookup[lookupKey] = edgeId;
                    edgeColors[edgeId] = GraphHelpers.COLOR_UNPROCESSED_EDGE;
                    CurrentEdgeWeights[edgeId] = null;
                }
                if (!graph.IsDirected)
                {
                    source = edge.TargetNodeId.ToString();
                    target = edge.SourceNodeId.ToString();
                    lookupKey = $"{source}->{target}";
                    edgeId = edge.Id.ToString();
                    if (!edgeLookup.ContainsKey(lookupKey))
                    {
                        edgeLookup[lookupKey] = edgeId;
                    }
                }
            }
        }

        public void UpdateCurrentTotalWeight(int currentTotalWeight)
        {
            this.currentTotalWeight = currentTotalWeight;
        }

        public void ColorNode(int nodeIndex, string color)
        {
            if (!makeSnapshots) return;
            if (nodeIndex < 0 || nodeIndex >= nodes.Length)
                throw new ArgumentOutOfRangeException(nameof(nodeIndex));

            string nodeId = nodes[nodeIndex].Id.ToString();

            if (nodeColors.ContainsKey(nodeId))
                nodeColors[nodeId] = color;

            Steps.Add(TakeSnapshot());
        }

        public void ColorEdge(string fromId, string toId, string color)
        {
            if (!makeSnapshots) return;
            string lookupKey = $"{fromId}->{toId}";
            if (edgeLookup.TryGetValue(lookupKey, out string edgeId))
            {
                edgeColors[edgeId] = color;
            }

            Steps.Add(TakeSnapshot());
        }

        public void ColorEdge(int startNodeIndex, int endNodeIndex, string color)
        {
            if (!makeSnapshots) return;
            if (startNodeIndex < 0 || startNodeIndex >= nodes.Length)
                throw new ArgumentOutOfRangeException(nameof(startNodeIndex));
            if (endNodeIndex < 0 || endNodeIndex >= nodes.Length)
                throw new ArgumentOutOfRangeException(nameof(endNodeIndex));

            string fromId = nodes[startNodeIndex].Id.ToString();
            string toId = nodes[endNodeIndex].Id.ToString();
            string lookupKey = $"{fromId}->{toId}";
            if (edgeLookup.TryGetValue(lookupKey, out string edgeId))
            {
                edgeColors[edgeId] = color;
            }

            Steps.Add(TakeSnapshot());
        }

        public void ColorEdge(int startNodeIndex, int endNodeIndex, string color, int currentEdgeWeight)
        {
            if (!makeSnapshots) return;
            if (startNodeIndex < 0 || startNodeIndex >= nodes.Length)
                throw new ArgumentOutOfRangeException(nameof(startNodeIndex));
            if (endNodeIndex < 0 || endNodeIndex >= nodes.Length)
                throw new ArgumentOutOfRangeException(nameof(endNodeIndex));

            string fromId = nodes[startNodeIndex].Id.ToString();
            string toId = nodes[endNodeIndex].Id.ToString();
            string lookupKey = $"{fromId}->{toId}";
            if (edgeLookup.TryGetValue(lookupKey, out string edgeId))
            {
                edgeColors[edgeId] = color;
                CurrentEdgeWeights[edgeId] = currentEdgeWeight;
            }

            Steps.Add(TakeSnapshot());
        }

        public string? GetEdgeId(string fromId, string toId)
        {
            string lookupKey = $"{fromId}->{toId}";
            edgeLookup.TryGetValue(lookupKey, out string? edgeId);
            return edgeId;
        }

        public Step TakeSnapshot()
        {
            var step = new Step
            {
                NodeColors = new Dictionary<string, string>(nodeColors),
                EdgeColors = new Dictionary<string, string>(edgeColors),
                EdgeCurrentWeights = new Dictionary<string, int?>(CurrentEdgeWeights),
                CurrentTotalWeight = currentTotalWeight
            };

            return step;
        }
    }
}
