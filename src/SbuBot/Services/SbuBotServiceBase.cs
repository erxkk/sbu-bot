using Disqord.Bot;
using Disqord.Bot.Hosting;

using Microsoft.Extensions.Logging;

namespace SbuBot.Services
{
    public abstract class SbuBotServiceBase : DiscordBotService
    {
        public override SbuBot Bot => (base.Bot as SbuBot)!;
        protected SbuBotServiceBase(ILogger logger, DiscordBotBase bot) : base(logger, bot) { }
    }
}