using Microsoft.EntityFrameworkCore;

namespace PEAS.Entities
{
    public class DataContext : DbContext
    {
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
    }
}