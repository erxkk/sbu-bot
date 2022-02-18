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
        [Command("delete")]
        [Description("Removes a given tag.")]
        public async Task<DiscordCommandResult> RemoveAsync(
            [AuthorMustOwn][Description("The tag that should be removed.")]
            OneOrAll<SbuTag> tag
        )
        {
            if (tag.IsAll)
            {
                ConfirmationState result = await ConfirmationAsync(
                    "Tag Removal",
                    "Are you sure you want to remove **all** your tags?"
                );

                switch (result)
                {
                    case ConfirmationState.None:
                    case ConfirmationState.Aborted:
                        return null!;

                    case ConfirmationState.TimedOut:
                        return Reply("Aborted.");

                    case ConfirmationState.Confirmed:

                        SbuDbContext context = Context.GetSbuDbContext();

                        List<SbuTag> tags = await context.Tags
                            .Where(t => t.OwnerId == Context.Author.Id && t.GuildId == Context.GuildId)
                            .ToListAsync(Bot.StoppingToken);

                        context.Tags.RemoveRange(tags);
                        await context.SaveChangesAsync();

                        return Response("All tags removed.");

                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
                        return null!;

                    case ConfirmationState.TimedOut:
                        return Reply("Aborted.");

                    case ConfirmationState.Confirmed:

                        SbuDbContext context = Context.GetSbuDbContext();

                        context.Tags.Remove(tag.Value);
                        await context.SaveChangesAsync();

                        return Response("Tag removed.");

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        [Group("purge")]
        [RequireAuthorChannelPermissions(Permission.Administrator)]
        [Remarks("Requires Administrator Permission.")]
        public sealed class PurgeSubModule : SbuModuleBase
        {
            [Command]
            [Description("Removes a given tag without owner restrictions.")]
            public async Task<DiscordCommandResult> RemoveOverrideAsync(
                [Description("The tag that should be removed.")]
                OneOrAll<SbuTag> tag
            )
            {
                if (tag.IsAll)
                {
                    ConfirmationState result = await ConfirmationAsync(
                        "Complete Tag Removal",
                        "Are you sure you want to remove **all** of this server's tags?"
                    );

                    switch (result)
                    {
                        case ConfirmationState.None:
                        case ConfirmationState.Aborted:
                            return null!;

                        case ConfirmationState.TimedOut:
                            return Reply("Aborted.");

                        case ConfirmationState.Confirmed:
                            SbuDbContext context = Context.GetSbuDbContext();

                            List<SbuTag> tags = await context.Tags
                                .Where(t => t.GuildId == Context.GuildId)
                                .ToListAsync(Bot.StoppingToken);

                            context.Tags.RemoveRange(tags);
                            await context.SaveChangesAsync();

                            return Response("All tags removed.");

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
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
                            return null!;

                        case ConfirmationState.TimedOut:
                            return Reply("Aborted.");

                        case ConfirmationState.Confirmed:
                            SbuDbContext context = Context.GetSbuDbContext();

                            context.Tags.Remove(tag.Value);
                            await context.SaveChangesAsync();

                            return Response("Tag removed.");

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            [Command("for")]
            [Description("Removes a given owners tag without owner restrictions.")]
            public async Task<DiscordCommandResult> RemoveOverrideMemberAsync(
                [Description("The owner who's tags should be removed.")]
                SbuMember owner
            )
            {
                ConfirmationState result = await ConfirmationAsync(
                    "Tag Removal",
                    $"Are you sure you want to remove **all** of {Mention.User(owner.Id)}'s tags?"
                );

                switch (result)
                {
                    case ConfirmationState.None:
                    case ConfirmationState.Aborted:
                        return null!;

                    case ConfirmationState.TimedOut:
                        return Reply("Aborted.");

                    case ConfirmationState.Confirmed:
                        SbuDbContext context = Context.GetSbuDbContext();

                        List<SbuTag> tags = await context.Tags
                            .Where(t => t.OwnerId == owner.Id)
                            .ToListAsync(Bot.StoppingToken);

                        context.Tags.RemoveRange(tags);
                        await context.SaveChangesAsync();

                        return Response($"All of {Mention.User(owner.Id)}'s tags removed.");

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
