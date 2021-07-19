using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Kkommon;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks;
using SbuBot.Commands.Parsing;
using SbuBot.Exceptions;
using SbuBot.Extensions;

namespace SbuBot.Commands.Modules
{
    public sealed partial class GuildManagementModule
    {
        [Command("archive")]
        [RequirePinBrigade(Group = "AdminOrPinBrigade"), RequireAdmin(Group = "AdminOrPinBrigade"),
         RequireGuild(SbuGlobals.Guild.Sbu.SELF)]
        [Description("Archives the given message or all pinned messages.")]
        [Remarks(
            "The Message are unpinned unless specified otherwise, specifying otherwise cannot be done when "
            + "replying to the message. In this case the message id/link must be used as message argument."
        )]
        [Usage(
            "archive (with {@reply})",
            "archive 836993360274784297",
            "archive https://discord.com/channels/732210852849123418/732231139233759324/836993360274784297",
            "archive all"
        )]
        public async Task<DiscordCommandResult> ArchiveMessageAsync(
            [OverrideDefault("{@reply}")][Description("The message that should be archived.")]
            OneOrAll<IUserMessage>? message = null,
            [Description("Whether or not the original message should be unpinned.")]
            bool unpinOriginal = true
        )
        {
            if (Context.Bot.GetPinArchive() is not ITextChannel pinArchive)
                throw new NotCachedException("Could not find required pin archive channel.");

            if (message is null)
            {
                if (!Context.Message.ReferencedMessage.HasValue)
                    return Reply("You need to provide a message or reply to one.");

                await _pinSingleMessageAsync(Context.Message.ReferencedMessage.Value, pinArchive, unpinOriginal);
            }
            else
            {
                switch (message)
                {
                    case OneOrAll<IUserMessage>.Specific specific:
                    {
                        if (await _pinSingleMessageAsync(specific.Value, pinArchive, unpinOriginal)
                            is Result<Unit, string>.Error error)
                            return Reply(error.Value);

                        break;
                    }

                    case OneOrAll<IUserMessage>.All:
                    {
                        IReadOnlyList<IUserMessage> pins = await Context.Channel.FetchPinnedMessagesAsync();

                        foreach (IUserMessage pinnedMessage in pins.OrderBy(m => m.CreatedAt()))
                        {
                            if (Context.Bot.StoppingToken.IsCancellationRequested)
                                throw new OperationCanceledException();

                            if (await _pinSingleMessageAsync(pinnedMessage, pinArchive, unpinOriginal)
                                is Result<Unit, string>.Error error)
                                return Reply(error.Value);
                        }

                        break;
                    }

                    // unreachable
                    default:
                        throw new();
                }
            }

            return Reply("Done.");

            static async Task<Result<Unit, string>> _pinSingleMessageAsync(
                IUserMessage message,
                ITextChannel channel,
                bool unpinOriginal
            )
            {
                switch (SbuUtility.TryCreatePinMessage(message))
                {
                    case Result<LocalMessage, string>.Success pinMessage:
                        await channel.SendMessageAsync(pinMessage);

                        if (unpinOriginal && message.IsPinned)
                            await message.UnpinAsync();

                        return new Result<Unit, string>.Success(new());

                    case Result<LocalMessage, string>.Error error:
                        return new Result<Unit, string>.Error(error.Value);

                    // unreachable
                    default:
                        throw new();
                }
            }
        }
    }
}