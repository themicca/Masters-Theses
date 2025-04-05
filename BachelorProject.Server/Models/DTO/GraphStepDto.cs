namespace BachelorProject.Server.Models.DTO
{
    public class GraphStepDto
    {
        public List<StepState> Steps { get; set; }
        public ResultGraphDto ResultGraph { get; set; }
    }

    public class StepState
    {
        public Dictionary<string, string> NodeColors { get; set; } = [];
        public Dictionary<string, string> EdgeColors { get; set; } = [];
        public Dictionary<string, int?> EdgeCurrentWeights { get; set; } = [];
        public int CurrentTotalWeight { get; set; }
    }
}
