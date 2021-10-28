using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class TagModule
    {
        [Command("claim")]
        [Description("Claims the given tag if it has no owner.")]
        [Usage("tag claim tagggg", "t take whomstve")]
        public async Task<DiscordCommandResult> ClaimAsync(
            [MustBeOwned(false)][Description("The tag to claim.")]
            SbuTag tag
        )
        {
            var context = Context.GetSbuDbContext();

            tag.OwnerId = Context.Author.Id;
            context.Tags.Update(tag);
            await context.SaveChangesAsync();

            return Reply("Tag claimed.");
        }
    }
}