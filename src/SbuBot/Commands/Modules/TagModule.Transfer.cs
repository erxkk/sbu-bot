using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Kkommon.Exceptions;

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
            switch (tag)
            {
                case OneOrAll<SbuTag>.All:
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
                            return Reply("Aborted.");

                        case ConfirmationState.Confirmed:
                            break;

                        default:
                            throw new UnreachableException();
                    }

                    List<SbuTag> tags = await Context.GetSbuDbContext()
                        .Tags
                        .Where(t => t.OwnerId == Context.Author.Id)
                        .ToListAsync(Context.Bot.StoppingToken);

                    foreach (SbuTag dbTag in tags)
                        dbTag.OwnerId = receiver.Id;

                    Context.GetSbuDbContext().Tags.UpdateRange(tags);
                    await Context.SaveChangesAsync();

                    return Reply($"{Mention.User(receiver.Id)} now owns all of your tags.");
                }

                case OneOrAll<SbuTag>.Specific specific:
                {
                    ConfirmationState result = await ConfirmationAsync(
                        "Tag Transfer",
                        string.Format(
                            "Are you sure you want to transfer `{0}` your tags to {1}?",
                            specific.Value.Name,
                            Mention.User(receiver.Id)
                        )
                    );

                    switch (result)
                    {
                        case ConfirmationState.None:
                        case ConfirmationState.Aborted:
                            return Reply("Aborted.");

                        case ConfirmationState.Confirmed:
                            break;

                        default:
                            throw new UnreachableException();
                    }

                    specific.Value.OwnerId = receiver.Id;
                    Context.GetSbuDbContext().Tags.Update(specific.Value);
                    await Context.SaveChangesAsync();

                    return Reply($"{Mention.User(receiver.Id)} now owns `{specific.Value.Name}`.");
                }

                default:
                    throw new UnreachableException();
            }
        }
    }
}