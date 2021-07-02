using Disqord.Bot.Hosting;

namespace SbuBot.Services
{
    public abstract class SbuBotServiceBase : DiscordBotService
    {
        protected SbuConfiguration Configuration { get; }

        protected SbuBotServiceBase(SbuConfiguration configuration) => Configuration = configuration;
    }
}