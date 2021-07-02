using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SbuBot
{
    public sealed class SbuConfiguration
    {
        private readonly IConfiguration _configuration;
        public bool IsProduction { get; }
        public string DbConnectionString { get; }
        public string this[string key] { get => _configuration[key]; set => _configuration[key] = value; }

        public SbuConfiguration(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            _configuration = configuration;
            IsProduction = hostEnvironment.IsProduction();

            DbConnectionString = string.Format(
                "Host={0};Database={1};Username={2};Password={3};Port={4};Include Error Detail={5};",
                configuration["Postgres:Host"],
                configuration["Postgres:Database"],
                configuration["Postgres:Username"],
                configuration["Postgres:Password"],
                configuration["Postgres:Port"],
                configuration["Postgres:DetailedErrors"] ?? (!IsProduction).ToString()
            );
        }
    }
}