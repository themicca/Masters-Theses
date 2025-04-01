using BachelorProject.Server.Models.Domain;
using BachelorProject.Server.Models.DTO;
using System.Drawing;

namespace BachelorProject.Server.Helpers
{
    public class Snapshots
    {
        private readonly NodeDto[] nodes;
        private readonly EdgeDto[] edges;
        // These dictionaries now use node id and edge id (as strings) as keys
        private readonly Dictionary<string, string> nodeColors;
        private readonly Dictionary<string, string> edgeColors;
        // Lookup mapping from a composite key ("source->target") to the edge id
        private readonly Dictionary<string, string> edgeLookup;

        public List<StepState> Steps { get; private set; }

        public Snapshots(NodeDto[] nodes, EdgeDto[] edges)
        {
            this.nodes = nodes;
            this.edges = edges;
            Steps = new List<StepState>();
            nodeColors = new Dictionary<string, string>();
            edgeColors = new Dictionary<string, string>();
            edgeLookup = new Dictionary<string, string>();

            // Initialize nodeColors using the node ids
            foreach (var node in nodes)
            {
                string nodeId = node.Id.ToString();
                nodeColors[nodeId] = Constants.ColorBase;
            }

            // Build the edge lookup and initialize edgeColors using the edge id
            foreach (var edge in edges)
            {
                string source = edge.SourceNodeId.ToString();
                string target = edge.TargetNodeId.ToString();
                string lookupKey = $"{source}->{target}";
                string edgeId = edge.Id.ToString();
                if (!edgeLookup.ContainsKey(lookupKey))
                {
                    edgeLookup[lookupKey] = edgeId;
                }
                if (!edgeColors.ContainsKey(edgeId))
                {
                    edgeColors[edgeId] = Constants.ColorBase;
                }
            }
        }

        // ---------- ADJACENCY MATRIX ----------
        // Looks up an edge by matching node ids from the matrix indices.
        public void InitializeFromAdjacencyMatrix(int[][] matrix)
        {
            int n = nodes.Length;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (matrix[i][j] != 0 && matrix[i][j] < Constants.MaxWeight)
                    {
                        string source = nodes[i].Id.ToString();
                        string target = nodes[j].Id.ToString();
                        string lookupKey = $"{source}->{target}";
                        if (edgeLookup.TryGetValue(lookupKey, out string edgeId))
                        {
                            edgeColors[edgeId] = Constants.ColorBase;
                        }
                    }
                }
            }
        }

        // ---------- ADJACENCY LIST ----------
        // Uses the provided dictionary whose keys/values are node id strings.
        public void InitializeFromAdjacencyList(Dictionary<string, List<(string to, int weight)>> adjList)
        {
            foreach (var from in adjList)
            {
                string source = from.Key;
                foreach (var (to, weight) in from.Value)
                {
                    string lookupKey = $"{source}->{to}";
                    if (edgeLookup.TryGetValue(lookupKey, out string edgeId))
                    {
                        if (!edgeColors.ContainsKey(edgeId))
                            edgeColors[edgeId] = Constants.ColorBase;
                    }
                }
            }
        }

        // ---------- EDGE LIST ----------
        // Since the edge list provides the edge id directly, we can use it as key.
        public void InitializeFromEdgeList(List<(string edgeId, int weight)> edgeList)
        {
            foreach (var (edgeId, weight) in edgeList)
            {
                if (!edgeColors.ContainsKey(edgeId))
                {
                    edgeColors[edgeId] = Constants.ColorBase;
                }
            }
        }

        // Updates the color of a node using its node id.
        public void ColorNode(string nodeId, string color)
        {
            if (nodeColors.ContainsKey(nodeId))
                nodeColors[nodeId] = color;

            Steps.Add(TakeSnapshot());
        }

        public void ColorNode(int nodeIndex, string color)
        {
            if (nodeIndex < 0 || nodeIndex >= nodes.Length)
                throw new ArgumentOutOfRangeException(nameof(nodeIndex));

            string nodeId = nodes[nodeIndex].Id.ToString();
            ColorNode(nodeId, color);
        }

        // Looks up the corresponding edge id from the two node ids and updates its color.
        public void ColorEdge(string fromId, string toId, string color)
        {
            string lookupKey = $"{fromId}->{toId}";
            if (edgeLookup.TryGetValue(lookupKey, out string edgeId))
            {
                if (edgeColors.ContainsKey(edgeId))
                    edgeColors[edgeId] = color;
            }

            Steps.Add(TakeSnapshot());
        }

        public void ColorEdge(int edgeIndex, string color)
        {
            if (edgeIndex < 0 || edgeIndex >= edges.Length)
                throw new ArgumentOutOfRangeException(nameof(edgeIndex));

            string edgeId = edges[edgeIndex].Id.ToString();
            if (edgeColors.ContainsKey(edgeId))
                edgeColors[edgeId] = color;

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
                if (edgeColors.ContainsKey(edgeId))
                    edgeColors[edgeId] = color;
            }

            Steps.Add(TakeSnapshot());
        }

        public string? GetEdgeId(string fromId, string toId)
        {
            string lookupKey = $"{fromId}->{toId}";
            edgeLookup.TryGetValue(lookupKey, out string? edgeId);
            return edgeId;
        }

        // Constructs a snapshot from the current node and edge colors.
        public StepState TakeSnapshot()
        {
            var step = new StepState
            {
                // Create a copy so that the snapshot is independent of future changes
                NodeColors = new Dictionary<string, string>(nodeColors),
                EdgeColors = new Dictionary<string, string>(edgeColors)
            };

            return step;
        }
    }
}
