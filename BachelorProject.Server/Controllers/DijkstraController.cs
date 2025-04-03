using BachelorProject.Server.GraphAlgorithms.ShortestPath;
using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.Domain;
using BachelorProject.Server.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace BachelorProject.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DijkstraController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> RunAlgo(GraphDto request)
        {
            try
            {
                GraphStepDto result = DijkstraAlgo.SolveGraph(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
