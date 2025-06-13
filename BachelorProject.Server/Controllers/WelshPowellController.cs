using BachelorProject.Server.GraphAlgorithms.NodeColoring;
using BachelorProject.Server.Interfaces;
using BachelorProject.Server.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BachelorProject.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WelshPowellController : ControllerBase, AlgoController
    {
        [HttpPost]
        public async Task<IActionResult> RunAlgo(GraphDto request)
        {
            try
            {
                GraphStepsResultDto result = await Task.Run(() => WelshPowellAlgo.SolveGraph(request, true));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
