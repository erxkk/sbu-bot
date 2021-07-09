using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    [Description("A collection of commands for server management like pin-archival or emote creation.")]
    public sealed partial class GuildManagementModule : SbuModuleBase
    {
        [Group("config")]
        [Description("A group of commands for requesting access to restricted channels or permissions.")]
        public sealed class ConfigSubModule : SbuModuleBase
        {
            private readonly ConfigService _configService;

            public ConfigSubModule(ConfigService configService) => _configService = configService;

            [Command("set")]
            [Description("Sets a config value.")]
            public async Task<DiscordCommandResult> SetAsync(SbuGuildConfig value)
            {
                await _configService.SetValueAsync(Context.GuildId, value);
                return Reply("Set config value to true.");
            }

            [Command("unset")]
            [Description("Unsets a config value.")]
            public async Task<DiscordCommandResult> UnsetAsync(SbuGuildConfig value)
            {
                await _configService.SetValueAsync(Context.GuildId, value, false);
                return Reply("Set config value to false.");
            }
        }
    }
}