using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.Parsing.Descriptors;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class TagModule
    {
        [Command("create")]
        [Description("Creates a new tag with the given name and content.")]
        public async Task<DiscordCommandResult> CreateAsync(
            [Description("The tag descriptor `<name> :: <content>`.")]
            TagDescriptor descriptor
        )
        {
            if (await Context.GetTagAsync(descriptor.Name) is { })
                return Reply("A tag with same name already exists.");

            SbuDbContext context = Context.GetSbuDbContext();

            context.AddTag(
                Context.Author.Id,
                Context.Guild.Id,
                descriptor.Name,
                descriptor.Content
            );

            await context.SaveChangesAsync();

            return Response("Tag created.");
        }
    }
}
