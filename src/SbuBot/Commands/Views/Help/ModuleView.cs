using System.Linq;
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

            TemplateMessage.Embeds[0]
                .WithTitle(module.Name)
                .WithDescription(
                    string.Format(
                        "**Description:**\n{0}\n**Submodules:**\n{1}\n**Commands:**\n{2}",
                        module.Description ?? "`--`",
                        module.Submodules.Count > 0
                            ? string.Join(
                                "\n",
                                module.Submodules.Select(
                                    m => string.Format(
                                        "{0} {1}",
                                        SbuGlobals.BULLET,
                                        m.Aliases.Count != 0 ? Markdown.Code(m.Aliases[0]) : m.Name
                                    )
                                )
                            )
                            : "`--`",
                        module.Commands.Count > 0
                            ? string.Join(
                                "\n",
                                module.Commands.Select(c => $"{SbuGlobals.BULLET} {Markdown.Code(c.GetSignature())}")
                            )
                            : "`--`"
                    )
                );

            if (module.Aliases.Count != 0)
                TemplateMessage.Embeds[0]
                    .AddInlineField("Aliases", string.Join(", ", module.Aliases.Select(Markdown.Code)));

            if (module.Remarks is { })
                TemplateMessage.Embeds[0].AddInlineField("Remarks", module.Remarks);

            foreach (Module submodule in module.Submodules)
            {
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

            foreach (Command command in module.Commands)
            {
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