namespace BachelorProject.Server.Models.DTO
{
    public class ResultGraphDto
    {
        public string[] NodeIds {  get; set; }
        public string[] EdgeIds { get; set; }
        public int? TotalWeight { get; set; }
        public Dictionary<string, int?> EdgeResultWeights { get; set; } = [];
        public string? EulerType { get; set; } = null;
        public string AlgoType { get; set; } = string.Empty;
    }
}
