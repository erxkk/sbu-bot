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

using SbuBot.Commands.Checks;
using SbuBot.Commands.Information;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    [Group("tag", "t")]
    public sealed class TagModule : SbuModuleBase
    {
        [Command]
        public DiscordCommandResult GetTag(SbuTag tag) => Reply(tag.Content);

        [Command("create", "new"), RequireAuthorInDb]
        public async Task<DiscordCommandResult> CreateTagAsync(
            [Minimum(SbuTag.MIN_NAME_LENGTH)] string name,
            string content
        )
        {
            SbuTag? tag;

            await using (Context.BeginYield())
            {
                tag = await Context.Db.Tags.FirstOrDefaultAsync(t => t.Name == name);
            }

            if (tag is { })
                return Reply("Tag with same name already exists.");

            if (SbuBotGlobals.RESERVED_NAMES.Any(rn => rn.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return Reply("The tag name cannot start with a reserved keyword.");

            await using (Context.BeginYield())
            {
                Context.Db.Tags.Add(new(Context.Author.Id, name, content));
                await Context.Db.SaveChangesAsync();
            }

            return Reply("Tag created.");
        }

        [Command("make"), RequireAuthorInDb]
        public async Task<DiscordCommandResult> CreateTagInteractiveAsync()
        {
            MessageReceivedEventArgs? waitNameResult;

            await Reply("How should the tag be called? (spaces are allowed)");

            await using (Context.BeginYield())
            {
                waitNameResult = await Context.WaitForMessageAsync(e => e.Member.Id == Context.Author.Id);
            }

            if (waitNameResult is null)
                return Reply("Aborted, you did not provide a tag name.");

            if (waitNameResult.Message.Content.Length < SbuTag.MIN_NAME_LENGTH)
                return Reply($"Aborted, the tag name must be at least {SbuTag.MIN_NAME_LENGTH} characters long.");

            if (SbuBotGlobals.RESERVED_NAMES.Any(
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

            await using (Context.BeginYield())
            {
                Context.Db.Tags.Add(
                    new(Context.Author.Id, waitNameResult.Message.Content, waitContentResult.Message.Content)
                );

                await Context.Db.SaveChangesAsync();
            }

            return Reply("Tag created.");
        }

        [Group("list", "show", "l")]
        public sealed class ListGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> ListOwnerTagsAsync([OverrideDefault("you")] IMember? owner = null)
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
            public async Task<DiscordCommandResult> ListTagsAsync(string name)
            {
                SbuTag? tag;

                await using (Context.BeginYield())
                {
                    tag = await Context.Db.Tags.FirstOrDefaultAsync(t => t.Name == name);
                }

                return tag is null
                    ? Reply("No tag with this name exists.")
                    : Reply(new LocalEmbed().WithTitle("Tag").WithDescription($"{tag.Name}\n{tag.Content}"));
            }
        }
    }
}