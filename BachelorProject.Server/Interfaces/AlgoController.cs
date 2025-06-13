using BachelorProject.Server.GraphAlgorithms.ShortestPath;
using BachelorProject.Server.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BachelorProject.Server.Interfaces
{
    public interface AlgoController
    {
        public Task<IActionResult> RunAlgo(GraphDto request);
    }
}
