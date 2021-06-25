using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;

using Qmmands;

using SbuBot.Extensions;

namespace SbuBot.Commands.Views.Help
{
    public sealed class CommandView : HelpView
    {
        private readonly Command _command;

        public CommandView(Command command)
        {
            _command = command;

            TemplateMessage.Embeds[0]
                .WithTitle(command.Name)
                .WithDescription(
                    string.Format(
                        "`{0}`\n**Description:**\n{1}",
                        command.GetSignature(),
                        command.Description ?? command.Module.Description ?? "`--`"
                    )
                );

            if (command.Aliases.Count != 0)
                TemplateMessage.Embeds[0]
                    .AddInlineField("Aliases", string.Join(", ", command.Aliases.Select(Markdown.Code)));

            if ((command.Remarks ?? command.Module.Remarks) is { } remarks)
                TemplateMessage.Embeds[0].AddInlineField("Remarks", remarks);

            foreach (Parameter parameter in command.Parameters)
                TemplateMessage.Embeds[0].AddInlineField(parameter.Name, parameter.Description);
        }

        public override ValueTask GoToParent(ButtonEventArgs e)
        {
            Menu.View = new ModuleView(_command.Module);
            return default;
        }
    }
}