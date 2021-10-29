using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SbuBot
{
    public sealed class SbuConfiguration
    {
        private readonly IConfiguration _configuration;
        public bool IsProduction { get; }
        public string DbConnectionString { get; }
        public string this[string key] { get => _configuration[key]; set => _configuration[key] = value; }

        public SbuConfiguration(
            ILogger<SbuConfiguration> logger,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment
        )
        {
            _configuration = configuration;
            IsProduction = hostEnvironment.IsProduction();

            string? db = configuration["Postgres:Database"];
            string? user = configuration["Postgres:Username"];

            DbConnectionString = string.Format(
                "Host={0};Port={1};Database={2};Username={3};Password={4};Include Error Detail={5};",
                configuration["Postgres:Host"],
                configuration["Postgres:Port"],
                db,
                user,
                configuration["Postgres:Password"],
                configuration["Postgres:DetailedErrors"] ?? (!IsProduction).ToString()
            );

            logger.LogInformation("Using Database: {@Database}", new { Db = db, User = user });
        }

        internal SbuConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
            IsProduction = false;

            DbConnectionString = string.Format(
                "Host={0};Port={1};Database={2};Username={3};Password={4};Include Error Detail={5};",
                configuration["Postgres:Host"],
                configuration["Postgres:Port"],
                configuration["Postgres:Database"],
                configuration["Postgres:Username"],
                configuration["Postgres:Password"],
                "true"
            );
        }
    }
}