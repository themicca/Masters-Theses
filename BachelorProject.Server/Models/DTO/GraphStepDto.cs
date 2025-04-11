namespace BachelorProject.Server.Models.DTO
{
    public class GraphStepDto
    {
        public List<Step> Steps { get; set; }
        public ResultGraphDto ResultGraph { get; set; }
    }

    public class Step
    {
        public Dictionary<string, string> NodeColors { get; set; } = [];
        public Dictionary<string, string> EdgeColors { get; set; } = [];
        public Dictionary<string, int?> EdgeCurrentWeights { get; set; } = [];
        public int? CurrentTotalWeight { get; set; }
    }
}
