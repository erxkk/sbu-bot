using System;
using System.Collections.Generic;

using Disqord;
using Disqord.Bot;

namespace SbuBot.Commands
{
    public abstract class SbuModuleBase : DiscordGuildModuleBase<SbuCommandContext>
    {
        protected DiscordMenuCommandResult MaybePages(
            IEnumerable<string> contents,
            string? title = null,
            string? footer = null,
            DateTimeOffset? timestamp = null,
            int itemsPerPage = -1,
            bool filled = true
        ) => Menu(
            new MaybePagedEmbed(
                Context.Author.Id,
                filled ? SbuUtility.FillPages(contents, itemsPerPage) : contents,
                title,
                footer,
                timestamp
            )
        );

        protected DiscordPrivateResponseCommandResult PrivateResponse(LocalMessage message) => new(Context, message);
    }
}