using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class GuildManagementModule
    {
        public sealed partial class ArchiveSubModule
        {
            [Command("set")]
            [RequireAuthorGuildPermissions(Permission.Administrator)]
            [Description("Sets the current pin archive.")]
            [UsageOverride("archive set #channel", "archive set 836993360274784297")]
            public async Task<DiscordCommandResult> SetArchiveAsync(
                [Description("The channel that should be the new archive.")]
                ITextChannel archive
            )
            {
                SbuDbContext context = Context.GetSbuDbContext();

                SbuGuild? guild = await context.GetGuildAsync(Context.Guild);
                guild!.ArchiveId = archive.Id;
                await context.SaveChangesAsync();

                return Reply($"{archive} is now the pin archive.");
            }

            [Command("list")]
            [Description("Lists the current pin archive.")]
            [UsageOverride("archive list")]
            public async Task<DiscordCommandResult> GetArchiveAsync()
            {
                SbuGuild guild = await Context.GetDbGuildAsync();

                return Reply(
                    guild.ArchiveId is null
                        ? "This guild doesn't have a pin archive channel set up. see `sbu help archive set`"
                        : $"{Mention.Channel(guild.ArchiveId.Value)} is the pin archive."
                );
            }
        }
    }
}
