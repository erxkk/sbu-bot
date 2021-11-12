using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Prompt;
using Disqord.Rest;

using SbuBot.Extensions;

namespace SbuBot.Commands.Views
{
    public class MultipleConfirmationView : PromptView
    {
        private readonly Dictionary<Snowflake, bool?> _authors;
        private readonly string? _description;

        public MultipleConfirmationView(HashSet<Snowflake> authorsId, string prompt, string? description = null)
            : base(
                new LocalMessage().WithEmbeds(
                    new LocalEmbed()
                        .WithTitle(prompt)
                        .WithDescription(
                            string.Format(
                                "{0}{1}{2}",
                                description,
                                description is null ? "" : "\n",
                                authorsId.Select(
                                        id => string.Format(
                                            "{0} {1} {2}",
                                            SbuGlobals.BULLET,
                                            LocalEmoji.Custom(SbuGlobals.Emote.Vote.NONE),
                                            Mention.User(id)
                                        )
                                    )
                                    .ToNewLines()
                            )
                        )
                        .WithColor(Color.Yellow)
                )
            )
        {
            _authors = authorsId.ToDictionary(k => k, _ => (bool?)null);
            _description = description;

            ConfirmButton.Label = null;
            DenyButton.Label = null;

            ConfirmButton.Emoji = LocalEmoji.Custom(SbuGlobals.Emote.Menu.CONFIRM);
            DenyButton.Emoji = LocalEmoji.Custom(SbuGlobals.Emote.Menu.STOP);
        }

        protected override async ValueTask CompleteAsync(bool result, ButtonEventArgs e)
        {
            _authors[e.AuthorId] = result;

            TemplateMessage.Embeds[0].Description = string.Format(
                "{0}{1}{2}",
                _description,
                _description is null ? "" : "\n",
                _authors.Select(
                        pair => string.Format(
                            "{0} {1} {2}",
                            SbuGlobals.BULLET,
                            LocalEmoji.Custom(
                                pair.Value switch
                                {
                                    true => SbuGlobals.Emote.Menu.CONFIRM,
                                    false => SbuGlobals.Emote.Menu.STOP,
                                    null => SbuGlobals.Emote.Vote.NONE,
                                }
                            ),
                            Mention.User(pair.Key)
                        )
                    )
                    .ToNewLines()
            );

            if (_authors.Any(pair => pair.Value == null))
                return;

            Result = _authors.All(pair => pair.Value == true);

            try
            {
                ConfirmButton.IsDisabled = true;
                ConfirmButton.Style = Result ? ConfirmButton.Style : LocalButtonComponentStyle.Secondary;

                DenyButton.IsDisabled = true;
                DenyButton.Style = Result ? LocalButtonComponentStyle.Secondary : DenyButton.Style;

                LocalMessage message = ToLocalMessage();

                await e.Interaction.Response()
                    .ModifyMessageAsync(
                        new()
                        {
                            Components = message.Components,
                            Embeds = new List<LocalEmbed>
                            {
                                message.Embeds[0].WithColor(result ? Color.Green : Color.Red),
                            },
                        }
                    );
            }
            finally
            {
                Menu.Stop();
            }
        }
    }
}
