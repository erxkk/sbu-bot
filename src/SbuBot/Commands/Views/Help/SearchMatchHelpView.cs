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
        private readonly object[] _commands;

        public SearchMatchHelpView(IEnumerable<Module> modules) : this(modules, true) { }
        public SearchMatchHelpView(IEnumerable<Command> commands) : this(commands, true) { }

        public SearchMatchHelpView(IEnumerable<object> commandsOrModules, bool _marker)
        {
            _commands = commandsOrModules.ToArray();

            StringBuilder description = new(256);
            SelectionViewComponent selection = new(_selectMatch);

            for (int i = 0; i < _commands.Length; i++)
            {
                string label = (i + 1).ToString();

                switch (_commands[i])
                {
                    case Command c:
                    {
                        c.AppendTo(
                            description.Append('`').Append(label).Append('`').Append(SbuGlobals.BULLET).Append(' ')
                        );

                        break;
                    }

                    case Module m:
                    {
                        m.AppendTo(
                            description.Append('`').Append(label).Append('`').Append(SbuGlobals.BULLET).Append(' ')
                        );

                        break;
                    }

                    default:
                        throw new($"Invalid type, expected Module or Command, got {_commands[i].GetType()}");
                }

                description.Append('\n');
                selection.Options.Add(new(label.TrimOrSelf(25), i.ToString()));
            }

            AddComponent(selection);
            TemplateMessage.Embeds[0].WithTitle("Multiple matches").WithDescription(description.ToString());
        }

        private ValueTask _selectMatch(SelectionEventArgs e)
        {
            if (e.Interaction.SelectedValues.Count != 1)
                return default;

            object commandOrModule = _commands[Convert.ToInt32(e.Interaction.SelectedValues[0])];

            Menu.View = commandOrModule is Command c
                ? c.Module.IsGroup() ? new GroupHelpView(c.Module) : new CommandHelpView(c)
                : new ModuleHelpView((commandOrModule as Module)!);

            return default;
        }
    }
}