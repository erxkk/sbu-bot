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
            IsProduction = string.Equals(Environment, "Production", StringComparison.OrdinalIgnoreCase);

            DbConnectionString = string.Format(
                "Host={0};Database={1};Username={2};Password={3};Port={4};Include Error Detail={5};",
                configuration["Psql_Host"],
                configuration["Psql_Database"],
                configuration["Psql_Username"],
                configuration["Psql_Password"],
                configuration["Psql_Port"],
                !IsProduction
            );
        }
    }
}