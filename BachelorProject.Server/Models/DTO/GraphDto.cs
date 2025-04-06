namespace BachelorProject.Server.Models.DTO
{
    public class GraphDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<NodeDto> Nodes { get; set; } = new();
        public List<EdgeDto> Edges { get; set; } = new();
        public Guid? Src { get; set; }
        public Guid? Target { get; set; }
        public bool IsDirected { get; set; }
        public bool IsWeighted { get; set; }
    }
}
