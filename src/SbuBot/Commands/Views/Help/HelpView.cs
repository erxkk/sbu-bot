using System.Threading.Tasks;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;

namespace SbuBot.Commands.Views.Help
{
    public abstract class HelpView : ViewBase
    {
        protected HelpView(bool hasNoParent = false) : base(new LocalMessage().WithEmbeds(new LocalEmbed()))
        {
            AddComponent(
                new ButtonViewComponent(GoToParent)
                {
                    Emoji = LocalEmoji.Unicode("◀️"),
                    Style = LocalButtonComponentStyle.Secondary,
                    Row = 4,
                    Position = 0,
                    IsDisabled = hasNoParent,
                }
            );
        }

        public virtual ValueTask GoToParent(ButtonEventArgs e) => default;

        [Button(Emoji = "⏹️", Row = 4, Position = 1, Style = LocalButtonComponentStyle.Secondary)]
        public ValueTask StopMenu(ButtonEventArgs e)
        {
            if (Menu is InteractiveMenu menu)
                menu.Message.DeleteAsync();

            return Menu.StopAsync();
        }
    }
}