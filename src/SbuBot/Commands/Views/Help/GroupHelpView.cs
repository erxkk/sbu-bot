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
    public sealed class GroupHelpView : HelpViewBase
    {
        private readonly Module _group;
        private readonly ImmutableDictionary<int, Command> _commands;

        public GroupHelpView(DiscordGuildCommandContext context, Module group) : base(context)
        {
            _group = group;
            _commands = group.Commands.ToImmutableDictionary(k => k.GetHashCode(), v => v);
        }

        private ValueTask _selectOverload(SelectionEventArgs e)
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
            Menu.View = new ModuleHelpView(Context, _group.Parent);
            return default;
        }

        public override async ValueTask UpdateAsync()
        {
            StringBuilder description = new("**Signatures:**\n", 512);
            SelectionViewComponent selection = new(_selectOverload);

            foreach ((int id, Command command) in _commands)
            {
                command.AppendTo(description.Append(SbuGlobals.BULLET).Append(' ').Append('`'));
                description.Append('`').Append('\n');
                selection.Options.Add(new(command.Format(false).TrimOrSelf(25), id.ToString()));
            }

            description.Append('\n').AppendLine("**Description:**").AppendLine(_group.Description).Append('\n');

            if (_group.Remarks is { })
                description.AppendLine("**Remarks:**").AppendLine(_group.Remarks);

            var result = await _group.RunChecksAsync(Context);

            if (result is ChecksFailedResult failedResult)
            {
                description.Append('\n')
                    .AppendLine("**Checks:**")
                    .AppendLine(failedResult.FailedChecks.Select((c => $"• {c.Result.FailureReason}")).ToNewLines());
            }
            else
            {
                description.Append('\n').AppendLine("**You can execute these commands.**");
            }

            description.Append('\n');

            if (_group.Aliases.Count != 0)
            {
                description
                    .AppendLine("**Aliases:**")
                    .AppendLine(string.Join(", ", _group.Aliases.Select(Markdown.Code)));
            }

            TemplateMessage.Embeds[0]
                .WithTitle(_group.FullAliases[0])
                .WithDescription(description.ToString());

            AddComponent(selection);
        }
    }
}