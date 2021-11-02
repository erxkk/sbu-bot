using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;

namespace SbuBot.Commands.Views.Help
{
    public abstract class HelpViewBase : ViewBase
    {
        protected DiscordGuildCommandContext Context { get; }

        protected HelpViewBase(DiscordGuildCommandContext context, bool hasNoParent = false)
            : base(new LocalMessage().WithEmbeds(new LocalEmbed()))
        {
            Context = context;

            AddComponent(
                new ButtonViewComponent(GoToParentAsync)
                {
                    Emoji = LocalEmoji.Custom(SbuGlobals.Emote.Menu.BACK),
                    Style = LocalButtonComponentStyle.Secondary,
                    Row = 4,
                    Position = 0,
                    IsDisabled = hasNoParent,
                }
            );
        }

        public virtual ValueTask GoToParentAsync(ButtonEventArgs e) => default;

        [Button(Emoji = SbuGlobals.Emote.Menu.STOP, Row = 4, Position = 1, Style = LocalButtonComponentStyle.Secondary)]
        public ValueTask StopMenuAsync(ButtonEventArgs e)
        {
            if (Menu is DefaultMenu menu)
                menu.Message.DeleteAsync();

            Menu.Stop();
            return default;
        }
    }
}