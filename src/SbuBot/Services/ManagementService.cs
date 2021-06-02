using Disqord;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using Disqord.Gateway;

using Kkommon;

using Microsoft.Extensions.Logging;

namespace SbuBot.Services
{
    public sealed class ManagementService : DiscordBotService
    {
        public ManagementService(ILogger<ManagementService> logger, DiscordBotBase bot) : base(logger, bot) { }

        public Result<LocalMessage, string> TryCreatePinMessage(IGatewayUserMessage message, bool force)
        {
            LocalEmbed embed = new LocalEmbed()
                .WithAuthor(message.Author)
                .AddField("Link to Original", Markdown.Link("Click here!", Utility.GetJumpUrl(message)))
                .WithFooter("Posted at")
                .WithTimestamp(message.CreatedAt());

            LocalMessage pinMessage = new LocalMessage().WithEmbed(embed);

            if (message.Embeds.Count != 0 && !force)
                return new Result<LocalMessage, string>.Error("Could not determine content embed type.");

            if (message.Embeds[0].Image is { } image)
                embed.WithImageUrl(image.Url);
            else if (message.Embeds[0].Video is { } video)
                pinMessage.WithContent($"{video.Url}");
            else if (!force)
                return new Result<LocalMessage, string>.Error("Could not determine content embed type.");

            return new Result<LocalMessage, string>.Success(pinMessage);
        }
    }
}