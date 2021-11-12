using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Commands.Parsing.HelperTypes;
using SbuBot.Commands.Views;
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

        [Command("claim")]
        [Description("Claims the given tag if it has no owner.")]
        public async Task<DiscordCommandResult> ClaimAsync(
            [MustBeOwned(false)][Description("The tag to claim.")]
            SbuTag tag
        )
        {
            SbuDbContext context = Context.GetSbuDbContext();

            tag.OwnerId = Context.Author.Id;
            context.Tags.Update(tag);
            await context.SaveChangesAsync();

            return Reply("Tag claimed.");
        }

        [Command("transfer")]
        [Description("Transfers ownership of a given tag(s) to the given member.")]
        public async Task<DiscordCommandResult> TransferAsync(
            [NotAuthor][Description("The member that should receive the given tag(s).")]
            SbuMember receiver,
            [AuthorMustOwn][Description("The tag that the given member should receive.")]
            OneOrAll<SbuTag> tag
        )
        {
            if (tag.IsAll)
            {
                ConfirmationState result = await AgreementAsync(
                    new()
                    {
                        Context.Author.Id,
                        receiver.Id,
                    },
                    string.Format(
                        "Are you sure you want to transfer all your tags to {0}?",
                        Mention.User(receiver.Id)
                    )
                );

                switch (result)
                {
                    case ConfirmationState.None:
                    case ConfirmationState.Aborted:
                    case ConfirmationState.TimedOut:
                        return Reply("Aborted.");

                    case ConfirmationState.Confirmed:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                SbuDbContext context = Context.GetSbuDbContext();

                List<SbuTag> tags = await context
                    .Tags
                    .Where(t => t.OwnerId == Context.Author.Id)
                    .ToListAsync(Context.Bot.StoppingToken);

                foreach (SbuTag dbTag in tags)
                    dbTag.OwnerId = receiver.Id;

                context.Tags.UpdateRange(tags);
                await context.SaveChangesAsync();

                return Response($"{Mention.User(receiver.Id)} now owns all of your tags.");
            }
            else
            {
                ConfirmationState result = await AgreementAsync(
                    new()
                    {
                        Context.Author.Id,
                        receiver.Id,
                    },
                    "Do you accept the tag transfer?"
                );

                switch (result)
                {
                    case ConfirmationState.None:
                    case ConfirmationState.Aborted:
                    case ConfirmationState.TimedOut:
                        return Reply("Aborted.");

                    case ConfirmationState.Confirmed:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                SbuDbContext context = Context.GetSbuDbContext();

                tag.Value.OwnerId = receiver.Id;
                context.Tags.Update(tag.Value);
                await context.SaveChangesAsync();

                return Response($"{Mention.User(receiver.Id)} now owns `{tag.Value.Name}`.");
            }
        }

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
                    .ToListAsync(Context.Bot.StoppingToken);

                if (tags.Count == 0)
                {
                    return Reply(
                        $"{(notAuthor ? Mention.User(ownerId) + "doesn't" : "You don't")} own any tags."
                    );
                }

                return DistributedPages(
                    tags.Select(t => $"{SbuGlobals.BULLET} `{t.Name}`\n{t.Content}\n"),
                    embedFactory: embed => embed.WithTitle(
                        $"{(notAuthor ? $"{Mention.User(ownerId)}'s" : "Your")} Tags"
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
                            t.Content
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
