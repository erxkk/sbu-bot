using System;

using Microsoft.Extensions.Configuration;

namespace SbuBot
{
    public sealed class SbuBotConfiguration
    {
        public string Environment { get; }
        public bool IsProduction { get; }
        public string DbConnectionString { get; }

        public SbuBotConfiguration(IConfiguration configuration)
        {
            Environment = configuration["environment"];

            IsProduction = string.Equals(
                configuration["environment"],
                "Production",
                StringComparison.OrdinalIgnoreCase
            );

            DbConnectionString = string.Format(
                "Host={0};Database={1};Username={2};Password={3};Port={4};",
                configuration["Postgres:Host"],
                configuration["Postgres:Database"],
                configuration["Postgres:Username"],
                configuration["Postgres:Password"],
                configuration["Postgres:Port"]
            );
        }
    }
}