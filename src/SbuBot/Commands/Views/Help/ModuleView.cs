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

        public ModuleView(Module module)
        {
            _module = module;

            StringBuilder description = new StringBuilder("**Description:**\n", 512)
                .AppendLine(module.Description);

            if (module.Remarks is { })
                description.AppendLine("**Remarks:**").AppendLine(module.Remarks);

            if (module.Submodules.Count > 0)
                description.AppendLine("**Submodules:**");

            foreach (Module submodule in module.Submodules)
            {
                description.Append(SbuGlobals.BULLET)
                    .Append(' ')
                    .AppendLine(
                        submodule.Aliases.Count != 0 ? Markdown.Code(submodule.Aliases[0]) : submodule.Name
                    );

                AddComponent(
                    new ButtonViewComponent(
                        _ =>
                        {
                            Menu.View = submodule.IsGroup() ? new GroupView(submodule) : new ModuleView(submodule);
                            return default;
                        }
                    )
                    {
                        Label = submodule.Aliases.Count != 0 ? submodule.Aliases[0] : submodule.Name,
                        Style = ButtonComponentStyle.Secondary,
                    }
                );
            }

            if (module.Commands.Count > 0)
                description.AppendLine("**Commands:**");

            foreach (Command command in module.Commands)
            {
                description.Append(SbuGlobals.BULLET)
                    .Append(' ')
                    .AppendLine(Markdown.Code(command.GetSignature()));

                AddComponent(
                    new ButtonViewComponent(
                        _ =>
                        {
                            Menu.View = new CommandView(command);
                            return default;
                        }
                    )
                    {
                        Label = command.Name,
                        Style = ButtonComponentStyle.Secondary,
                    }
                );
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

        public override ValueTask GoToParent(ButtonEventArgs e)
        {
            Menu.View = _module.Parent is null
                ? new RootView(_module.Service)
                : new ModuleView(_module.Parent);

            return default;
        }
    }
}