using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;

using Kkommon.Extensions.Enumerable;

namespace SbuBot.Commands
{
    public class MaybePagedEmbed : PagedMenu
    {
        public MaybePagedEmbed(
            Snowflake userId,
            IEnumerable<string> pages,
            string? title = null,
            string? footer = null,
            DateTimeOffset? timestamp = null
        ) : base(userId, new MaybePageEmbedProvider(pages, title, footer, timestamp), pages.CountAtLeast(2)) { }
    }

    public class MaybePageEmbedProvider : IPageProvider, IEnumerable<LocalEmbed>
    {
        private readonly List<Page> _pages;

        public int PageCount => _pages.Count;

        public MaybePageEmbedProvider(
            IEnumerable<string> pages,
            string? title = null,
            string? footer = null,
            DateTimeOffset? timestamp = null
        )
        {
            List<string> pageContentList = pages.ToList();
            _pages = new(pageContentList.Count);

            for (var i = 0; i < pageContentList.Count; i++)
            {
                _pages.Add(
                    new LocalEmbed()
                        .WithTitle(title)
                        .WithDescription(pageContentList[i])
                        .WithFooter($"{(footer is { } ? $"{footer} • " : "")}{i + 1}/{pageContentList.Count}")
                        .WithTimestamp(timestamp)
                        .Clone()
                );
            }
        }

        public ValueTask<Page> GetPageAsync(PagedMenu menu) => ValueTask.FromResult(_pages[menu.CurrentPageIndex]);

        public IEnumerator<LocalEmbed> GetEnumerator() => _pages.Select(page => page.Embed).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}