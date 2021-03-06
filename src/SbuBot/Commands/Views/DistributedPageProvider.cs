using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            int maxPageLength = LocalEmbed.MaxDescriptionLength,
            Func<LocalEmbed, LocalEmbed>? embedFactory = null
        )
        {
            Pages = DistributedPageProvider.FillPages(content, maxItemsPerPage, maxPageLength)
                .Select(str => new Page().WithEmbeds((embedFactory?.Invoke(new()) ?? new()).WithDescription(str)))
                .ToList();
        }

        private static IEnumerable<string> FillPages(
            IEnumerable<string> source,
            int maxElementsPerPage = -1,
            int maxPageLength = LocalEmbed.MaxDescriptionLength
        )
        {
            StringBuilder builder = new StringBuilder();
            int elements = 0;

            foreach (string item in source)
            {
                if (item.Length + 1 > maxPageLength)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(source),
                        item.Length,
                        $"An item in the collection was longer than maximum page length of {maxPageLength}."
                    );
                }

                if ((maxElementsPerPage == -1 || elements <= maxElementsPerPage)
                    && builder.Length + item.Length + 3 <= maxPageLength)
                {
                    elements++;
                    builder.AppendLine(item);

                    continue;
                }

                yield return builder.ToString();

                elements = 0;
                builder.Clear().AppendLine(item);
            }

            if (builder.Length == 0)
                throw new ArgumentException("Source cannot be empty", nameof(source));

            yield return builder.ToString();
        }

        public override ValueTask<Page> GetPageAsync(PagedViewBase view)
            => new(Pages.ElementAtOrDefault(view.CurrentPageIndex)!);
    }
}
