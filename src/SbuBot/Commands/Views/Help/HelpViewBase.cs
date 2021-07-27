using System.Threading.Tasks;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;

namespace SbuBot.Commands.Views.Help
{
    // TODO: add permissions and checks
    public abstract class HelpViewBase : ViewBase
    {
        protected HelpViewBase(bool hasNoParent = false) : base(new LocalMessage().WithEmbeds(new LocalEmbed()))
        {
            AddComponent(
                new ButtonViewComponent(GoToParent)
                {
                    Emoji = LocalEmoji.Custom(SbuGlobals.Emote.Menu.BACK),
                    Style = LocalButtonComponentStyle.Secondary,
                    Row = 4,
                    Position = 0,
                    IsDisabled = hasNoParent,
                }
            );
        }

        public virtual ValueTask GoToParent(ButtonEventArgs e) => default;

        [Button(Emoji = SbuGlobals.Emote.Menu.STOP, Row = 4, Position = 1, Style = LocalButtonComponentStyle.Secondary)]
        public ValueTask StopMenu(ButtonEventArgs e)
        {
            if (Menu is InteractiveMenu menu)
                menu.Message.DeleteAsync();

            Menu.Stop();
            return default;
        }
    }
}