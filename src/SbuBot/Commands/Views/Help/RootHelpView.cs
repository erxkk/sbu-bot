using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;

using Qmmands;

using SbuBot.Extensions;

namespace SbuBot.Commands.Views.Help
{
    public sealed class RootHelpView : HelpViewBase
    {
        private readonly ImmutableDictionary<int, Module> _modules;

        public RootHelpView(DiscordGuildCommandContext context, CommandService service) : base(context, true)
        {
            _modules = service.TopLevelModules.ToImmutableDictionary(k => k.GetHashCode(), v => v);

            StringBuilder description = new(512);
            SelectionViewComponent selection = new(_selectModule);

            foreach ((int id, Module module) in _modules)
            {
                description.Append(SbuGlobals.BULLET).Append(' ').AppendLine(module.Name);
                selection.Options.Add(new(module.Name.TrimOrSelf(25), id.ToString()));
            }

            AddComponent(selection);

            TemplateMessage.Embeds[0]
                .WithTitle("Modules")
                .WithDescription(description.ToString());
        }

        private ValueTask _selectModule(SelectionEventArgs e)
        {
            if (e.Interaction.SelectedValues.Count != 1)
                return default;

            Menu.View = new ModuleHelpView(
                Context,
                _modules[Convert.ToInt32(e.Interaction.SelectedValues[0])]
            );

            return default;
        }
    }
}