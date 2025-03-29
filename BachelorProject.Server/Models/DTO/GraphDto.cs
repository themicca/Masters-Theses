namespace BachelorProject.Server.Models.DTO
{
    public class GraphDto
    {
        public Guid Id { get; set; }

        public string GraphName { get; set; }

        public string[] GraphNodes { get; set; }

        public int[][] GraphEdges { get; set; }

        public string GraphSrc { get; set; }

        public string GraphTarget { get; set; }
    }
}
