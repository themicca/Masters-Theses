using BachelorProject.Server.Data;
using BachelorProject.Server.Models.Domain;
using BachelorProject.Server.Models.DTO;
using BachelorProject.Server.Repositories.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BachelorProject.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraphsController : ControllerBase
    {
        private readonly IGraphRepository graphRepository;

        public GraphsController(IGraphRepository graphRepository)
        {
            this.graphRepository = graphRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGraph(CreateGraphRequestDto request)
        {
            var graph = new GraphModel
            {
                Graph = request.GraphName,
            };

            await graphRepository.CreateAsync(graph);

            var response = new GraphDto
            {
                Id = graph.Id,
                GraphName = graph.Graph,
            };

            return Ok(response);
        }
    }
}
