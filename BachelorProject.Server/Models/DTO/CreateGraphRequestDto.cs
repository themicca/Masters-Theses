namespace BachelorProject.Server.Models.DTO
{
    public class CreateGraphRequestDto
    {
        public string GraphName { get; set; }

        public string[] GraphNodes { get; set; }

        public int[][] GraphEdges { get; set; }

        public string? GraphSrc { get; set; }

        public string? GraphTarget { get; set; }

        public bool GraphDirected { get; set; }
        public List<NodePosition> GraphNodePositions { get; set; }
    }

    public class NodePosition
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
