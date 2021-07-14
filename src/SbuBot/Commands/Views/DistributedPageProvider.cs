using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace SbuBot.Commands.Views
{
    public sealed class DistributedPageProvider : PageProvider
    {
        public List<Page> Pages { get; }
        public override int PageCount => Pages.Count;

        public DistributedPageProvider(
            IEnumerable<string> content,
            int maxItemsPerPage = -1,
            Func<LocalEmbed, LocalEmbed>? embedFactory = null
        )
        {
            Pages = SbuUtility.FillPages(content, maxItemsPerPage)
                .Select(str => new Page().WithEmbeds((embedFactory?.Invoke(new()) ?? new()).WithDescription(str)))
                .ToList();
        }

        public override ValueTask<Page> GetPageAsync(PagedViewBase view)
            => new(Pages.ElementAtOrDefault(view.CurrentPageIndex)!);
    }
}