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
using SbuBot.Commands.Parsing;
using SbuBot.Commands.Parsing.Descriptors;
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
            await Context.SaveChangesAsync();

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

                await Context.SaveChangesAsync();

                return Reply("Tag created.");
            }

            [Command]
            public async Task<DiscordCommandResult> CreateInteractiveAsync()
            {
                string? name;

                switch (await Context.WaitFollowUpForAsync("How should the tag be called? (spaces are allowed)."))
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

                switch (await Context.WaitFollowUpForAsync("What do you want the tag content to be?"))
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

                await Context.SaveChangesAsync();

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
                        $"{(notAuthor ? Mention.User(owner.Id) + "doesn't" : "You don't")} own any tags."
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
                await Context.SaveChangesAsync();

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

                switch (await Context.WaitFollowUpForAsync("What do you want the tag content to be?"))
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
                await Context.SaveChangesAsync();

                return Reply("The tag has been updated.");
            }
        }

        [Command("delete", "remove", "rm")]
        [Description("Removes a given tag.")]
        [Usage("tag remove da dog", "t delete h", "t rm all")]
        public async Task<DiscordCommandResult> RemoveAsync(
            [AuthorMustOwn][Description("The tag that should be removed.")]
            OneOrAll<SbuTag> tag
        )
        {
            switch (tag)
            {
                case OneOrAll<SbuTag>.All:
                {
                    ConfirmationResult result = await Context.WaitForConfirmationAsync(
                        "Are you sure you want to remove all your tags? Respond `yes` to confirm."
                    );

                    switch (result)
                    {
                        case ConfirmationResult.Timeout:
                        case ConfirmationResult.Aborted:
                            return Reply("Aborted.");

                        case ConfirmationResult.Confirmed:
                            break;

                        // unreachable
                        default:
                            throw new();
                    }

                    SbuMember owner = await Context.GetSbuDbContext().GetMemberAsync(Context.Author);

                    List<SbuTag> tags = await Context.GetSbuDbContext()
                        .Tags
                        .Where(t => t.OwnerId == owner.Id)
                        .ToListAsync(Context.Bot.StoppingToken);

                    Context.GetSbuDbContext().Tags.RemoveRange(tags);
                    await Context.SaveChangesAsync();

                    return Reply("All tags removed.");
                }

                case OneOrAll<SbuTag>.Specific specific:
                {
                    ConfirmationResult result = await Context.WaitForConfirmationAsync(
                        "Are you sure you want to remove this tag? Respond `yes` to confirm."
                    );

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

                    Context.GetSbuDbContext().Tags.Remove(specific.Value);
                    await Context.SaveChangesAsync();

                    return Reply("Tag removed.");
                }

                // unreachable
                default:
                    throw new();
            }
        }

        [Command("transfer", "mv")]
        [Description("Transfers ownership of a given tag to the given member.")]
        [Usage("tag transfer @user da dog", "t transfer 352815253828141056 h", "t mv @user all")]
        public async Task<DiscordCommandResult> TransferAsync(
            [NotAuthor][Description("The member that should receive the given tag.")]
            SbuMember receiver,
            [AuthorMustOwn][Description("The tag that the given member should receive.")]
            OneOrAll<SbuTag> tag
        )
        {
            switch (tag)
            {
                case OneOrAll<SbuTag>.All:
                {
                    ConfirmationResult result = await Context.WaitForConfirmationAsync(
                        string.Format(
                            "Are you sure you want to transfer all your tags to {0}? Respond `yes` to confirm.",
                            Mention.User(receiver.Id)
                        )
                    );

                    switch (result)
                    {
                        case ConfirmationResult.Timeout:
                        case ConfirmationResult.Aborted:
                            return Reply("Aborted.");

                        case ConfirmationResult.Confirmed:
                            break;

                        // unreachable
                        default:
                            throw new();
                    }

                    List<SbuTag> tags = await Context.GetSbuDbContext()
                        .Tags
                        .Where(t => t.OwnerId == Context.Author.Id)
                        .ToListAsync(Context.Bot.StoppingToken);

                    foreach (SbuTag dbTag in tags)
                        dbTag.OwnerId = receiver.Id;

                    Context.GetSbuDbContext().Tags.UpdateRange(tags);
                    await Context.SaveChangesAsync();

                    return Reply($"{Mention.User(receiver.Id)} now owns all of your tags.");
                }

                case OneOrAll<SbuTag>.Specific specific:
                {
                    specific.Value.OwnerId = receiver.Id;
                    Context.GetSbuDbContext().Tags.Update(specific.Value);
                    await Context.SaveChangesAsync();

                    return Reply($"{Mention.User(receiver.Id)} now owns `{specific.Value.Name}`.");
                }

                // unreachable
                default:
                    throw new();
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