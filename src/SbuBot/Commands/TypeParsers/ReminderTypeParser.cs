using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class ReminderTypeParser : SbuTypeParser<SbuReminder>
    {
        protected override async ValueTask<TypeParserResult<SbuReminder>> ParseAsync(
            Parameter parameter,
            string value,
            SbuCommandContext context
        )
        {
            ReminderService service = context.Services.GetRequiredService<ReminderService>();

            if (value.Equals("last", StringComparison.OrdinalIgnoreCase) || value == "--")
            {
                return service.CurrentReminders.Values.FirstOrDefault(r => r.OwnerId == context.Author.Id)
                    is { } queriedReminder
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