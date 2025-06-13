using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.Interfaces
{
    public interface AlgoBase
    {
        public static abstract GraphStepsResultDto SolveGraph(GraphDto graph, bool makeSnapshots);
    }
}
