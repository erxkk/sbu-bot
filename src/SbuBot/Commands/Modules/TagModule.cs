using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Commands.Attributes;
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
        [Usage("tag tagggg", "t whomstve")]
        public DiscordCommandResult Get([Description("The tag to invoke.")] SbuTag tag)
            => Response(tag.Content);

        [Group("list")]
        [Description("A group of commands for listing tags.")]
        public sealed class ListGroup : SbuModuleBase
        {
            [Command]
            [Description("Lists the tags of a given member, or of the command author if no member is specified.")]
            [Usage("tag list me", "t list @user", "tag list 352815253828141056", "tag list Allah")]
            public async Task<DiscordCommandResult> ListFromOwnerAsync(
                [Description("The member who's tags should be listed.")]
                SbuMember owner
            )
            {
                bool notAuthor = owner.Id != Context.Author.Id;

                List<SbuTag> tags = await Context.GetSbuDbContext()
                    .Tags
                    .Where(t => t.OwnerId == owner.Id)
                    .ToListAsync(Context.Bot.StoppingToken);

                if (tags.Count == 0)
                {
                    return Reply(
                        $"{(notAuthor ? Mention.User(owner.Id) + "doesn't" : "You don't")} own any tags."
                    );
                }

                return DistributedPages(
                    tags.Select(t => $"{SbuGlobals.BULLET} {t.Name}\n`{t.Content}`\n"),
                    embedFactory: embed => embed.WithTitle(
                        $"{(notAuthor ? $"{Mention.User(owner.Id)}'s" : "Your")} Tags"
                    )
                );
            }

            [Command]
            [Description("Lists all tags.")]
            public async Task<DiscordCommandResult> ListAllAsync()
            {
                List<SbuTag> tags = await Context.GetTagsFullAsync();

                if (tags.Count == 0)
                    return Reply("No tags found.");

                return DistributedPages(
                    tags.Select(
                        t => string.Format(
                            "{0} {1}: {2}\n`{3}`\n",
                            SbuGlobals.BULLET,
                            t.OwnerId is { } ? Mention.User(t.Owner!.Id) : "No owner",
                            t.Name,
                            t.Content
                        )
                    ),
                    embedFactory: embed => embed.WithTitle("Tags")
                );
            }
        }

        [Command("reserved")]
        [Description(
            "Lists the reserved keywords, tags are not allowed to be any of these keywords, but can start with, end "
            + "with or contain them."
        )]
        public DiscordCommandResult GetReservedKeywords() => Reply(
            string.Format(
                "The following keywords are not allowed to be tags, but tags may contain them:\n{0}",
                SbuGlobals.Keyword.ALL_RESERVED.Select(rn => $"> `{rn}`").ToNewLines()
            )
        );
    }
}