using BachelorProject.Server.GraphAlgorithms.ShortestPath;
using BachelorProject.Server.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BachelorProject.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EdmondsKarpController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> RunAlgo(GraphDto request)
        {
            try
            {
                GraphStepDto result = EdmondsKarpAlgo.SolveGraph(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
