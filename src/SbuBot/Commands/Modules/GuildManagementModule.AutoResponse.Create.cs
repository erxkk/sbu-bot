using System.Threading.Tasks;

using Disqord.Bot;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Parsing.Descriptors;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    public sealed partial class GuildManagementModule
    {
        public sealed partial class AutoResponseSubModule
        {
            [Command("create")]
            [Description("Creates a new auto response.")]
            [UsageOverride(
                "auto create refuses to elaborate :: https://cdn.discordapp.com/attachments/820403561526722570/"
                + "848874138587758592/Screenshot_20210530_010226.png",
                "auto make what da dog doin :: what is the canine partaking in",
                "auto mk h :: h"
            )]
            public async Task<DiscordCommandResult> CreateAsync(
                [Description("The auto response descriptor `<trigger> :: <response>`.")]
                AutoResponseDescriptor descriptor
            )
            {
                ChatService service = Context.Services.GetRequiredService<ChatService>();

                if (service.GetAutoResponse(Context.GuildId, descriptor.Trigger) is { })
                    return Reply("An auto response with same name already exists.");

                await service.SetAutoResponseAsync(
                    Context.Guild.Id,
                    descriptor.Trigger,
                    descriptor.Response
                );

                return Response("Auto response created.");
            }
        }
    }
}
