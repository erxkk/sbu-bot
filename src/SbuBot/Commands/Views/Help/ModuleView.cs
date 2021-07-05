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
    public sealed class ModuleView : HelpView
    {
        private readonly Module _module;
        private readonly ImmutableDictionary<int, Module> _submodules;
        private readonly ImmutableDictionary<int, Command> _commands;

        public ModuleView(Module module)
        {
            _module = module;
            _submodules = module.Submodules.ToImmutableDictionary(k => k.GetHashCode(), v => v);
            _commands = module.Commands.ToImmutableDictionary(k => k.GetHashCode(), v => v);

            StringBuilder description = new StringBuilder("**Description:**\n", 512)
                .AppendLine(module.Description)
                .Append('\n');

            if (module.Remarks is { })
                description.AppendLine("**Remarks:**").AppendLine(module.Remarks).Append('\n');

            SelectionViewComponent moduleSelection = new(_selectModule);
            SelectionViewComponent commandSelection = new(_selectCommand);

            if (_submodules.Count != 0)
            {
                description.AppendLine("**Submodules:**");

                foreach ((int id, Module submodule) in _submodules)
                {
                    string label = submodule.Aliases.Count != 0 ? submodule.Aliases[0] : submodule.Name;
                    description.Append(SbuGlobals.BULLET).Append(' ').AppendLine(Markdown.Code(label));
                    moduleSelection.Options.Add(new(label.TrimOrSelf(25), id.ToString()));
                }

                AddComponent(moduleSelection);
            }

            if (_commands.Count != 0)
            {
                description.Append('\n').AppendLine("**Commands:**");

                foreach ((int id, Command command) in _commands)
                {
                    command.AppendTo(description.Append(SbuGlobals.BULLET).Append(' ').Append('`'));
                    description.Append('`').Append('\n');
                    commandSelection.Options.Add(new(command.Format(false).TrimOrSelf(25), id.ToString()));
                }

                AddComponent(commandSelection);
            }

            TemplateMessage.Embeds[0]
                .WithTitle(module.Name)
                .WithDescription(description.ToString());

            if (module.Aliases.Count != 0)
            {
                TemplateMessage.Embeds[0]
                    .AddInlineField("Aliases", string.Join(", ", module.Aliases.Select(Markdown.Code)));
            }
        }

        private ValueTask _selectModule(SelectionEventArgs e)
        {
            if (e.Interaction.SelectedValues.Count != 1)
                return default;

            Module submodule = _submodules[Convert.ToInt32(e.Interaction.SelectedValues[0])];
            Menu.View = submodule.IsGroup() ? new GroupView(submodule) : new ModuleView(submodule);
            return default;
        }

        private ValueTask _selectCommand(SelectionEventArgs e)
        {
            if (e.Interaction.SelectedValues.Count != 1)
                return default;

            Menu.View = new CommandView(_commands[Convert.ToInt32(e.Interaction.SelectedValues[0])]);
            return default;
        }

        public override ValueTask GoToParent(ButtonEventArgs e)
        {
            Menu.View = _module.Parent is null
                ? new RootView(_module.Service)
                : new ModuleView(_module.Parent);

            return default;
        }
    }
}