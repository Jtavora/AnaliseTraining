using Microsoft.EntityFrameworkCore;
using treinamento_estagiarios.Models;

namespace treinamento_estagiarios.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet para cada model (representa uma tabela no banco)
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}
