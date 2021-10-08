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
        public sealed partial class ArchiveSubModule : SbuModuleBase
        {
            [Command("set")]
            [RequireAuthorGuildPermissions(Permission.Administrator, Group = "AdminOrManageMessagePerm")]
            [Description("Sets the current pin archive.")]
            [Usage("archive set #channel", "archive set 836993360274784297")]
            public async Task<DiscordCommandResult> SetArchiveAsync(
                [Description("The channel that should be the new archive.")]
                ITextChannel archive
            )
            {
                SbuGuild guild = await Context.GetGuildAsync();
                guild.ArchiveId = archive.Id;
                await Context.SaveChangesAsync();

                return Reply($"{archive} is now the pin archive.");
            }

            [Command("list")]
            [Description("Lists the current pin archive.")]
            [Usage("archive get")]
            public async Task<DiscordCommandResult> GetArchiveAsync()
            {
                SbuGuild guild = await Context.GetGuildAsync();

                return Reply(
                    guild.ArchiveId is null
                        ? "This guild doesn't have a pin archive channel set up. see `sbu help archive set`"
                        : $"{Mention.TextChannel(guild.ArchiveId.Value)} is the pin archive."
                );
            }
        }
    }
}