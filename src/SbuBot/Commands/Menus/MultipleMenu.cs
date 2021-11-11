using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Gateway;
using Disqord.Rest;

namespace SbuBot.Commands.Menus
{
    public class MultipleMenu : MenuBase
    {
        public HashSet<Snowflake> AuthorIds { get; }

        public MultipleMenu(ViewBase view, IEnumerable<Snowflake> authorIds) : base(view)
            => AuthorIds = authorIds.ToHashSet();

        protected override async ValueTask<Snowflake> InitializeAsync(CancellationToken cancellationToken)
        {
            ValidateView();
            await View.UpdateAsync();

            if (MessageId != default)
                return MessageId;

            IUserMessage userMessage = await Client.SendMessageAsync(
                ChannelId,
                View.ToLocalMessage(),
                cancellationToken: cancellationToken
            );

            return userMessage.Id;
        }

        protected override ValueTask<bool> CheckInteractionAsync(InteractionReceivedEventArgs e)
            => ValueTask.FromResult(AuthorIds.Contains(e.AuthorId));
    }
}