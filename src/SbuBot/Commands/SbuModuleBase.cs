using System;
using System.Collections.Generic;

using Disqord;
using Disqord.Bot;

namespace SbuBot.Commands
{
    public abstract class SbuModuleBase : DiscordGuildModuleBase<SbuCommandContext>
    {
        protected DiscordMenuCommandResult SinglePages(
            IEnumerable<string> contents,
            string? title = null,
            string? footer = null,
            DateTimeOffset? timestamp = null
        ) => Menu(new MaybePagedEmbed(Context.Author.Id, contents, title, footer, timestamp));

        protected DiscordMenuCommandResult MaybePages(
            IEnumerable<string> contents,
            string? title = null,
            string? footer = null,
            DateTimeOffset? timestamp = null,
            int itemsPerPage = -1
        ) => Menu(
            new MaybePagedEmbed(Context.Author.Id, Utility.FillPages(contents, itemsPerPage), title, footer, timestamp)
        );

        protected DiscordPrivateResponseCommandResult PrivateResponse(LocalMessage message) => new(Context, message);
    }
}