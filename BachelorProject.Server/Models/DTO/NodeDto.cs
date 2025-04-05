namespace BachelorProject.Server.Models.DTO
{
    public class NodeDto
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public float X { get; set; }
        public float Y { get; set; }
        public bool? IsStart { get; set; }
        public bool? IsEnd { get; set; }
    }
}
