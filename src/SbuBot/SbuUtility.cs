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

        public static int CustomEmojiSlots(IGuild guild) => guild.BoostTier switch
        {
            BoostTier.None => 50,
            BoostTier.First => 100,
            BoostTier.Second => 150,
            BoostTier.Third => 250,
            _ => throw new ArgumentOutOfRangeException(nameof(guild), guild.BoostTier, null),
        };

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
                idPair = (ulong.Parse(matches.Groups[1].Value), ulong.Parse(matches.Groups[2].Value));
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

        public static IRole? GetSbuColorRole(IMember sbuMember)
        {
            if (sbuMember is null)
                throw new ArgumentNullException(nameof(sbuMember));

            if (sbuMember.GuildId != SbuGlobals.Guild.SELF)
                throw new ArgumentException("SbuMember must be from SBU.", nameof(sbuMember));

            return sbuMember.GetRoles()
                .Values.OrderByDescending(role => role.Position)
                .FirstOrDefault(role => role.Color is { });
        }

        public static IMember? GetSbuColorRoleOwner(IRole sbuColorRole)
        {
            if (sbuColorRole is null)
                throw new ArgumentNullException(nameof(sbuColorRole));

            if (sbuColorRole.GuildId != SbuGlobals.Guild.SELF)
                throw new ArgumentException("SbuColorRole must be from SBU.", nameof(sbuColorRole));

            return sbuColorRole.GetGatewayClient()
                .GetGuild(sbuColorRole.GuildId)
                .Members.Values.FirstOrDefault(m => m.RoleIds.Contains(sbuColorRole.Id));
        }

        public static bool IsSbuColorRole(IRole sbuColorRole, SbuBot bot)
        {
            if (sbuColorRole is null)
                throw new ArgumentNullException(nameof(sbuColorRole));

            if (sbuColorRole.GuildId != SbuGlobals.Guild.SELF)
                throw new ArgumentException("SbuColorRole must be from SBU.", nameof(sbuColorRole));

            return sbuColorRole.Position < bot.GetColorRoleSeparator().Position && sbuColorRole.Color is { };
        }
    }
}