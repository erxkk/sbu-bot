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
            tag.OwnerId = Context.Author.Id;
            Context.Db.Tags.Update(tag);
            await Context.Db.SaveChangesAsync();

            return Reply("Tag claimed.");
        }

        [Group("create", "make", "new"), PureGroup]
        [Description("Creates a new tag with the given name and content.")]
        public sealed class CreateGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> CreateAsync(TagDescriptor tagDescriptor)
            {
                SbuTag? tag;

                await using (Context.BeginYield())
                {
                    tag = await Context.Db.Tags.FirstOrDefaultAsync(t => t.Name == tagDescriptor.Name);
                }

                if (tag is { })
                    return Reply("A tag with same name already exists.");

                Context.Db.Tags.Add(new(Context.Author.Id, tagDescriptor.Name, tagDescriptor.Content));
                await Context.Db.SaveChangesAsync();

                return Reply("Tag created.");
            }

            [Command]
            public async Task<DiscordCommandResult> CreateInteractiveAsync()
            {
                MessageReceivedEventArgs? waitNameResult;

                await Reply("How should the tag be called? (spaces are allowed)");

                await using (Context.BeginYield())
                {
                    waitNameResult = await Context.WaitForMessageAsync(e => e.Member.Id == Context.Author.Id);
                }

                if (waitNameResult is null)
                    return Reply("Aborted: You did not provide a tag name.");

                switch (SbuTag.IsValidTagName(waitNameResult.Message.Content))
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

                SbuTag? tag;

                await using (Context.BeginYield())
                {
                    tag = await Context.Db.Tags.FirstOrDefaultAsync(t => t.Name == waitNameResult.Message.Content);
                }

                if (tag is { })
                    return Reply("Tag with same name already exists.");

                MessageReceivedEventArgs? waitContentResult;
                await Reply("What do you want the tag content to be?");

                await using (Context.BeginYield())
                {
                    waitContentResult = await Context.WaitForMessageAsync(e => e.Member.Id == Context.Author.Id);
                }

                if (waitContentResult is null)
                    return Reply("Aborted: You did not provide tag content.");

                if (waitContentResult.Message.Content.Length > SbuTag.MAX_CONTENT_LENGTH)
                {
                    return Reply(
                        $"Aborted: The tag content must be at most {SbuTag.MAX_CONTENT_LENGTH} characters long."
                    );
                }

                Context.Db.Tags.Add(
                    new(Context.Author.Id, waitNameResult.Message.Content, waitContentResult.Message.Content)
                );

                await Context.Db.SaveChangesAsync();

                return Reply("Tag created.");
            }
        }

        [Command("list")]
        [Description("Lists the tags of a given member, or of the author if no member is specified.")]
        public async Task<DiscordCommandResult> ListFromOwnerAsync(
            [OverrideDefault("@author")] SbuMember? owner = null
        )
        {
            owner ??= Context.Invoker;
            bool notAuthor = owner.DiscordId != Context.Author.Id;

            List<SbuTag> tags;

            await using (Context.BeginYield())
            {
                tags = await Context.Db.Tags.Where(t => t.OwnerId == owner.DiscordId).ToListAsync();
            }

            if (tags.Count == 0)
                return Reply($"{(notAuthor ? Mention.User(owner.DiscordId) + "doesn't" : "you don't")} own and tags.");

            return MaybePages(
                tags.Select(t => $"**Name:** {t.Name}\n**Content:** {t.Content}"),
                $"{(notAuthor ? $"{Mention.User(owner.DiscordId)}'s" : "Your")} Tags"
            );
        }

        [Group("edit", "change"), PureGroup]
        [Description("Modifies the content of a given tag.")]
        public class Group : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> EditAsync(TagDescriptor tagDescriptor)
            {
                SbuTag? tag;

                await using (Context.BeginYield())
                {
                    tag = await Context.Db.Tags.FirstOrDefaultAsync(t => t.Name == tagDescriptor.Name);
                }

                if (tag is null)
                    return Reply("No tag found.");

                if (tag.OwnerId != Context.Invoker.DiscordId)
                    return Reply("You must be the owner of this tag.");

                tag.Content = tagDescriptor.Content;
                Context.Db.Tags.Update(tag);
                await Context.Db.SaveChangesAsync();

                return Reply("The tag has been updated.");
            }

            [Command]
            public async Task<DiscordCommandResult> EditInteractiveAsync([AuthorMustOwn] SbuTag? tag = null)
            {
                if (tag is null)
                {
                    MessageReceivedEventArgs? waitNameResult;
                    await Reply("What tag do you want to edit?");

                    await using (Context.BeginYield())
                    {
                        waitNameResult = await Context.WaitForMessageAsync(e => e.Member.Id == Context.Author.Id);
                    }

                    if (waitNameResult is null)
                        return Reply("Aborted: You did not provide tag.");

                    await using (Context.BeginYield())
                    {
                        tag = await Context.Db.Tags.FirstOrDefaultAsync(t => t.Name == waitNameResult.Message.Content);
                    }

                    if (tag is null)
                        return Reply("No tag found.");

                    if (tag.OwnerId != Context.Invoker.DiscordId)
                        return Reply("You must be the owner of this tag.");
                }

                MessageReceivedEventArgs? waitContentResult;
                await Reply("What do you want the new tag content to be?");

                await using (Context.BeginYield())
                {
                    waitContentResult = await Context.WaitForMessageAsync(e => e.Member.Id == Context.Author.Id);
                }

                if (waitContentResult is null)
                    return Reply("Aborted: You did not provide tag content.");

                tag.Content = waitContentResult.Message.Content;
                Context.Db.Tags.Update(tag);
                await Context.Db.SaveChangesAsync();

                return Reply("The tag has been updated.");
            }
        }

        [Command("remove", "delete")]
        [Description("Removes a given tag, or all if no tag is specified.")]
        public async Task<DiscordCommandResult> RemoveAsync(
            [OverrideDefault("all"), AuthorMustOwn]
            SbuTag? tag = null
        )
        {
            if (tag is { })
            {
                Context.Db.Tags.Remove(tag);
                await Context.Db.SaveChangesAsync();

                return Reply("Tag removed.");
            }

            MessageReceivedEventArgs? waitConfirmResult;
            await Reply("Are you sure you want to remove all your tags? Respond with `yes` to continue.");

            await using (Context.BeginYield())
            {
                waitConfirmResult = await Context.WaitForMessageAsync(e => e.Member.Id == Context.Author.Id);
            }

            if (waitConfirmResult is null || waitConfirmResult.Message.Content != "yes")
                return Reply("Aborted.");

            List<SbuTag> tags;

            await using (Context.BeginYield())
            {
                tags = await Context.Db.Tags.Where(t => t.OwnerId == Context.Author.Id).ToListAsync();
            }

            Context.Db.Tags.RemoveRange(tags);
            await Context.Db.SaveChangesAsync();

            return Reply("All tags removed.");
        }

        [Command("transfer")]
        [Description("Transfers ownership of a given tag to another member, or all if no tag is specified.")]
        public async Task<DiscordCommandResult> TransferAllAsync(
            [NotAuthor] SbuMember receiver,
            [OverrideDefault("all"), AuthorMustOwn]
            SbuTag? tag = null
        )
        {
            if (tag is { })
            {
                tag.OwnerId = receiver.DiscordId;
                Context.Db.Tags.Update(tag);
                await Context.Db.SaveChangesAsync();

                return Reply($"{Mention.User(receiver.DiscordId)} now owns `{tag.Name}`.");
            }

            MessageReceivedEventArgs? waitConfirmResult;

            await Reply(
                string.Format(
                    "Are you sure you want to transfer all your tags to {0}? Respond with `yes` to continue.",
                    Mention.User(receiver.DiscordId)
                )
            );

            await using (Context.BeginYield())
            {
                waitConfirmResult = await Context.WaitForMessageAsync(e => e.Member.Id == Context.Author.Id);
            }

            if (waitConfirmResult is null || waitConfirmResult.Message.Content != "yes")
                return Reply("Aborted.");

            List<SbuTag> tags;

            await using (Context.BeginYield())
            {
                tags = await Context.Db.Tags.Where(t => t.OwnerId == Context.Author.Id).ToListAsync();
            }

            foreach (SbuTag dbTag in tags)
            {
                dbTag.OwnerId = receiver.DiscordId;
            }

            Context.Db.Tags.UpdateRange(tags);
            await Context.Db.SaveChangesAsync();

            return Reply($"{Mention.User(receiver.DiscordId)} now owns all of your tags.");
        }

        [Command("reserved")]
        [Description(
            "Lists the reserved keywords, tags are not allowed to be any of these keywords, but can start with, end "
            + "with or contain them."
        )]
        public DiscordCommandResult GetReservedKeywords() => Reply(
            "The following keywords are not allowed to be tags, but tags may contain them:\n"
            + string.Join("\n", SbuBotGlobals.RESERVED_KEYWORDS.Select(rn => $"> `{rn}`"))
        );
    }
}