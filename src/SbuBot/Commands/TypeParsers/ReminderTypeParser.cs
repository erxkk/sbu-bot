using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class ReminderTypeParser : SbuTypeParserBase<SbuReminder>
    {
        public static readonly string[] ACCEPTED_KEYWORDS = { "last" };

        protected override async ValueTask<TypeParserResult<SbuReminder>> ParseAsync(
            Parameter parameter,
            string value,
            SbuCommandContext context
        )
        {
            ReminderService service = context.Services.GetRequiredService<ReminderService>();
            SbuMember owner = await context.GetOrCreateMemberAsync();
            SbuGuild guild = await context.GetOrCreateGuildAsync();

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