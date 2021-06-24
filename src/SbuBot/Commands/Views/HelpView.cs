using System.Threading.Tasks;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;

using Kkommon;

using Qmmands;

namespace SbuBot.Commands.Views
{
    public abstract class HelpView : ViewBase
    {
        private readonly History<HelpView> _history;

        private HelpView(Module module, History<HelpView> history)
            : base(new LocalMessage().WithEmbeds(new LocalEmbed()))
            => _history = history;

        private HelpView(Command command, History<HelpView> history)
            : base(new LocalMessage().WithEmbeds(new LocalEmbed()))
            => _history = history;

        [Button(Emoji = "◀️", Style = ButtonComponentStyle.Primary, Row = 3)]
        public ValueTask Previous(ButtonEventArgs e)
        {
            if (_history.TryGoBack(out HelpView? view))
                Menu.View = view;

            return default;
        }

        [Button(Emoji = "⏹️", Style = ButtonComponentStyle.Danger, Row = 3)]
        public ValueTask Stop(ButtonEventArgs e)
        {
            if (Menu is InteractiveMenu menu)
                menu.Message.DeleteAsync();

            return Menu.StopAsync();
        }

        [Button(Emoji = "▶️", Style = ButtonComponentStyle.Primary, Row = 3)]
        public ValueTask Next(ButtonEventArgs e)
        {
            if (_history.TryGoForward(out HelpView? view))
                Menu.View = view;

            return default;
        }

        public static HelpView Module(Module module)
        {
            History<HelpView> history = new();
            history.Add(new ModuleView(module, history));
            return history.Current!;
        }

        public static HelpView Command(Command command)
        {
            History<HelpView> history = new();
            history.Add(new CommandView(command, history));
            return history.Current!;
        }

        // TODO: modify embed to show command structure
        private sealed class ModuleView : HelpView
        {
            private readonly Module _module;

            public ModuleView(Module module, History<HelpView> history) : base(module, history)
            {
                _module = module;

                TemplateMessage.Embeds[0] = new LocalEmbed()
                    .WithTitle("ModuleView")
                    .WithDescription($"Module Name: {module}");

                foreach (Module subModule in module.Submodules)
                {
                    AddComponent(
                        new ButtonViewComponent(
                            _ =>
                            {
                                _history.Add(new ModuleView(subModule, _history));
                                Menu.View = _history.Current;
                                return default;
                            }
                        ) { Label = subModule.Name, Row = 1, Style = ButtonComponentStyle.Secondary }
                    );
                }

                foreach (Command command in module.Commands)
                {
                    AddComponent(
                        new ButtonViewComponent(
                            _ =>
                            {
                                _history.Add(new CommandView(command, _history));
                                Menu.View = _history.Current;
                                return default;
                            }
                        ) { Label = command.Name, Row = 2, Style = ButtonComponentStyle.Secondary }
                    );
                }
            }

            [Button(Label = "Module", Style = ButtonComponentStyle.Primary)]
            public ValueTask Parent(ButtonEventArgs e)
            {
                if (_module.Parent is null)
                    return default;

                _history.Add(new ModuleView(_module.Parent, _history));
                Menu.View = _history.Current;

                return default;
            }
        }

        private sealed class CommandView : HelpView
        {
            private readonly Command _command;

            public CommandView(Command command, History<HelpView> history) : base(command, history)
            {
                _command = command;

                TemplateMessage.Embeds[0] = new LocalEmbed()
                    .WithTitle("CommandView")
                    .WithDescription($"Command Name: {command}");
            }

            [Button(Label = "Module", Style = ButtonComponentStyle.Primary)]
            public ValueTask Parent(ButtonEventArgs e)
            {
                _history.Add(new ModuleView(_command.Module, _history));
                Menu.View = _history.Current;
                return default;
            }
        }
    }
}