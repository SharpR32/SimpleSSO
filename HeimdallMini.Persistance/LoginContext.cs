using HeimdallMini.Domain.Entities;
using HeimdallMini.Domain.Entities.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HeimdallMini.Persistance
{
    public class LoginContext : DbContext
    {
        protected readonly IConfiguration _configuration;
        public LoginContext(IConfiguration configuration)
            => _configuration = configuration;

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("Default"));

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(x => x.UserName);

            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            PerformAudit();
            return base.SaveChangesAsync(cancellationToken);
        }

        protected void PerformAudit()
        {
            var entries = ChangeTracker.Entries<AuditableEntity>().Where(x => x.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                entry.Entity.SetUpdateTime();
            }
        }

        public static IApplicationBuilder InitiateUserDatabase(IApplicationBuilder builder)
        {
            using var scope = builder.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LoginContext>();
            dbContext.Database.Migrate();
            return builder;
        }
    }
}