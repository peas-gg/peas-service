using Microsoft.EntityFrameworkCore;
using PEAS.Entities.Authentication;
using PEAS.Entities.Site;

namespace PEAS.Entities
{
    public class DataContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        public DbSet<Business> Businesses { get; set; }

        public DbSet<Template> Templates { get; set; }

        private readonly IConfiguration Configuration;

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to SqlServer database
            options.UseSqlServer(Configuration.GetConnectionString("Database"));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Account>()
                .HasIndex(x => new { x.Email })
                .IsUnique();

            builder.Entity<Account>()
                .HasIndex(x => new { x.Phone })
                .IsUnique();

            builder.Entity<Business>()
                .HasIndex(x => new { x.Sign })
                .IsUnique();

            builder.Entity<Account>()
                .Property(x => x.Role)
                .HasConversion<string>();

            builder.Entity<Account>().OwnsMany(p => p.Devices, a =>
            {
                a.Property(b => b.DeviceType)
                .HasConversion<string>();
            });
        }
    }
}