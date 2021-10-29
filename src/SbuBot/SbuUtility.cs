using System.Text.RegularExpressions;

using Disqord;

using Kkommon;

namespace SbuBot
{
    public static class SbuUtility
    {
        public static readonly Regex IMAGE_FILE_REGEX = new(@"\.(gif|jpeg|jpg|png)$", RegexOptions.Compiled);

        // TODO: handout 2 messages in case of videos  so those are easily viewable
        public static Result<LocalMessage, string> TryCreatePinMessage(Snowflake guildId, IUserMessage message)
        {
            LocalEmbed embed = new LocalEmbed()
                .WithAuthor(message.Author)
                .WithDescription(message.Content)
                .WithFooter("Original posted")
                .WithTimestamp(message.CreatedAt())
                .AddField(
                    "Link to Original",
                    Markdown.Link(
                        "Click here!",
                        Discord.MessageJumpLink(guildId, message.ChannelId, message.Id)
                    ),
                    true
                );

            LocalMessage pinMessage = new LocalMessage().WithEmbeds(embed);

            if (message.Embeds.Count != 0)
            {
                IEmbed userEmbed = message.Embeds[0];

                // use image type comparison to make sure it's actually an image that is being set
                // marked as deprecated by discord
                if (userEmbed.Type == "image" && (userEmbed.Url ?? userEmbed.Image?.Url) is { } url)
                    embed.WithImageUrl(url);
                else if (userEmbed.Video is { } video)
                    embed.AddField("Video-Url", Markdown.Link("Click here!", video.Url), true);
            }
            else if (message.Attachments.Count != 0)
            {
                IAttachment attachment = message.Attachments[0];

                if (IMAGE_FILE_REGEX.IsMatch(attachment.FileName))
                    embed.WithImageUrl(attachment.Url);
                else
                    embed.AddField("Unknown-Media-Url", Markdown.Link("Click here!", attachment.Url), true);
            }

            return new Result<LocalMessage, string>.Success(pinMessage);
        }

        public static bool TryParseMessageLink(string value, out (Snowflake ChannelId, Snowflake MessageId) idPair)
        {
            if (value.Length >= 76 && Discord.MessageJumpLinkRegex.Match(value) is { Success: true } matches)
            {
                idPair = (
                    ulong.Parse(matches.Groups["channel_id"].Value),
                    ulong.Parse(matches.Groups["message_id"].Value)
                );

                return true;
            }

            idPair = default;
            return false;
        }
    }
}