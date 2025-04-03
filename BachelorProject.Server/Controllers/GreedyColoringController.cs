using BachelorProject.Server.GraphAlgorithms.EdgeColoring;
using BachelorProject.Server.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BachelorProject.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GreedyColoringController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> RunAlgo(GraphDto request)
        {
            try
            {
                GraphStepDto result = GreedyColoringAlgo.SolveGraph(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
