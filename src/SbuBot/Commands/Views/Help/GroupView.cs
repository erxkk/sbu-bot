using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;

using Qmmands;

using SbuBot.Extensions;

namespace SbuBot.Commands.Views.Help
{
    public sealed class GroupView : HelpView
    {
        private readonly Module _group;

        public GroupView(Module group)
        {
            _group = group;

            StringBuilder description = new("**Signatures:**\n", 512);

            foreach (Command command in group.Commands)
            {
                description.Append(SbuGlobals.BULLET).Append(' ').AppendLine(Markdown.Code(command.GetSignature()));

                AddComponent(
                    new ButtonViewComponent(
                        _ =>
                        {
                            Menu.View = new CommandView(command);
                            return default;
                        }
                    )
                    {
                        Label = command.GetParameterSignature(),
                        Row = 1,
                        Style = ButtonComponentStyle.Secondary,
                    }
                );
            }

            description.AppendLine("**Description:**").Append(group.Description);

            if (group.Remarks is { })
                description.AppendLine("**Remarks:**").Append(group.Remarks);

            TemplateMessage.Embeds[0]
                .WithTitle(group.Aliases[0])
                .WithDescription(description.ToString());

            if (group.Aliases.Count != 0)
            {
                TemplateMessage.Embeds[0]
                    .AddInlineField("Aliases", string.Join(", ", group.Aliases.Select(Markdown.Code)));
            }
        }

        public override ValueTask GoToParent(ButtonEventArgs e)
        {
            if (_group.Parent is null)
                return default;

            Menu.View = new ModuleView(_group.Parent);
            return default;
        }
    }
}