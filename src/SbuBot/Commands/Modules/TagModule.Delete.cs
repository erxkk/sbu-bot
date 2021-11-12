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
    public sealed partial class TagModule
    {
        [Group("delete")]
        [Description("Removes a given tag.")]
        public sealed class TagGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> RemoveAsync(
                [AuthorMustOwn][Description("The tag that should be removed.")]
                OneOrAll<SbuTag> tag
            )
            {
                if (tag.IsAll)
                {
                    ConfirmationState result = await ConfirmationAsync(
                        "Tag Removal",
                        "Are you sure you want to remove all your tags?"
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

                    List<SbuTag> tags = await context.Tags
                        .Where(t => t.OwnerId == Context.Author.Id && t.GuildId == Context.GuildId)
                        .ToListAsync(Context.Bot.StoppingToken);

                    context.Tags.RemoveRange(tags);
                    await context.SaveChangesAsync();

                    return Response("All tags removed.");
                }
                else
                {
                    ConfirmationState result = await ConfirmationAsync(
                        "Tag Removal",
                        "Are you sure you want to remove this tag?"
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

                    context.Tags.Remove(tag.Value);
                    await context.SaveChangesAsync();

                    return Response("Tag removed.");
                }
            }

            [Command]
            [RequireAuthorChannelPermissions(Permission.Administrator)]
            [Description("Removes a given tag.")]
            [Remarks("Requires Administrator Permission.")]
            public async Task<DiscordCommandResult> RemoveOverrideAsync(
                [Description("The tag that should be removed.")]
                OneOrAll<SbuTag> tag
            )
            {
                if (tag.IsAll)
                {
                    ConfirmationState result = await ConfirmationAsync(
                        "Complete Tag Removal",
                        "Are you sure you want to remove **all** tags?"
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

                    List<SbuTag> tags = await context.Tags
                        .Where(t => t.GuildId == Context.GuildId)
                        .ToListAsync(Context.Bot.StoppingToken);

                    context.Tags.RemoveRange(tags);
                    await context.SaveChangesAsync();

                    return Response("All tags removed.");
                }
                else
                {
                    ConfirmationState result = await ConfirmationAsync(
                        "Tag Removal",
                        "Are you sure you want to remove this tag?"
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

                    context.Tags.Remove(tag.Value);
                    await context.SaveChangesAsync();

                    return Response("Tag removed.");
                }
            }
        }
    }
}
