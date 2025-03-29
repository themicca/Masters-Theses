namespace BachelorProject.Server.Models.DTO
{
    public class GraphStepDto
    {
        public List<StepState> Steps { get; set; }
        public CreateGraphRequestDto FinalGraph { get; set; }
    }

    public class StepState
    {
        public Dictionary<string, string> NodeColors { get; set; } = new Dictionary<string, string>();
        public List<EdgeState> Edges { get; set; } = new List<EdgeState>();
    }

    public class EdgeState
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public string Color { get; set; }
    }
}
