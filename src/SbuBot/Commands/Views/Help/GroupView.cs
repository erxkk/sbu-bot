using System;
using System.Collections.Immutable;
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
        private readonly Module _parent;
        private readonly ImmutableDictionary<int, Command> _commands;

        public GroupView(Module group)
        {
            _parent = group.Parent;
            _commands = group.Commands.ToImmutableDictionary(k => k.GetHashCode(), v => v);

            StringBuilder description = new("**Signatures:**\n", 512);
            SelectionViewComponent selection = new(_selectOverload);

            foreach ((int id, Command command) in _commands)
            {
                command.AppendTo(description.Append(SbuGlobals.BULLET).Append(' ').Append('`'));
                description.Append('`').Append('\n');
                selection.Options.Add(new(command.Format(false).TrimOrSelf(25), id.ToString()));
            }

            description.Append('\n').AppendLine("**Description:**").AppendLine(group.Description).Append('\n');

            if (group.Remarks is { })
                description.AppendLine("**Remarks:**").AppendLine(group.Remarks);

            AddComponent(selection);

            TemplateMessage.Embeds[0]
                .WithTitle(group.FullAliases[0])
                .WithDescription(description.ToString());

            if (group.Aliases.Count != 0)
            {
                TemplateMessage.Embeds[0]
                    .AddInlineField("Aliases", string.Join(", ", group.Aliases.Select(Markdown.Code)));
            }
        }

        private ValueTask _selectOverload(SelectionEventArgs e)
        {
            if (e.Interaction.SelectedValues.Count != 1)
                return default;

            Menu.View = new CommandView(_commands[Convert.ToInt32(e.Interaction.SelectedValues[0])]);
            return default;
        }

        public override ValueTask GoToParent(ButtonEventArgs e)
        {
            Menu.View = new ModuleView(_parent);
            return default;
        }
    }
}