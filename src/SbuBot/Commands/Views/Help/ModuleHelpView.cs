using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;

using Qmmands;

using SbuBot.Extensions;

namespace SbuBot.Commands.Views.Help
{
    public sealed class ModuleHelpView : HelpViewBase
    {
        private readonly Module _module;
        private readonly ImmutableDictionary<int, Module> _submodules;
        private readonly ImmutableDictionary<int, Command> _commands;

        public ModuleHelpView(DiscordGuildCommandContext context, Module module) : base(context)
        {
            _module = module;
            _submodules = module.Submodules.ToImmutableDictionary(k => k.GetHashCode(), v => v);
            _commands = module.Commands.ToImmutableDictionary(k => k.GetHashCode(), v => v);
        }

        private ValueTask _selectModule(SelectionEventArgs e)
        {
            if (e.Interaction.SelectedValues.Count != 1)
                return default;

            Module submodule = _submodules[Convert.ToInt32(e.Interaction.SelectedValues[0])];

            Menu.View = submodule.IsGroup()
                ? new GroupHelpView(Context, submodule)
                : new ModuleHelpView(Context, submodule);

            return default;
        }

        private ValueTask _selectCommand(SelectionEventArgs e)
        {
            if (e.Interaction.SelectedValues.Count != 1)
                return default;

            Menu.View = new CommandHelpView(
                Context,
                _commands[Convert.ToInt32(e.Interaction.SelectedValues[0])]
            );

            return default;
        }

        public override ValueTask GoToParentAsync(ButtonEventArgs e)
        {
            Menu.View = _module.Parent is null
                ? new RootHelpView(Context, _module.Service)
                : new ModuleHelpView(Context, _module.Parent);

            return default;
        }

        public override async ValueTask UpdateAsync()
        {
            StringBuilder description = new StringBuilder("**Description:**\n", 512)
                .AppendLine(_module.Description);

            if (_module.Remarks is { })
                description.AppendLine("**Remarks:**").AppendLine(_module.Remarks);

            var result = await _module.RunChecksAsync(Context);

            if (result is ChecksFailedResult failedResult)
            {
                description.Append('\n')
                    .AppendLine("**Checks:**")
                    .AppendLine(
                        failedResult.FailedChecks
                            .Select((c => $"{SbuGlobals.BULLET} {c.Result.FailureReason}"))
                            .ToNewLines()
                    );
            }
            else
            {
                description.Append('\n')
                    .Append(LocalEmoji.Custom(SbuGlobals.Emote.Menu.STOP))
                    .AppendLine("**You can execute commands in this module.**");
            }

            SelectionViewComponent moduleSelection = new(_selectModule);
            SelectionViewComponent commandSelection = new(_selectCommand);

            if (_submodules.Count != 0)
            {
                description.Append('\n').AppendLine("**Submodules:**");

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

            if (_module.Aliases.Count != 0)
            {
                description
                    .AppendLine("**Aliases:**")
                    .AppendLine(string.Join(", ", _module.Aliases.Select(Markdown.Code)));
            }

            TemplateMessage.Embeds[0]
                .WithTitle(_module.Name)
                .WithDescription(description.ToString());
        }
    }
}