using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Extensions;

namespace SbuBot.Commands.Views.Help
{
    public sealed class CommandHelpView : HelpViewBase
    {
        private readonly Command _command;

        public CommandHelpView(DiscordGuildCommandContext context, Command command) : base(context)
            => _command = command;

        public override ValueTask GoToParentAsync(ButtonEventArgs e)
        {
            Menu.View = _command.Module.IsGroup()
                ? new GroupHelpView(Context, _command.Module)
                : new ModuleHelpView(Context, _command.Module);

            return default;
        }

        public override async ValueTask UpdateAsync()
        {
            StringBuilder description = new("**Signature:**\n", 512);
            _command.AppendTo(description.Append('`'));
            description.Append('`').Append('\n').Append('\n');

            description.AppendLine("**Description:**")
                .AppendLine(_command.Description ?? _command.Module.Description)
                .Append('\n');

            if ((_command.Remarks ?? _command.Module.Remarks) is { } remarks)
                description.AppendLine("**Remarks:**").AppendLine(remarks).Append('\n');

            var result = await _command.RunChecksAsync(Context);

            if (result is ChecksFailedResult failedResult)
            {
                description.Append('\n')
                    .AppendLine("**Checks:**")
                    .AppendLine(failedResult.FailedChecks.Select((c => $"• {c.Result.FailureReason}")).ToNewLines());
            }
            else
            {
                description.Append('\n').AppendLine("**You can execute this command.**");
            }

            description.Append('\n');

            if (_command.Attributes.OfType<UsageAttribute>().FirstOrDefault() is { } usage)
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

            if (_command.Aliases.Count != 0)
            {
                description
                    .AppendLine("Aliases:")
                    .AppendLine(string.Join(", ", _command.Aliases.Select(Markdown.Code)));
            }

            TemplateMessage.Embeds[0]
                .WithTitle(_command.Name)
                .WithDescription(description.ToString());

            foreach (Parameter parameter in _command.Parameters)
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
    }
}