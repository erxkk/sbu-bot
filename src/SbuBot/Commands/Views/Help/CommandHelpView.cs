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
    public sealed class CommandHelpView : HelpViewBase
    {
        private readonly Command _command;

        public CommandHelpView(Command command)
        {
            _command = command;

            StringBuilder description = new("**Signature:**\n", 512);
            command.AppendTo(description.Append('`'));
            description.Append('`').Append('\n').Append('\n');

            description.AppendLine("**Description:**")
                .AppendLine(command.Description ?? command.Module.Description)
                .Append('\n');

            if ((command.Remarks ?? command.Module.Remarks) is { } remarks)
                description.AppendLine("**Remarks:**").AppendLine(remarks).Append('\n');

            if (command.Attributes.OfType<UsageAttribute>().FirstOrDefault() is { } usage)
            {
                description.AppendLine("**Examples:**");

                foreach (string example in usage.Values)
                {
                    description.Append(SbuGlobals.BULLET)
                        .Append(' ')
                        .Append('`')
                        .Append(SbuGlobals.DEFAULT_PREFIX)
                        .Append(' ')
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
                        parameter.Format(),
                        string.Format(
                            "**Description:**\n{0}{1}",
                            parameter.Description,
                            (parameter.Remarks is { } ? "\n\n**Remarks:**\n" + parameter.Remarks : "")
                        )
                    );
            }
        }

        public override ValueTask GoToParent(ButtonEventArgs e)
        {
            Menu.View = _command.Module.IsGroup()
                ? new GroupHelpView(_command.Module)
                : new ModuleHelpView(_command.Module);

            return default;
        }
    }
}