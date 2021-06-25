using System.Linq;
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

            TemplateMessage.Embeds[0]
                .WithTitle(group.Aliases[0])
                .WithDescription(
                    string.Format(
                        "**Signatures:**\n{0}\n**Description:**\n{1}\n**Remarks:**\n{2}",
                        string.Join(
                            "\n",
                            group.Commands.Select(c => $"{SbuGlobals.BULLET} {Markdown.Code(c.GetSignature())}")
                        ),
                        group.Description ?? "`--`",
                        group.Remarks ?? "`--`"
                    )
                );

            if (group.Aliases.Count != 0)
                TemplateMessage.Embeds[0]
                    .AddInlineField("Aliases", string.Join(", ", group.Aliases.Select(Markdown.Code)));

            if (group.Remarks is { })
                TemplateMessage.Embeds[0].AddInlineField("Remarks", group.Remarks);

            foreach (Command command in group.Commands)
            {
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