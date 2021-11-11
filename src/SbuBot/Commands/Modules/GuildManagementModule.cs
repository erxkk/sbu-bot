using System;
using System.Text;
using System.Threading.Tasks;

using Disqord;
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

            [Command]
            [Description("Lists the the config values for this guild.")]
            public DiscordCommandResult View()
            {
                SbuGuildConfig config = _configService.GetConfig(Context.GuildId);
                string[] names = Enum.GetNames<SbuGuildConfig>();
                SbuGuildConfig[] values = Enum.GetValues<SbuGuildConfig>();

                StringBuilder builder = new();

                for (var i = 0; i < names.Length; i++)
                {
                    if (values[i] is SbuGuildConfig.None or SbuGuildConfig.All)
                        continue;

                    builder.Append(SbuGlobals.BULLET)
                        .Append(' ')
                        .Append(names[i])
                        .Append(':')
                        .Append(' ')
                        .AppendLine(config.HasFlag(values[i]) ? "enabled" : "disabled");
                }

                return Reply(new LocalEmbed().WithDescription(builder.ToString()));
            }

            [Command("set")]
            [Description("Sets a config value.")]
            public async Task<DiscordCommandResult> SetAsync(SbuGuildConfig value)
            {
                await _configService.SetValueAsync(Context.GuildId, value);
                return Reply($"Enabled {value}.");
            }

            [Command("unset")]
            [Description("Unsets a config value.")]
            public async Task<DiscordCommandResult> UnsetAsync(SbuGuildConfig value)
            {
                await _configService.SetValueAsync(Context.GuildId, value, false);
                return Reply($"Disabled {value}.");
            }
        }
    }
}