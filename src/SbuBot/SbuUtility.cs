using System.Linq;
using System.Text.RegularExpressions;

using Disqord;
using Disqord.Bot;

using Kkommon;

using Qmmands;

using SbuBot.Commands;
using SbuBot.Extensions;

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

        public static string? FormatFailureReason(DiscordCommandContext context, FailedResult result)
        {
            return result switch
            {
                CommandNotFoundResult => null,
                TypeParseFailedResult parseFailedResult => string.Format(
                    "Type parse failed for parameter `{0}`:\n• {1}",
                    parseFailedResult.Parameter.Format(false),
                    parseFailedResult.FailureReason
                ),
                ChecksFailedResult checksFailed => string.Format(
                    "Checks failed:\n{0}",
                    checksFailed.FailedChecks.Select((c => $"• {c.Result.FailureReason}")).ToNewLines()
                ),
                ParameterChecksFailedResult parameterChecksFailed => string.Format(
                    "Checks failed for parameter `{0}`:\n{1}",
                    parameterChecksFailed.Parameter.Format(false),
                    parameterChecksFailed.FailedChecks.Select((c => $"• {c.Result.FailureReason}")).ToNewLines()
                ),
                _ => result.FailureReason,
            };
        }

        public static LocalMessage? FormatFailureMessage(DiscordCommandContext context, FailedResult result)
        {
            string? description = FormatFailureReason(context, result);

            if (description is null)
                return null;

            LocalEmbed embed = new LocalEmbed().WithDescription(description).WithColor(3092790);

            if (result is OverloadsFailedResult overloadsFailed)
            {
                foreach ((Command overload, FailedResult overloadResult) in overloadsFailed.FailedOverloads)
                {
                    string? reason = FormatFailureReason(context, overloadResult);

                    if (reason is { })
                        embed.AddField(string.Format("Overload: {0}", overload.FullAliases[0]), reason);
                }
            }
            else if (context.Command is { })
            {
                embed.WithTitle(string.Format("Command: {0}", context.Command.FullAliases[0]));
            }

            return new LocalMessage().WithEmbeds(embed);
        }
    }
}