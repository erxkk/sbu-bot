using System;
using System.Linq;
using System.Threading.Tasks;

using Disqord.Bot;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Extensions;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class ReminderTypeParser : DiscordGuildTypeParser<SbuReminder>
    {
        public static readonly string[] ACCEPTED_KEYWORDS = { "last" };

        public override async ValueTask<TypeParserResult<SbuReminder>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordGuildCommandContext context
        )
        {
            ReminderService service = context.Services.GetRequiredService<ReminderService>();
            SbuMember owner = await context.GetSbuDbContext().GetSbuMemberAsync(context.Author);
            SbuGuild guild = await context.GetSbuDbContext().GetSbuGuildAsync(context.Guild);

            if (value.Equals("last", StringComparison.OrdinalIgnoreCase))
            {
                return service.CurrentReminders.Values.FirstOrDefault(
                    r => r.OwnerId == owner.Id && r.GuildId == guild.Id
                ) is { } queriedReminder
                    ? TypeParser<SbuReminder>.Success(queriedReminder)
                    : TypeParser<SbuReminder>.Failure("Could not find reminder.");
            }

            TypeParser<Guid> guidParser = context.Bot.Commands.GetTypeParser<Guid>();

            if (await guidParser.ParseAsync(parameter, value, context) is not { IsSuccessful: true } guidParseResult)
                return TypeParser<SbuReminder>.Failure("Could not parse reminder.");

            return service.CurrentReminders.TryGetValue(guidParseResult.Value, out var indexedReminder)
                ? TypeParser<SbuReminder>.Success(indexedReminder)
                : TypeParser<SbuReminder>.Failure("Could not find reminder.");
        }
    }
}