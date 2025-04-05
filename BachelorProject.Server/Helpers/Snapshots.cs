using BachelorProject.Server.Models.Domain;
using BachelorProject.Server.Models.DTO;
using System.Drawing;

namespace BachelorProject.Server.Helpers
{
    public class Snapshots
    {
        private readonly NodeDto[] nodes;
        private readonly EdgeDto[] edges;
        private readonly Dictionary<string, string> nodeColors;
        private readonly Dictionary<string, string> edgeColors;
        private readonly Dictionary<string, string> edgeLookup;
        private readonly Dictionary<string, int?> currentEdgeWeights;
        int currentTotalWeight = 0;

        public List<StepState> Steps { get; private set; }

        public Snapshots(GraphDto graph)
        {
            nodes = graph.Nodes.ToArray();
            edges = graph.Edges.ToArray();
            Steps = new List<StepState>();
            nodeColors = new Dictionary<string, string>();
            edgeColors = new Dictionary<string, string>();
            edgeLookup = new Dictionary<string, string>();
            currentEdgeWeights = new Dictionary<string, int?>();

            foreach (var node in nodes)
            {
                string nodeId = node.Id.ToString();
                nodeColors[nodeId] = Constants.ColorBase;
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
                    edgeColors[edgeId] = Constants.ColorBase;
                    currentEdgeWeights[edgeId] = null;
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
            if (nodeIndex < 0 || nodeIndex >= nodes.Length)
                throw new ArgumentOutOfRangeException(nameof(nodeIndex));

            string nodeId = nodes[nodeIndex].Id.ToString();

            if (nodeColors.ContainsKey(nodeId))
                nodeColors[nodeId] = color;

            Steps.Add(TakeSnapshot());
        }

        public void ColorEdge(string fromId, string toId, string color)
        {
            string lookupKey = $"{fromId}->{toId}";
            if (edgeLookup.TryGetValue(lookupKey, out string edgeId))
            {
                edgeColors[edgeId] = color;
            }

            Steps.Add(TakeSnapshot());
        }

        public void ColorEdge(int startNodeIndex, int endNodeIndex, string color)
        {
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
                currentEdgeWeights[edgeId] = currentEdgeWeight;
            }

            Steps.Add(TakeSnapshot());
        }

        public string? GetEdgeId(string fromId, string toId)
        {
            string lookupKey = $"{fromId}->{toId}";
            edgeLookup.TryGetValue(lookupKey, out string? edgeId);
            return edgeId;
        }

        public StepState TakeSnapshot()
        {
            var step = new StepState
            {
                NodeColors = new Dictionary<string, string>(nodeColors),
                EdgeColors = new Dictionary<string, string>(edgeColors),
                EdgeCurrentWeights = new Dictionary<string, int?>(currentEdgeWeights),
                CurrentTotalWeight = currentTotalWeight
            };

            return step;
        }
    }
}
