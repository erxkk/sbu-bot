using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

namespace SbuBot.Commands
{
    public class DiscordPrivateResponseCommandResult : DiscordCommandResult
    {
        public LocalMessage Message { get; }

        public DiscordPrivateResponseCommandResult(DiscordCommandContext context, LocalMessage message) : base(context)
            => Message = message;

        public override Task ExecuteAsync() => Context.Author.SendMessageAsync(Message);
    }
}