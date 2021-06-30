using System.Collections.Generic;
using System.Text;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;

using Kkommon.Extensions.Enumerable;

using Qmmands;

namespace SbuBot.Commands.Views.Help
{
    public sealed class SearchMatchView : HelpView
    {
        public SearchMatchView(IEnumerable<Command> commands)
        {
            StringBuilder description = new(256);

            foreach ((int index, Command command) in commands.Enumerate())
            {
                description.Append('`')
                    .Append(index)
                    .Append('`')
                    .Append(SbuGlobals.BULLET)
                    .Append(' ')
                    .AppendLine(command.GetSignature());

                AddComponent(
                    new ButtonViewComponent(
                        _ =>
                        {
                            Menu.View = new CommandView(command);
                            return default;
                        }
                    )
                    {
                        Label = index.ToString(),
                        Style = ButtonComponentStyle.Secondary,
                    }
                );
            }

            TemplateMessage.Embeds[0].WithTitle("Multiple matches").WithDescription(description.ToString());
        }
    }
}