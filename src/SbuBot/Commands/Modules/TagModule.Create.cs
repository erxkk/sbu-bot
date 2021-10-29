using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Kkommon;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Parsing.Descriptors;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class TagModule
    {
        [Group("create")]
        [Description("Creates a new tag with the given name and content.")]
        public sealed class CreateGroup : SbuModuleBase
        {
            [Command]
            [Usage("tag create tagggg :: new tag who dis", "t make da dog :: what da dog doin", "tag mk h :: h")]
            public async Task<DiscordCommandResult> CreateAsync(
                [Description("The tag descriptor.")] TagDescriptor descriptor
            )
            {
                if (await Context.GetTagAsync(descriptor.Name) is { })
                    return Reply("A tag with same name already exists.");

                var context = Context.GetSbuDbContext();

                context.AddTag(
                    Context.Author.Id,
                    Context.Guild.Id,
                    descriptor.Name,
                    descriptor.Content
                );

                await context.SaveChangesAsync();

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

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (SbuTag.IsValidTagName(name))
                {
                    case SbuTag.ValidNameType.TooShort:
                        return Reply(
                            $"Aborted: The tag name must be at least {SbuTag.MIN_NAME_LENGTH} characters long."
                        );

                    case SbuTag.ValidNameType.TooLong:
                        return Reply(
                            $"Aborted: The tag name can be at most {SbuTag.MAX_NAME_LENGTH} characters long."
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
                                $"Aborted: The tag content can be at most {SbuTag.MAX_CONTENT_LENGTH} characters long."
                            );
                        }

                        break;

                    case Result<string, FollowUpError>.Error error:
                        return Reply(
                            error.Value == FollowUpError.Aborted
                                ? "Aborted."
                                : "Aborted: You did not provide tag content."
                        );

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var context = Context.GetSbuDbContext();

                context.AddTag(
                    Context.Author.Id,
                    Context.Guild.Id,
                    name,
                    content
                );

                await context.SaveChangesAsync();

                return Reply("Tag created.");
            }
        }
    }
}