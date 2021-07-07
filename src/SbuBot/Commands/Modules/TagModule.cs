using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Kkommon;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Commands.TypeParsers.Descriptors;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    [Group("tag", "t")]
    [Description("A collection of commands for creation, modification, removal and usage of tags.")]
    [Remarks("Tags may contain spaces or reserved keywords, tag names must be unique.")]
    public sealed class TagModule : SbuModuleBase
    {
        [Command]
        [Description("Responds with the given tag's content.")]
        [Usage("tag tagggg", "t whomstve")]
        public DiscordCommandResult Get([Description("The tag to invoke.")] SbuTag tag) => Response(tag.Content);

        [Command("claim", "take")]
        [Description("Claims the given tag if it has no owner.")]
        [Usage("tag claim tagggg", "t take whomstve")]
        public async Task<DiscordCommandResult> ClaimTagAsync(
            [MustBeOwned(false)][Description("The tag to claim.")]
            SbuTag tag
        )
        {
            tag.OwnerId = Context.Author.Id;
            Context.GetSbuDbContext().Tags.Update(tag);
            await Context.GetSbuDbContext().SaveChangesAsync();

            return Reply("Tag claimed.");
        }

        [Group("create", "make", "mk")]
        [Description("Creates a new tag with the given name and content.")]
        public sealed class CreateGroup : SbuModuleBase
        {
            [Command]
            [Usage("tag create tagggg :: new tag who dis", "t make da dog :: what da dog doin", "tag new h :: h")]
            public async Task<DiscordCommandResult> CreateAsync(
                [Description("The tag descriptor.")] TagDescriptor tagDescriptor
            )
            {
                if (await Context.GetTagAsync(tagDescriptor.Name) is { })
                    return Reply("A tag with same name already exists.");

                Context.GetSbuDbContext()
                    .AddTag(
                        Context.Author.Id,
                        Context.Guild.Id,
                        tagDescriptor.Name,
                        tagDescriptor.Content
                    );

                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply("Tag created.");
            }

            [Command]
            public async Task<DiscordCommandResult> CreateInteractiveAsync()
            {
                string? name;
                await Reply("How should the tag be called? (spaces are allowed)");

                switch (await Context.WaitFollowUpForAsync())
                {
                    case Result<string, FollowUpError>.Success followUp:
                        name = followUp.Value.Trim();
                        break;

                    case Result<string, FollowUpError>.Error error:
                        return Reply(
                            error.Value == FollowUpError.Aborted
                                ? "Aborted."
                                : "Aborted: You did not provide a tag name."
                        );

                    // unreachable
                    default:
                        throw new();
                }

                switch (SbuTag.IsValidTagName(name))
                {
                    case SbuTag.ValidNameType.TooShort:
                        return Reply(
                            $"Aborted: The tag name must be at least {SbuTag.MIN_NAME_LENGTH} characters long."
                        );

                    case SbuTag.ValidNameType.TooLong:
                        return Reply(
                            $"Aborted: The tag name must be at most {SbuTag.MAX_NAME_LENGTH} characters long."
                        );

                    case SbuTag.ValidNameType.Reserved:
                        return Reply("The tag name cannot be a reserved keyword.");

                    case SbuTag.ValidNameType.Valid:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (await Context.GetTagAsync(name) is { })
                    return Reply("Tag with same name already exists.");

                string? content;
                await Reply("What do you want the tag content to be?");

                switch (await Context.WaitFollowUpForAsync())
                {
                    case Result<string, FollowUpError>.Success followUp:
                        content = followUp.Value.Trim();

                        if (content.Length > SbuTag.MAX_CONTENT_LENGTH)
                        {
                            return Reply(
                                $"Aborted: The tag content must be at most {SbuTag.MAX_CONTENT_LENGTH} characters long."
                            );
                        }

                        break;

                    case Result<string, FollowUpError>.Error error:
                        return Reply(
                            error.Value == FollowUpError.Aborted
                                ? "Aborted."
                                : "Aborted: You did not provide tag content."
                        );

                    // unreachable
                    default:
                        throw new();
                }

                Context.GetSbuDbContext()
                    .AddTag(
                        Context.Author.Id,
                        Context.Guild.Id,
                        name,
                        content
                    );

                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply("Tag created.");
            }
        }

        [Group("list", "ls")]
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
                        $"{(notAuthor ? Mention.User(owner.Id) + "doesn't" : "you don't")} own and tags."
                    );
                }

                return FilledPages(
                    tags.Select(t => $"**Name:** {t.Name}\n**Content:** {t.Content}"),
                    embedModifier: embed => embed.WithTitle(
                        $"{(notAuthor ? $"{Mention.User(owner.Id)}'s" : "Your")} Tags"
                    )
                );
            }

            [Command]
            [Description("Lists all tags.")]
            public async Task<DiscordCommandResult> ListAllAsync()
            {
                List<SbuTag> tags = await Context.GetTagsAsync();

                if (tags.Count == 0)
                    return Reply("No tags found.");

                return FilledPages(
                    tags.Select(
                        t => string.Format(
                            "**Name:** {0}\n**Owner:** {1}",
                            t.Name,
                            t.OwnerId is { } ? Mention.User(t.Owner!.Id) : "None"
                        )
                    ),
                    embedModifier: embed => embed.WithTitle("Tags")
                );
            }
        }

        [Group("edit", "change")]
        [Description("Modifies the content of a given tag.")]
        public sealed class EditGroup : SbuModuleBase
        {
            [Command]
            [Usage("tag edit da dog :: what da dog doin now", "t change h :: h!!!")]
            public async Task<DiscordCommandResult> EditAsync(
                [Description("The tag descriptor.")] TagDescriptor tagDescriptor
            )
            {
                SbuTag? tag = await Context.GetTagAsync(tagDescriptor.Name);

                if (tag is null)
                    return Reply("No tag found.");

                if (tag.OwnerId != Context.Author.Id)
                    return Reply("You must be the owner of this tag.");

                tag.Content = tagDescriptor.Content;
                Context.GetSbuDbContext().Tags.Update(tag);
                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply("The tag has been updated.");
            }

            [Command]
            [Usage("tag edit da dog", "t change h")]
            public async Task<DiscordCommandResult> EditInteractiveAsync(
                [AuthorMustOwn][Description("The tag that should be modified.")]
                SbuTag tag
            )
            {
                string? content;
                await Reply("What do you want the tag content to be?");

                switch (await Context.WaitFollowUpForAsync())
                {
                    case Result<string, FollowUpError>.Success followUp:
                        content = followUp.Value.Trim();

                        if (content.Length > SbuTag.MAX_CONTENT_LENGTH)
                        {
                            return Reply(
                                $"Aborted: The tag content must be at most {SbuTag.MAX_CONTENT_LENGTH} characters long."
                            );
                        }

                        break;

                    case Result<string, FollowUpError>.Error error:
                        return Reply(
                            error.Value == FollowUpError.Aborted
                                ? "Aborted."
                                : "Aborted: You did not provide tag content."
                        );

                    // unreachable
                    default:
                        throw new();
                }

                tag.Content = content.Trim();
                Context.GetSbuDbContext().Tags.Update(tag);
                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply("The tag has been updated.");
            }
        }

        [Group("delete", "remove", "rm")]
        [Description("A group of commands for removing tags.")]
        public sealed class RemoveGroup : SbuModuleBase
        {
            [Command]
            [Description("Removes a given tag.")]
            [Usage("tag remove da dog", "t delete h")]
            public async Task<DiscordCommandResult> RemoveAsync(
                [AuthorMustOwn][Description("The tag that should be removed.")]
                SbuTag tag
            )
            {
                await Reply("Are you sure you want to remove this tag? Respond `yes` to confirm.");
                ConfirmationResult result = await Context.WaitForConfirmationAsync();

                switch (result)
                {
                    case ConfirmationResult.Timeout:
                    case ConfirmationResult.Aborted:
                        return Reply("Aborted.");

                    case ConfirmationResult.Confirmed:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Context.GetSbuDbContext().Tags.Remove(tag);
                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply("Tag removed.");
            }

            [Command("all")]
            [Description("Removes all of the command author's tags.")]
            public async Task<DiscordCommandResult> RemoveAllAsync()
            {
                await Reply("Are you sure you want to remove all your tags? Respond `yes` to confirm.");
                ConfirmationResult result = await Context.WaitForConfirmationAsync();

                switch (result)
                {
                    case ConfirmationResult.Timeout:
                    case ConfirmationResult.Aborted:
                        return Reply("Aborted.");

                    case ConfirmationResult.Confirmed:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                SbuMember owner = await Context.GetSbuDbContext().GetMemberAsync(Context.Author);

                List<SbuTag> tags = await Context.GetSbuDbContext()
                    .Tags
                    .Where(t => t.OwnerId == owner.Id)
                    .ToListAsync(Context.Bot.StoppingToken);

                Context.GetSbuDbContext().Tags.RemoveRange(tags);
                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply("All tags removed.");
            }
        }

        [Group("transfer", "mv")]
        [Description("A group of commands for transferring tags.")]
        public sealed class TransferGroup : SbuModuleBase
        {
            [Command]
            [Description("Transfers ownership of a given tag to the given member.")]
            [Usage("tag transfer @user da dog", "t transfer 352815253828141056 h")]
            public async Task<DiscordCommandResult> TransferAsync(
                [NotAuthor][Description("The member that should receive the given tag.")]
                SbuMember receiver,
                [AuthorMustOwn][Description("The tag that the given member should receive.")]
                SbuTag tag
            )
            {
                tag.OwnerId = receiver.Id;
                Context.GetSbuDbContext().Tags.Update(tag);
                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply($"{Mention.User(receiver.Id)} now owns `{tag.Name}`.");
            }

            [Command("all")]
            [Description("Transfers ownership of all of the command author's tags to the given member.")]
            [Usage("tag transfer all @user", "t transfer all 352815253828141056")]
            public async Task<DiscordCommandResult> TransferAllAsync(
                [NotAuthor][Description("The member that should receive the given tags.")]
                SbuMember receiver
            )
            {
                await Reply(
                    string.Format(
                        "Are you sure you want to transfer all your tags to {0}? Respond `yes` to confirm.",
                        Mention.User(receiver.Id)
                    )
                );

                ConfirmationResult result = await Context.WaitForConfirmationAsync();

                switch (result)
                {
                    case ConfirmationResult.Timeout:
                    case ConfirmationResult.Aborted:
                        return Reply("Aborted.");

                    case ConfirmationResult.Confirmed:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                List<SbuTag> tags = await Context.GetSbuDbContext()
                    .Tags
                    .Where(t => t.OwnerId == Context.Author.Id)
                    .ToListAsync(Context.Bot.StoppingToken);

                foreach (SbuTag dbTag in tags)
                    dbTag.OwnerId = receiver.Id;

                Context.GetSbuDbContext().Tags.UpdateRange(tags);
                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply($"{Mention.User(receiver.Id)} now owns all of your tags.");
            }
        }

        [Command("reserved")]
        [Description(
            "Lists the reserved keywords, tags are not allowed to be any of these keywords, but can start with, end "
            + "with or contain them."
        )]
        public DiscordCommandResult GetReservedKeywords() => Reply(
            "The following keywords are not allowed to be tags, but tags may contain them:\n"
            + string.Join("\n", SbuGlobals.RESERVED_KEYWORDS.Select(rn => $"> `{rn}`"))
        );
    }
}