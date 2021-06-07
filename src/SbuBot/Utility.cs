using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Disqord;
using Disqord.Gateway;

using Kkommon;

namespace SbuBot
{
    public static class Utility
    {
        private static readonly Random RANDOM = new();

        public const int RANDOM_STRING_LENGTH = 5;
        public static readonly char[] RANDOM_SOURCE_CHARS = "abcdefghijklmnopkrstuvwxyz1234567890_-!?*".ToCharArray();

        public static Result<LocalMessage, string> TryCreatePinMessage(IUserMessage message, bool force)
        {
            LocalEmbed embed = new LocalEmbed()
                .WithAuthor(message.Author)
                .AddField(
                    "Link to Original",
                    Markdown.Link(
                        "Click here!",
                        Discord.MessageJumpLink(SbuBotGlobals.Guild.ID, message.ChannelId, message.Id)
                    )
                )
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

        public static string GeneratePseudoRandomString()
        {
            StringBuilder buffer = new(Utility.RANDOM_STRING_LENGTH);

            for (int i = 0; i < Utility.RANDOM_STRING_LENGTH; i++)
                buffer.Append(Utility.RANDOM_SOURCE_CHARS[Utility.RANDOM.Next(0, Utility.RANDOM_SOURCE_CHARS.Length)]);

            return buffer.ToString();
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
                    throw new ArgumentOutOfRangeException(
                        nameof(source),
                        item.Length,
                        $"An item in the collection was longer than maximum page length of {maxPageLength}."
                    );

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

            if (sbuMember.GuildId != SbuBotGlobals.Guild.ID)
                throw new ArgumentException("SbuMember must be from SBU.", nameof(sbuMember));

            if (sbuMember is not IGatewayEntity)
                throw new ArgumentException("SbuMember must be of type IGatewayEntity.", nameof(sbuMember));

            return sbuMember.GetRoles()
                .Values.OrderByDescending(role => role.Position)
                .FirstOrDefault(role => role.Color is { });
        }

        public static IMember? GetSbuColorRoleOwner(IRole sbuColorRole)
        {
            if (sbuColorRole is null)
                throw new ArgumentNullException(nameof(sbuColorRole));

            if (sbuColorRole.GuildId != SbuBotGlobals.Guild.ID)
                throw new ArgumentException("SbuColorRole must be from SBU.", nameof(sbuColorRole));

            if (sbuColorRole is not IGatewayEntity)
                throw new ArgumentException("SbuColorRole must be of type IGatewayEntity.", nameof(sbuColorRole));

            return sbuColorRole.GetGatewayClient()
                .GetGuild(sbuColorRole.GuildId)
                .Members.Values.FirstOrDefault(m => m.RoleIds.Contains(sbuColorRole.Id));
        }

        public static bool IsSbuColorRole(IRole sbuColorRole)
        {
            if (sbuColorRole is null)
                throw new ArgumentNullException(nameof(sbuColorRole));

            if (sbuColorRole.GuildId != SbuBotGlobals.Guild.ID)
                throw new ArgumentException("SbuColorRole must be from SBU.", nameof(sbuColorRole));

            if (sbuColorRole is not IGatewayEntity)
                throw new ArgumentException("SbuColorRole must be of type IGatewayEntity.", nameof(sbuColorRole));

            if (!sbuColorRole.GetGatewayClient()
                .GetGuild(sbuColorRole.GuildId)
                .Roles.TryGetValue(SbuBotGlobals.Roles.Categories.COLOR, out var colorSeparatorRole))
                throw new RequiredCacheException("Could not find required color separator role in cache.");

            return sbuColorRole.Position < colorSeparatorRole.Position && sbuColorRole.Color is { };
        }
    }
}