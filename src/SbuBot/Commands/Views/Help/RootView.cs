using System.Text;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;

using Qmmands;

namespace SbuBot.Commands.Views.Help
{
    public sealed class RootView : HelpView
    {
        public RootView(CommandService service) : base(true)
        {
            StringBuilder description = new(512);

            foreach (Module submodule in service.TopLevelModules)
            {
                description.Append(SbuGlobals.BULLET).Append(' ').AppendLine(submodule.Name);

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

            TemplateMessage.Embeds[0]
                .WithTitle("Modules")
                .WithDescription(description.ToString());
        }
    }
}