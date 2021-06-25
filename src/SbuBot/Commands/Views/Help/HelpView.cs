using System.Threading.Tasks;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;

namespace SbuBot.Commands.Views.Help
{
    public abstract class HelpView : ViewBase
    {
        protected HelpView(bool hasNoParent = false) : base(new LocalMessage().WithEmbeds(new LocalEmbed()))
        {
            AddComponent(
                new ButtonViewComponent(GoToParent)
                {
                    Label = "⮝",
                    Style = ButtonComponentStyle.Secondary,
                    Row = 4,
                    Position = 0,
                    IsDisabled = hasNoParent,
                }
            );
        }

        public virtual ValueTask GoToParent(ButtonEventArgs e) => default;
    }
}