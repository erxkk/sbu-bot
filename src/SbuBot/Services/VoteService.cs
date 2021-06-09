using System.Threading.Tasks;

using Disqord.Bot;
using Disqord.Bot.Hosting;
using Disqord.Gateway;

using Microsoft.Extensions.Logging;

namespace SbuBot.Services
{
    // TODO: handle events in `The Senate`
    public sealed class VoteService : SbuBotServiceBase
    {
        public VoteService(ILogger<VoteService> logger, DiscordBotBase bot) : base(logger, bot) { }

        protected override ValueTask OnMessageReceived(BotMessageReceivedEventArgs e) => base.OnMessageReceived(e);

        protected override ValueTask OnReactionAdded(ReactionAddedEventArgs e) => base.OnReactionAdded(e);

        protected override ValueTask OnReactionRemoved(ReactionRemovedEventArgs e) => base.OnReactionRemoved(e);

        protected override ValueTask OnReactionsCleared(ReactionsClearedEventArgs e) => base.OnReactionsCleared(e);
    }
}