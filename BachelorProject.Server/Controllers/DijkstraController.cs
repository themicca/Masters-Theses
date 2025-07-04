﻿using BachelorProject.Server.GraphAlgorithms.ShortestPath;
using BachelorProject.Server.Models.DTO;
using BachelorProject.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace BachelorProject.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DijkstraController : ControllerBase, AlgoController
    {
        [HttpPost]
        public async Task<IActionResult> RunAlgo(GraphDto request)
        {
            try
            {
                GraphStepsResultDto result = await Task.Run(() => DijkstraAlgo.SolveGraph(request, true));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
