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
                command.AppendTo(description.Append(SbuGlobals.BULLET).Append(' ').Append('`'));
                description.Append('`').Append('\n');

                string paramSig = command.FormatParameters(false);

                AddComponent(
                    new ButtonViewComponent(
                        _ =>
                        {
                            Menu.View = new CommandView(command);
                            return default;
                        }
                    )
                    {
                        Label = string.IsNullOrWhiteSpace(paramSig) ? "--" : paramSig,
                        Row = 1,
                        Style = ButtonComponentStyle.Secondary,
                    }
                );
            }

            description.Append('\n').AppendLine("**Description:**").AppendLine(group.Description).Append('\n');

            if (group.Remarks is { })
                description.AppendLine("**Remarks:**").AppendLine(group.Remarks);

            TemplateMessage.Embeds[0]
                .WithTitle(group.FullAliases[0])
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