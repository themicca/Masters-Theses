using BachelorProject.Server.GraphAlgorithms.MaxMatching;
using BachelorProject.Server.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BachelorProject.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GreedyMatchingController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> RunAlgo(CreateGraphRequestDto request)
        {
            try
            {
                GraphStepDto result = GreedyMatchingAlgo.SolveGraph(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
