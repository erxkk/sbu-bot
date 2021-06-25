using System.Linq;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;

using Qmmands;

namespace SbuBot.Commands.Views.Help
{
    public sealed class RootView : HelpView
    {
        public RootView(CommandService service) : base(true)
        {
            TemplateMessage.Embeds[0]
                .WithTitle("Modules")
                .WithDescription(
                    string.Join("\n", service.TopLevelModules.Select(m => $"{SbuGlobals.BULLET} {m.Name}"))
                );

            foreach (Module submodule in service.TopLevelModules)
            {
                AddComponent(
                    new ButtonViewComponent(
                        _ =>
                        {
                            Menu.View = new ModuleView(submodule);
                            return default;
                        }
                    )
                    {
                        Label = submodule.Name,
                        Style = ButtonComponentStyle.Secondary,
                    }
                );
            }
        }
    }
}