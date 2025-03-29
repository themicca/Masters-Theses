using BachelorProject.Server.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace BachelorProject.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<GraphModel> GraphModels { get; set; }
    }
}
