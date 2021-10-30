using System.Collections.Generic;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Kkommon;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class ColorRoleModule
    {
        [Command("create")]
        [RequireColorRole(false)]
        [Description("Creates a new color role.")]
        [Usage("role create #afafaf my gray role", "r make green dream role dream role")]
        public async Task<DiscordCommandResult> CreateAsync(
            [Description("The role color.")] Color color,
            [Maximum(SbuColorRole.MAX_NAME_LENGTH)]
            [Description("The role name.")]
            [Remarks("Cannot be longer than 100 characters.")]
            string? name = null
        )
        {
            if (name is null)
            {
                switch (await Context.WaitFollowUpForAsync("What do you want the role name to be?"))
                {
                    case Result<string, FollowUpError>.Success followUp:
                        name = followUp.Value;

                        if (name.Length > SbuColorRole.MAX_NAME_LENGTH)
                            return Reply("The role name must be shorter than 100 characters.");

                        break;

                    case Result<string, FollowUpError>.Error error:
                        if (error.Value == FollowUpError.Aborted)
                            return Reply("Aborted");

                        await Reply("You didn't provide a role name so i just named it after yourself.");

                        name = Context.Author.Nick ?? Context.Author.Name;
                        break;
                }
            }

            SbuDbContext context = Context.GetSbuDbContext();
            var guild = await context.GetGuildAsync(Context.Guild);
            var rolePos = Context.Guild.Roles.GetValueOrDefault(guild!.ColorRoleBottomId ?? 0)?.Position + 1;

            IRole role = await Context.Guild.CreateRoleAsync(
                r =>
                {
                    r.Color = color;
                    r.Name = name;
                }
            );

            if (rolePos is { })
            {
                if (Context.CurrentMember.GetHierarchy() <= rolePos)
                {
                    await role.DeleteAsync();
                    return Reply(string.Format(ColorRoleModule.ROLE_HAS_HIGHER_HIERARCHY_FORMAT, "move"));
                }

                await role.ModifyAsync(r => r.Position = rolePos.Value);
            }

            await Context.Author.GrantRoleAsync(role.Id);
            context.AddColorRole(role, Context.Author.Id);
            await context.SaveChangesAsync();

            return Reply($"{role.Mention} is your new color role.");
        }
    }
}