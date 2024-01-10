using Microsoft.EntityFrameworkCore;
using SampleWebApp.Models;

namespace SampleWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Todo> Todos => Set<Todo>();
    }
}
