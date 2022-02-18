using System.Collections.Generic;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Kkommon;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class ColorRoleModule
    {
        [Command("create")]
        [Description("Creates a new color role.")]
        public async Task<DiscordCommandResult> CreateAsync(
            [Description("The role color.")] Color color,
            [Maximum(SbuColorRole.MAX_NAME_LENGTH)]
            [Description("The role name.")]
            [Remarks("Cannot be longer than 100 characters.")]
            [UsageOverride("my role name", "funny role")]
            string? name = null
        )
        {
            SbuDbContext context = Context.GetSbuDbContext();
            SbuMember? member = await context.GetMemberFullAsync(Context.Author);

            if (member!.ColorRole is { })
                return Reply("You must to have no color role to create one.");

            if (name is null)
            {
                switch (await Context.WaitFollowUpForAsync("What do you want the role name to be?"))
                {
                    case Result<string, FollowUpError>.Success followUp:
                    {
                        name = followUp.Value;

                        if (name.Length > SbuColorRole.MAX_NAME_LENGTH)
                            return Reply("The role name must be shorter than 100 characters.");

                        break;
                    }

                    case Result<string, FollowUpError>.Error error:
                    {
                        if (error.Value == FollowUpError.Aborted)
                            return Reply("Aborted");

                        await Response("You didn't provide a role name so i just named it after yourself.");

                        name = Context.Author.Nick ?? Context.Author.Name;
                        break;
                    }
                }
            }

            SbuGuild? guild = await context.GetGuildAsync(Context.Guild);
            int? rolePos = Context.Guild.Roles.GetValueOrDefault(guild!.ColorRoleBottomId ?? 0)?.Position + 1;

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
                    // the role is not yet added to the db so we can ignore the deletion to safe us a db call
                    await ConsistencyService.IgnoreRoleRemovedAsync(role.Id);
                    await role.DeleteAsync();
                    return Reply(SbuUtility.Format.HasHigherHierarchy("move the role"));
                }

                await role.ModifyAsync(r => r.Position = rolePos.Value);
            }

            await Context.Author.GrantRoleAsync(role.Id);
            context.AddColorRole(role, Context.Author.Id);
            await context.SaveChangesAsync();

            return Response($"{role.Mention} is your new color role.");
        }
    }
}
