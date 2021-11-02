using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Commands.Parsing;
using SbuBot.Commands.Views;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class TagModule
    {
        [Command("transfer")]
        [Description("Transfers ownership of a given tag to the given member.")]
        [Usage("tag transfer @user da dog", "t transfer 352815253828141056 h", "t mv @user all")]
        public async Task<DiscordCommandResult> TransferAsync(
            [NotAuthor][Description("The member that should receive the given tag.")]
            SbuMember receiver,
            [AuthorMustOwn][Description("The tag that the given member should receive.")]
            OneOrAll<SbuTag> tag
        )
        {
            if (tag.IsAll)
            {
                ConfirmationState result = await ConfirmationAsync(
                    "Tag Transfer",
                    string.Format(
                        "Are you sure you want to transfer all your tags to {0}?",
                        Mention.User(receiver.Id)
                    )
                );

                switch (result)
                {
                    case ConfirmationState.None:
                    case ConfirmationState.Aborted:
                    case ConfirmationState.TimedOut:
                        return Reply("Aborted.");

                    case ConfirmationState.Confirmed:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                SbuDbContext context = Context.GetSbuDbContext();

                List<SbuTag> tags = await context
                    .Tags
                    .Where(t => t.OwnerId == Context.Author.Id)
                    .ToListAsync(Context.Bot.StoppingToken);

                foreach (SbuTag dbTag in tags)
                    dbTag.OwnerId = receiver.Id;

                context.Tags.UpdateRange(tags);
                await context.SaveChangesAsync();

                return Reply($"{Mention.User(receiver.Id)} now owns all of your tags.");
            }
            else
            {
                ConfirmationState result = await ConfirmationAsync(
                    "Tag Transfer",
                    string.Format(
                        "Are you sure you want to transfer `{0}` your tags to {1}?",
                        tag.Value.Name,
                        Mention.User(receiver.Id)
                    )
                );

                switch (result)
                {
                    case ConfirmationState.None:
                    case ConfirmationState.Aborted:
                    case ConfirmationState.TimedOut:
                        return Reply("Aborted.");

                    case ConfirmationState.Confirmed:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                SbuDbContext context = Context.GetSbuDbContext();

                tag.Value.OwnerId = receiver.Id;
                context.Tags.Update(tag.Value);
                await context.SaveChangesAsync();

                return Reply($"{Mention.User(receiver.Id)} now owns `{tag.Value.Name}`.");
            }
        }
    }
}