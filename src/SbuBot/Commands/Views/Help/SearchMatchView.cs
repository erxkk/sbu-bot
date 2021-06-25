using System.Collections.Generic;
using System.Linq;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;

using Qmmands;

namespace SbuBot.Commands.Views.Help
{
    public sealed class SearchMatchView : HelpView
    {
        public SearchMatchView(IEnumerable<Command> commands)
        {
            int i = 0;

            TemplateMessage.Embeds[0]
                .WithTitle("Multiple matches")
                .WithDescription(
                    string.Join("\n", commands.Select(c => $"`{++i}` {SbuGlobals.BULLET} {c.GetSignature()}"))
                );

            i = 0;

            foreach (Command command in commands)
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
                        Label = (++i).ToString(),
                        Style = ButtonComponentStyle.Secondary,
                    }
                );
            }
        }
    }
}