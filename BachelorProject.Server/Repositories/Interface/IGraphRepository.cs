using BachelorProject.Server.Models.Domain;

namespace BachelorProject.Server.Repositories.Interface
{
    public interface IGraphRepository
    {
        Task<GraphModel> CreateAsync(GraphModel graph);
    }
}
