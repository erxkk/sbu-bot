using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Disqord;
using Disqord.Gateway;

using Kkommon;

using SbuBot.Extensions;

namespace SbuBot
{
    public static class SbuUtility
    {
        public static readonly Regex IMAGE_FILE_REGEX = new(@"\.(gif|jpeg|jpg|png)$", RegexOptions.Compiled);

        public static Result<LocalMessage, string> TryCreatePinMessage(IUserMessage message)
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
                        Discord.MessageJumpLink(SbuGlobals.Guild.SELF, message.ChannelId, message.Id)
                    ),
                    true
                );

            LocalMessage pinMessage = new LocalMessage().WithEmbed(embed);

            if (message.Embeds.Count != 0)
            {
                if (message.Embeds[0].Image is { } image)
                    embed.WithImageUrl(image.Url);
                else if (message.Embeds[0].Video is { } video)
                    embed.AddField("Video-Url", Markdown.Link("Click here!", video.Url), true);
            }
            else if (message.Attachments.Count != 0)
            {
                Attachment attachment = message.Attachments[0];

                if (SbuUtility.IMAGE_FILE_REGEX.IsMatch(attachment.FileName))
                    embed.WithImageUrl(attachment.Url);
                else
                    embed.AddField("Unknown-Media-Type-Url", Markdown.Link("Click here!", attachment.Url), true);
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

        public static IEnumerable<string> FillPages(
            IEnumerable<string> source,
            int maxElementsPerPage = -1,
            int maxPageLength = 2048
        )
        {
            var builder = new StringBuilder();
            var elements = 0;

            foreach (string item in source)
            {
                if (item.Length + 1 > maxPageLength)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(source),
                        item.Length,
                        $"An item in the collection was longer than maximum page length of {maxPageLength}."
                    );
                }

                if ((maxElementsPerPage == -1 || elements <= maxElementsPerPage)
                    && builder.Length + item.Length + 1 <= maxPageLength)
                {
                    elements++;
                    builder.AppendLine(item);

                    continue;
                }

                yield return builder.ToString();

                elements = 0;
                builder = new StringBuilder().AppendLine(item);
            }

            if (builder.Length == 0)
                throw new ArgumentException("Source cannot be empty", nameof(source));

            yield return builder.ToString();
        }
    }
}