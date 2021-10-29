using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Kkommon;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Commands.Parsing.Descriptors;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class TagModule
    {
        [Group("edit")]
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

                SbuDbContext context = Context.GetSbuDbContext();

                tag.Content = tagDescriptor.Content;
                context.Tags.Update(tag);
                await context.SaveChangesAsync();

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

                SbuDbContext context = Context.GetSbuDbContext();

                tag.Content = content.Trim();
                context.Tags.Update(tag);
                await context.SaveChangesAsync();

                return Reply("The tag has been updated.");
            }
        }
    }
}