using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Commands.Parsing.HelperTypes;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    [Group("tag", "t")]
    [Description("A collection of commands for creation, modification, removal and usage of tags.")]
    [Remarks("Tags may contain spaces or reserved keywords, tag names must be unique.")]
    public sealed partial class TagModule : SbuModuleBase
    {
        [Command]
        [Description("Responds with the given tag's content.")]
        public DiscordCommandResult Get([Description("The tag to invoke.")] SbuTag tag)
            => Response(tag.Content);

        [Command("owner")]
        [Description("Returns the owner of the given tag.")]
        public DiscordCommandResult GetOwner([Description("The tag to invoke.")] SbuTag tag)
            => Response(
                tag.OwnerId is null
                    ? "Nobody owns this tag."
                    : $"{Mention.User(tag.OwnerId.Value)} owns this tag."
            );

        [Command("list")]
        [Description("Lists the tags of a given member, or of the command author if no member is specified.")]
        public async Task<DiscordCommandResult> ListFromOwnerAsync(
            [Description("The member who's tags should be listed.")]
            OneOrAll<SbuMember>? owner = null
        )
        {
            if (owner is null || !owner.IsAll)
            {
                Snowflake ownerId = (owner?.Value?.Id ?? Context.Author.Id);
                bool notAuthor = ownerId != Context.Author.Id;

                List<SbuTag> tags = await Context.GetSbuDbContext()
                    .Tags
                    .Where(t => t.OwnerId == ownerId)
                    .ToListAsync(Bot.StoppingToken);

                if (tags.Count == 0)
                {
                    return Reply(
                        $"{(notAuthor ? Mention.User(ownerId) + "doesn't" : "You don't")} own any tags."
                    );
                }

                IMember author = Context.Guild.GetMember(ownerId) ?? await Context.Guild.FetchMemberAsync(ownerId);

                return DistributedPages(
                    tags.Select(t => $"{SbuGlobals.BULLET} `{t.Name}`\n{SbuUtility.Truncate(t.Content, 256)}\n"),
                    embedFactory: embed => embed.WithAuthor(author)
                        .WithTitle(
                            $"{(notAuthor ? $"{author.Nick ?? author.Name}'s" : "Your")} Tags"
                        ),
                    itemsPerPage: 20,
                    maxPageLength: LocalEmbed.MaxDescriptionLength / 2
                );
            }
            else
            {
                List<SbuTag> tags = await Context.GetTagsFullAsync();

                if (tags.Count == 0)
                    return Reply("No tags found.");

                return DistributedPages(
                    tags.Select(
                        t => string.Format(
                            "{0} {1}: `{2}`\n{3}\n",
                            SbuGlobals.BULLET,
                            t.OwnerId is { } ? Mention.User(t.Owner!.Id) : "No owner",
                            t.Name,
                            SbuUtility.Truncate(t.Content, 256)
                        )
                    ),
                    embedFactory: embed => embed.WithTitle("Tags"),
                    itemsPerPage: 20,
                    maxPageLength: LocalEmbed.MaxDescriptionLength / 2
                );
            }
        }
    }
}
