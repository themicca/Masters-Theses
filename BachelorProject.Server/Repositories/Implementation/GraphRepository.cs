using BachelorProject.Server.Data;
using BachelorProject.Server.Models.Domain;
using BachelorProject.Server.Repositories.Interface;

namespace BachelorProject.Server.Repositories.Implementation
{
    public class GraphRepository : IGraphRepository
    {
        private readonly ApplicationDbContext dbContext;

        public GraphRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<GraphModel> CreateAsync(GraphModel graph)
        {
            await dbContext.GraphModels.AddAsync(graph);
            await dbContext.SaveChangesAsync();

            return graph;
        }
    }
}
