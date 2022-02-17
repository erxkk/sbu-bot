using System.Text.RegularExpressions;

using Disqord;

using Kkommon;

namespace SbuBot
{
    public static class SbuUtility
    {
        public static readonly Regex IMAGE_FILE_REGEX = new(@"\.(gif|jpeg|jpg|png)$", RegexOptions.Compiled);

        public static Result<(LocalMessage?, LocalMessage), string> TryCreatePinMessage(
            Snowflake guildId,
            IUserMessage message
        )
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

            LocalMessage? videoMessage = null;
            LocalMessage pinMessage = new LocalMessage().WithEmbeds(embed);

            if (message.Attachments.Count != 0)
            {
                IAttachment attachment = message.Attachments[0];

                if (SbuUtility.IMAGE_FILE_REGEX.IsMatch(attachment.FileName))
                    embed.WithImageUrl(attachment.Url);
                else
                    videoMessage = new LocalMessage().WithContent(attachment.Url);
            }
            else if (message.Embeds.Count != 0)
            {
                IEmbed userEmbed = message.Embeds[0];

                // Discord is really making it hard to get proper media info about a message
                if ((userEmbed.Type is "image" or "gifv") && (userEmbed.Url ?? userEmbed.Image?.Url) is { } url)
                    embed.WithImageUrl(url);
                else if (userEmbed.Image is { } image)
                    embed.WithImageUrl(image.Url);
                else if (userEmbed.Video is { })
                    videoMessage = new LocalMessage().WithContent(userEmbed.Url);
            }

            return new Result<(LocalMessage?, LocalMessage), string>.Success((videoMessage, pinMessage));
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

        public static string Truncate(string text, int length)
        {
            Preconditions.Greater(length, 0, nameof(length));

            return text.Length >= length + 1 ? text.Substring(0, length - 1) + SbuGlobals.ELLIPSES : text;
        }

        public static class Format
        {
            public static string DoesNotExist(string value, string? suffix = null)
                => string.Format("{0} doesn't exist{1}", value, suffix is null ? "." : $", {suffix}");

            public static string HasHigherHierarchy(string action, string value = "it", string? suffix = null)
                => string.Format(
                    "Can't {0}, {1} has higher hierarchy{2}",
                    action,
                    value,
                    suffix is null ? "." : $", {suffix}"
                );
        }
    }
}
