using System;

using Disqord;
using Disqord.Bot;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SbuBot
{
    public sealed class SbuBot : DiscordBot
    {
        public SbuBot(
            IOptions<DiscordBotConfiguration> options,
            ILogger<SbuBot> logger,
            IServiceProvider services,
            DiscordClient client
        ) : base(options, logger, services, client) { }
    }
}