using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Extensions;

namespace SbuBot.Commands.Views.Help
{
    public sealed class CommandView : HelpView
    {
        private readonly Command _command;

        public CommandView(Command command)
        {
            _command = command;

            StringBuilder description = new(512);
            command.AppendTo(description.Append('`'));
            description.Append('`');

            description.AppendLine("`\n**Description:**").AppendLine(command.Description ?? command.Module.Description);

            if ((command.Remarks ?? command.Module.Remarks) is { } remarks)
                description.AppendLine("**Remarks:**").AppendLine(remarks);

            if (command.Attributes.OfType<UsageAttribute>().FirstOrDefault() is { } usage)
            {
                description.AppendLine("**Examples:**");

                foreach (string example in usage.Values)
                {
                    description.Append(SbuGlobals.BULLET)
                        .Append(' ')
                        .Append('`')
                        .Append(SbuGlobals.DEFAULT_PREFIX)
                        .Append(example)
                        .Append('`')
                        .Append('\n');
                }
            }

            TemplateMessage.Embeds[0]
                .WithTitle(command.Name)
                .WithDescription(description.ToString());

            if (command.Aliases.Count != 0)
            {
                TemplateMessage.Embeds[0]
                    .AddInlineField("Aliases", string.Join(", ", command.Aliases.Select(Markdown.Code)))
                    .AddBlankInlineField()
                    .AddBlankInlineField();
            }

            foreach (Parameter parameter in command.Parameters)
            {
                TemplateMessage.Embeds[0]
                    .AddInlineField(
                        parameter.Name,
                        string.Format(
                            "**Description:**\n{0}{1}",
                            parameter.Description,
                            (parameter.Remarks is { } ? "\n**Remarks:**\n" + parameter.Remarks : "")
                        )
                    );
            }
        }

        public override ValueTask GoToParent(ButtonEventArgs e)
        {
            Menu.View = new ModuleView(_command.Module);
            return default;
        }
    }
}