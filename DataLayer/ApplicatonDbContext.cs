using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace DataLayer
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TestAttempt> TestAttempts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Здесь можно добавить дополнительные настройки модели
        }
    }

    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql("postgresql://mathdb_lg7k_user:RkNo3LAN7nH1dq375yYYBVQxWlVotqWc@dpg-cva9vqt2ng1s73c14dfg-a.oregon-postgres.render.com:5432/mathdb_lg7k",
                b => b.MigrationsAssembly("DataLayer"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}