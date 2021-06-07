using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;

using Kkommon.Extensions;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Commands.Checks.Parameters;
using SbuBot.Commands.Descriptors;
using SbuBot.Commands.Information;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    [Group("tag", "t")]
    public sealed class TagModule : SbuModuleBase
    {
        [Command]
        public DiscordCommandResult Get(SbuTag tag) => Reply(tag.Content);

        // TODO: TEST
        [Command("claim", "take")]
        public async Task<DiscordCommandResult> ClaimTagAsync([MustBeOwned(false)] SbuTag tag)
        {
            tag.OwnerId = Context.Author.Id;
            Context.Db.Tags.Update(tag);
            await Context.Db.SaveChangesAsync();

            return Reply($"You now own `{tag.Name}`.");
        }

        // TODO: TEST
        [Group("create", "make", "new")]
        public sealed class CreateGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> CreateAsync(TagDescriptor descriptor)
            {
                Context.Db.Tags.Add(new(Context.Author.Id, descriptor.Name, descriptor.Content));
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
                    return Reply("Aborted, you did not provide a tag name.");

                if (waitNameResult.Message.Content.Length.IsInRange(
                    SbuTag.MIN_NAME_LENGTH,
                    SbuTag.MAX_NAME_LENGTH,
                    rightExclusive: false
                )) return Reply($"Aborted, the tag name must be at least {SbuTag.MIN_NAME_LENGTH} characters long.");

                if (SbuBotGlobals.RESERVED_KEYWORDS.Any(
                    rn => rn.Equals(waitNameResult.Message.Content, StringComparison.OrdinalIgnoreCase)
                )) return Reply("The tag name cannot start with a reserved keyword.");

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
                    return Reply("Aborted, you did not provide tag content.");

                Context.Db.Tags.Add(
                    new(Context.Author.Id, waitNameResult.Message.Content, waitContentResult.Message.Content)
                );

                await Context.Db.SaveChangesAsync();

                return Reply("Tag created.");
            }
        }

        [Group("list"), PureGroup]
        public sealed class ListGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> ListFromOwnerAsync([OverrideDefault("@you")] IMember? owner = null)
            {
                owner ??= Context.Author;
                bool notAuthor = owner.Id != Context.Author.Id;

                IEnumerable<SbuTag> tags;

                await using (Context.BeginYield())
                {
                    tags = await Context.Db.Tags.Include(t => t.Owner)
                        .Where(t => t.Owner!.DiscordId == owner.Id)
                        .ToListAsync();
                }

                if (tags.Any())
                    return Reply($"Couldn't find any tags for {(notAuthor ? "this member" : "you")}.");

                return MaybePages(
                    tags.Select(t => $"{t.Name}\n{t.Content}"),
                    $"{(notAuthor ? $"{owner.Mention}'s" : "Your")} Tags"
                );
            }

            [Command]
            public DiscordCommandResult ListTag(SbuTag tag) => Reply(
                new LocalEmbed().WithTitle(tag.Name).WithDescription(tag.Content)
            );
        }

        // TODO: TEST
        [Command("edit", "change")]
        public async Task<DiscordCommandResult> EditAsync([AuthorMustOwn] SbuTag tag, string? newContent = null)
        {
            if (newContent is null)
            {
                MessageReceivedEventArgs? waitContentResult;
                await Reply("What do you want the new tag content to be?");

                await using (Context.BeginYield())
                {
                    waitContentResult = await Context.WaitForMessageAsync(e => e.Member.Id == Context.Author.Id);
                }

                if (waitContentResult is null)
                    return Reply("Aborted, you did not provide tag content.");

                newContent = waitContentResult.Message.Content;
            }

            tag.Content = newContent;
            Context.Db.Tags.Update(tag);
            await Context.Db.SaveChangesAsync();

            return Reply($"Tag `{tag.Name}` has been updated.");
        }

        // TODO: TEST
        [Command("remove", "delete")]
        public async Task<DiscordCommandResult> RemoveAsync([AuthorMustOwn] SbuTag tag)
        {
            Context.Db.Tags.Remove(tag);
            await Context.Db.SaveChangesAsync();

            return Reply($"Tag `{tag.Name}` has been removed.");
        }

        // TODO: TEST
        [Command("transfer")]
        public async Task<DiscordCommandResult> TransferAsync(
            [NotAuthor] SbuMember member,
            [AuthorMustOwn] SbuTag tag
        )
        {
            tag.OwnerId = member.DiscordId;
            Context.Db.Tags.Update(tag);
            await Context.Db.SaveChangesAsync();

            return Reply($"{Mention.User(member.DiscordId)} now owns `{tag.Name}`.");
        }

        [Command("reserved")]
        public DiscordCommandResult GetReservedKeywords() => Reply(
            "The following keywords are not allowed to be tags, but tags may contain them:\n"
            + string.Join("\n", SbuBotGlobals.RESERVED_KEYWORDS.Select(rn => $"> {rn}"))
        );
    }
}