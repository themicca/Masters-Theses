namespace BachelorProject.Server.Models.DTO
{
    public class GraphStepDto
    {
        public List<StepState> Steps { get; set; }
        public GraphDto FinalGraph { get; set; }
    }

    public class StepState
    {
        public Dictionary<string, string> NodeColors { get; set; } = [];
        public List<EdgeState> Edges { get; set; } = [];
    }

    public class EdgeState
    {
        public Guid EdgeId { get; set; }
        public string Color { get; set; }
    }
}
