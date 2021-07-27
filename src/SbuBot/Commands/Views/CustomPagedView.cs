using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace SbuBot.Commands.Views
{
    public class CustomPagedView : PagedView
    {
        public CustomPagedView(PageProvider pageProvider, LocalMessage? templateMessage = null)
            : base(pageProvider, templateMessage)
        {
            FirstPageButton.Emoji = LocalEmoji.Custom(SbuGlobals.Guild.Emote.Menu.FAST_BACK);
            PreviousPageButton.Emoji = LocalEmoji.Custom(SbuGlobals.Guild.Emote.Menu.BACK);
            NextPageButton.Emoji = LocalEmoji.Custom(SbuGlobals.Guild.Emote.Menu.FORWARD);
            LastPageButton.Emoji = LocalEmoji.Custom(SbuGlobals.Guild.Emote.Menu.FAST_FORWARD);
            StopButton.Emoji = LocalEmoji.Custom(SbuGlobals.Guild.Emote.Menu.STOP);
        }
    }
}