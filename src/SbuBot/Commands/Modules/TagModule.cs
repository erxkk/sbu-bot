using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Commands.Checks.Parameters;
using SbuBot.Commands.Descriptors;
using SbuBot.Commands.Information;
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
        public DiscordCommandResult Get(SbuTag tag) => Response(tag.Content);

        [Command("claim", "take")]
        [Description("Claims the given tag if it has no owner.")]
        public async Task<DiscordCommandResult> ClaimTagAsync([MustBeOwned(false)] SbuTag tag)
        {
            tag.OwnerId = (await Context.GetSbuDbContext().GetSbuMemberAsync(Context.Author)).Id;
            Context.GetSbuDbContext().Tags.Update(tag);
            await Context.GetSbuDbContext().SaveChangesAsync();

            return Reply("Tag claimed.");
        }

        [Group("create", "make", "new")]
        [Description("Creates a new tag with the given name and content.")]
        public sealed class CreateGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> CreateAsync(
                [Description("The tag descriptor.")] TagDescriptor tagDescriptor
            )
            {
                if (await Context.GetSbuDbContext()
                        .Tags.FirstOrDefaultAsync(
                            t => t.Name == tagDescriptor.Name,
                            Context.Bot.StoppingToken
                        ) is { }
                ) return Reply("A tag with same name already exists.");

                Context.GetSbuDbContext()
                    .Tags.Add(
                        new(
                            (await Context.GetSbuDbContext().GetSbuMemberAsync(Context.Author)).Id,
                            (await Context.GetSbuDbContext().GetSbuGuildAsync(Context.Guild)).Id,
                            tagDescriptor.Name,
                            tagDescriptor.Content
                        )
                    );

                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply("Tag created.");
            }

            [Command]
            public async Task<DiscordCommandResult> CreateInteractiveAsync()
            {
                await Reply("How should the tag be called? (spaces are allowed)");

                MessageReceivedEventArgs? waitNameResult;

                await using (Context.BeginYield())
                {
                    waitNameResult = await Context.WaitForMessageAsync(
                        e => e.Member.Id == Context.Author.Id,
                        cancellationToken: Context.Bot.StoppingToken
                    );
                }

                if (waitNameResult is null)
                    return Reply("Aborted: You did not provide a tag name.");

                string name = waitNameResult.Message.Content.Trim();

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

                if (await Context.GetSbuDbContext()
                    .Tags.FirstOrDefaultAsync(t => t.Name == name, Context.Bot.StoppingToken) is { })
                    return Reply("Tag with same name already exists.");

                await Reply("What do you want the tag content to be?");

                MessageReceivedEventArgs? waitContentResult;

                await using (Context.BeginYield())
                {
                    waitContentResult = await Context.WaitForMessageAsync(
                        e => e.Member.Id == Context.Author.Id,
                        cancellationToken: Context.Bot.StoppingToken
                    );
                }

                if (waitContentResult is null)
                    return Reply("Aborted: You did not provide tag content.");

                if (waitContentResult.Message.Content.Length > SbuTag.MAX_CONTENT_LENGTH)
                {
                    return Reply(
                        $"Aborted: The tag content must be at most {SbuTag.MAX_CONTENT_LENGTH} characters long."
                    );
                }

                Context.GetSbuDbContext()
                    .Tags.Add(
                        new(
                            (await Context.GetSbuDbContext().GetSbuMemberAsync(Context.Author)).Id,
                            (await Context.GetSbuDbContext().GetSbuGuildAsync(Context.Guild)).Id,
                            waitNameResult.Message.Content,
                            waitContentResult.Message.Content
                        )
                    );

                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply("Tag created.");
            }
        }

        [Group("list")]
        [Description("A group of commands for listing tags.")]
        public sealed class ListGroup : SbuModuleBase
        {
            [Command]
            [Description("Lists the tags of a given member, or of the command author if no member is specified.")]
            public async Task<DiscordCommandResult> ListFromOwnerAsync(
                [OverrideDefault("@author")]
                [Description("The member who's tags should be listed.")]
                [Remarks("Defaults to the command author.")]
                SbuMember? owner = null
            )
            {
                owner ??= await Context.GetSbuDbContext().GetSbuMemberAsync(Context.Author);
                bool notAuthor = owner.DiscordId != Context.Author.Id;

                List<SbuTag> tags = await Context.GetSbuDbContext()
                    .Tags
                    .Where(t => t.OwnerId == owner.Id)
                    .ToListAsync(Context.Bot.StoppingToken);

                if (tags.Count == 0)
                {
                    return Reply(
                        $"{(notAuthor ? Mention.User(owner.DiscordId) + "doesn't" : "you don't")} own and tags."
                    );
                }

                return MaybePages(
                    tags.Select(t => $"**Name:** {t.Name}\n**Content:** {t.Content}"),
                    $"{(notAuthor ? $"{Mention.User(owner.DiscordId)}'s" : "Your")} Tags"
                );
            }

            [Command("all")]
            [Description("Lists all tags.")]
            public async Task<DiscordCommandResult> ListAllAsync()
            {
                List<SbuTag> tags = await Context.GetSbuDbContext()
                    .Tags.Include(t => t.Owner)
                    .ToListAsync(Context.Bot.StoppingToken);

                if (tags.Count == 0)
                    return Reply("No tags found.");

                return MaybePages(
                    tags.Select(
                        t => string.Format(
                            "**Name:** {0}\n**Owner:** {1}\n**Content:** {2}",
                            t.Name,
                            t.OwnerId is { } ? Mention.User(t.Owner!.DiscordId) : "None",
                            t.Content
                        )
                    )
                );
            }
        }

        [Group("edit", "change")]
        [Description("Modifies the content of a given tag.")]
        public sealed class EditGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> EditAsync(
                [Description("The tag descriptor.")] TagDescriptor tagDescriptor
            )
            {
                SbuTag? tag = await Context.GetSbuDbContext()
                    .Tags.FirstOrDefaultAsync(
                        t => t.Name == tagDescriptor.Name,
                        Context.Bot.StoppingToken
                    );

                if (tag is null)
                    return Reply("No tag found.");

                if (tag.OwnerId != (await Context.GetSbuDbContext().GetSbuMemberAsync(Context.Author)).Id)
                    return Reply("You must be the owner of this tag.");

                tag.Content = tagDescriptor.Content;
                Context.GetSbuDbContext().Tags.Update(tag);
                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply("The tag has been updated.");
            }

            [Command]
            public async Task<DiscordCommandResult> EditInteractiveAsync(
                [AuthorMustOwn][Description("The tag that should be modified.")]
                SbuTag tag
            )
            {
                await Reply("What do you want the new tag content to be?");

                MessageReceivedEventArgs? waitContentResult;

                await using (Context.BeginYield())
                {
                    waitContentResult = await Context.WaitForMessageAsync(
                        e => e.Member.Id == Context.Author.Id,
                        cancellationToken: Context.Bot.StoppingToken
                    );
                }

                if (waitContentResult is null)
                    return Reply("Aborted: You did not provide tag content.");

                tag.Content = waitContentResult.Message.Content;
                Context.GetSbuDbContext().Tags.Update(tag);
                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply("The tag has been updated.");
            }
        }

        [Group("remove", "delete")]
        [Description("A group of commands for removing tags.")]
        public sealed class RemoveGroup : SbuModuleBase
        {
            [Command]
            [Description("Removes a given tag.")]
            public async Task<DiscordCommandResult> RemoveAsync(
                [AuthorMustOwn][Description("The tag that should be removed.")]
                SbuTag tag
            )
            {
                await Reply("Are you sure you want to remove this tag? Respond `yes` to confirm.");

                MessageReceivedEventArgs? waitConfirmResult;

                await using (Context.BeginYield())
                {
                    waitConfirmResult = await Context.WaitForMessageAsync(
                        e => e.Member.Id == Context.Author.Id,
                        cancellationToken: Context.Bot.StoppingToken
                    );
                }

                if (waitConfirmResult is null
                    || !waitConfirmResult.Message.Content.Equals("yes", StringComparison.OrdinalIgnoreCase))
                    return Reply("Aborted.");

                Context.GetSbuDbContext().Tags.Remove(tag);
                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply("Tag removed.");
            }

            [Command("all")]
            [Description("Removes all of the command author's tags.")]
            public async Task<DiscordCommandResult> RemoveAllAsync()
            {
                await Reply("Are you sure you want to remove all your tags? Respond `yes` to confirm.");

                MessageReceivedEventArgs? waitConfirmResult;

                await using (Context.BeginYield())
                {
                    waitConfirmResult = await Context.WaitForMessageAsync(
                        e => e.Member.Id == Context.Author.Id,
                        cancellationToken: Context.Bot.StoppingToken
                    );
                }

                if (waitConfirmResult is null
                    || !waitConfirmResult.Message.Content.Equals("yes", StringComparison.OrdinalIgnoreCase))
                    return Reply("Aborted.");

                SbuMember owner = await Context.GetSbuDbContext().GetSbuMemberAsync(Context.Author);

                List<SbuTag> tags = await Context.GetSbuDbContext()
                    .Tags
                    .Where(t => t.OwnerId == owner.Id)
                    .ToListAsync(Context.Bot.StoppingToken);

                Context.GetSbuDbContext().Tags.RemoveRange(tags);
                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply("All tags removed.");
            }
        }

        [Group("transfer")]
        [Description("A group of commands for transferring tags.")]
        public sealed class TransferGroup : SbuModuleBase
        {
            [Command]
            [Description("Transfers ownership of a given tag to the given member.")]
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

                return Reply($"{Mention.User(receiver.DiscordId)} now owns `{tag.Name}`.");
            }

            [Command("all")]
            [Description("Transfers ownership of all of the command author's tags to the given member.")]
            public async Task<DiscordCommandResult> TransferAllAsync(
                [NotAuthor][Description("The member that should receive the given tags.")]
                SbuMember receiver
            )
            {
                await Reply(
                    string.Format(
                        "Are you sure you want to transfer all your tags to {0}? Respond `yes` to confirm.",
                        Mention.User(receiver.DiscordId)
                    )
                );

                MessageReceivedEventArgs? waitConfirmResult;

                await using (Context.BeginYield())
                {
                    waitConfirmResult = await Context.WaitForMessageAsync(
                        e => e.Member.Id == Context.Author.Id,
                        cancellationToken: Context.Bot.StoppingToken
                    );
                }

                if (waitConfirmResult is null
                    || !waitConfirmResult.Message.Content.Equals("yes", StringComparison.OrdinalIgnoreCase))
                    return Reply("Aborted.");

                SbuMember owner = await Context.GetSbuDbContext().GetSbuMemberAsync(Context.Author);

                List<SbuTag> tags = await Context.GetSbuDbContext()
                    .Tags
                    .Where(t => t.OwnerId == owner.Id)
                    .ToListAsync(Context.Bot.StoppingToken);

                foreach (SbuTag dbTag in tags)
                {
                    dbTag.OwnerId = receiver.Id;
                }

                Context.GetSbuDbContext().Tags.UpdateRange(tags);
                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply($"{Mention.User(receiver.DiscordId)} now owns all of your tags.");
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