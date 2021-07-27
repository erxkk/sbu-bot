using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;

using Qmmands;

using SbuBot.Extensions;

namespace SbuBot.Commands.Views.Help
{
    public sealed class SearchMatchHelpView : HelpViewBase
    {
        private readonly Command[] _commands;

        public SearchMatchHelpView(IEnumerable<Command> commands)
        {
            _commands = commands.ToArray();

            StringBuilder description = new(256);
            SelectionViewComponent selection = new(_selectMatch);

            for (int i = 0; i < _commands.Length; i++)
            {
                string label = (i + 1).ToString();

                _commands[i]
                    .AppendTo(
                        description.Append('`').Append(label).Append('`').Append(SbuGlobals.BULLET).Append(' ')
                    );

                description.Append('\n');
                selection.Options.Add(new(label.TrimOrSelf(25), label));
            }

            AddComponent(selection);
            TemplateMessage.Embeds[0].WithTitle("Multiple matches").WithDescription(description.ToString());
        }

        private ValueTask _selectMatch(SelectionEventArgs e)
        {
            if (e.Interaction.SelectedValues.Count != 1)
                return default;

            Command command = _commands[Convert.ToInt32(e.Interaction.SelectedValues[0])];
            Menu.View = command.Module.IsGroup() ? new GroupHelpView(command.Module) : new CommandHelpView(command);

            return default;
        }
    }
}