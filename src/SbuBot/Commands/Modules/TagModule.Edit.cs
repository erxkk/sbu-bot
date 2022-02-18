using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Commands.Parsing.Descriptors;
using SbuBot.Commands.Parsing.HelperTypes;
using SbuBot.Commands.Views;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class TagModule
    {
        [Command("edit")]
        [Description("Modifies the content of a given tag.")]
        public async Task<DiscordCommandResult> EditAsync(
            [Description("The tag descriptor `<name> :: <content>`.")]
            TagDescriptor tagDescriptor
        )
        {
            SbuTag? tag = await Context.GetTagAsync(tagDescriptor.Name);

            if (tag is null)
                return Reply("No tag found.");

            if (tag.OwnerId != Context.Author.Id)
                return Reply("You must be the owner of this tag.");

            SbuDbContext context = Context.GetSbuDbContext();

            tag.Content = tagDescriptor.Content;
            context.Tags.Update(tag);
            await context.SaveChangesAsync();

            return Response("The tag has been updated.");
        }

        [Command("claim")]
        [Description("Claims the given tag if it has no owner.")]
        public async Task<DiscordCommandResult> ClaimAsync(
            [MustBeOwned(false)][Description("The tag to claim.")]
            SbuTag tag
        )
        {
            SbuDbContext context = Context.GetSbuDbContext();

            tag.OwnerId = Context.Author.Id;
            context.Tags.Update(tag);
            await context.SaveChangesAsync();

            return Reply("Tag claimed.");
        }

        [Command("transfer")]
        [Description("Transfers ownership of a given tag(s) to the given member.")]
        public async Task<DiscordCommandResult> TransferAsync(
            [NotAuthor][Description("The member that should receive the given tag(s).")]
            SbuMember receiver,
            [AuthorMustOwn][Description("The tag that the given member should receive.")]
            OneOrAll<SbuTag> tag
        )
        {
            if (tag.IsAll)
            {
                ConfirmationState result = await AgreementAsync(
                    new()
                    {
                        Context.Author.Id,
                        receiver.Id,
                    },
                    string.Format(
                        "Are you sure you want to transfer **all** your tags to {0}?",
                        Mention.User(receiver.Id)
                    )
                );

                switch (result)
                {
                    case ConfirmationState.None:
                    case ConfirmationState.Aborted:
                        return null!;

                    case ConfirmationState.TimedOut:
                        return Reply("Aborted.");

                    case ConfirmationState.Confirmed:
                        SbuDbContext context = Context.GetSbuDbContext();

                        List<SbuTag> tags = await context
                            .Tags
                            .Where(t => t.OwnerId == Context.Author.Id)
                            .ToListAsync(Bot.StoppingToken);

                        foreach (SbuTag dbTag in tags)
                            dbTag.OwnerId = receiver.Id;

                        context.Tags.UpdateRange(tags);
                        await context.SaveChangesAsync();

                        return Response($"{Mention.User(receiver.Id)} now owns all of your tags.");

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                ConfirmationState result = await AgreementAsync(
                    new()
                    {
                        Context.Author.Id,
                        receiver.Id,
                    },
                    "Do you accept the tag transfer?"
                );

                switch (result)
                {
                    case ConfirmationState.None:
                    case ConfirmationState.Aborted:
                        return null!;

                    case ConfirmationState.TimedOut:
                        return Reply("Aborted.");

                    case ConfirmationState.Confirmed:
                        SbuDbContext context = Context.GetSbuDbContext();

                        tag.Value.OwnerId = receiver.Id;
                        context.Tags.Update(tag.Value);
                        await context.SaveChangesAsync();

                        return Response($"{Mention.User(receiver.Id)} now owns `{tag.Value.Name}`.");

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
