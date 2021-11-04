using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

namespace SbuBot.Commands.Attributes.Checks
{
    public class RequireAttachmentAttribute : DiscordGuildCheckAttribute
    {
        public int Count { get; }
        public bool RequireExact { get; set; }

        public RequireAttachmentAttribute(int count = 1) => Count = count;

        public override ValueTask<CheckResult> CheckAsync(DiscordGuildCommandContext context)
            => RequireExact
                ? context.Message.Attachments.Count == Count
                    ? Success()
                    : Failure($"The message must have exactly {Count} attachments.")
                : context.Message.Attachments.Count >= Count
                    ? Success()
                    : Failure($"The message must have {Count} or more attachments.");
    }
}