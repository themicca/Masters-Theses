namespace BachelorProject.Server.Models.DTO
{
    public class EdgeDto
    {
        public Guid Id { get; set; }
        public Guid SourceNodeId { get; set; }
        public Guid TargetNodeId { get; set; }
        public int Weight { get; set; }
    }

}
