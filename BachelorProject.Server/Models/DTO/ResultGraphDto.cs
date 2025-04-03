namespace BachelorProject.Server.Models.DTO
{
    public class ResultGraphDto
    {
        public string[] NodeIds {  get; set; }
        public string[] EdgeIds { get; set; }
        public int? TotalWeight { get; set; }
    }
}
