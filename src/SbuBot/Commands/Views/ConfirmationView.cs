using System.Collections.Generic;
using System.Threading.Tasks;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Prompt;
using Disqord.Rest;

namespace SbuBot.Commands.Views
{
    public sealed class ConfirmationView : PromptView
    {
        public ConfirmationView(string prompt, string? description = null) : this(
            new LocalMessage().WithEmbeds(
                new LocalEmbed()
                    .WithTitle(prompt)
                    .WithDescription(description)
                    .WithColor(Color.Yellow)
            )
        ) { }

        public ConfirmationView() : this(
            new LocalMessage().WithEmbeds(
                new LocalEmbed()
                    .WithTitle("Confirmation Required")
                    .WithDescription("Are you sure you want to proceed?")
                    .WithColor(Color.Yellow)
            )
        ) { }

        public ConfirmationView(LocalMessage templateMessage) : base(templateMessage)
        {
            ConfirmButton.Label = null;
            DenyButton.Label = null;

            ConfirmButton.Emoji = LocalEmoji.Custom(SbuGlobals.Emote.Menu.CONFIRM);
            DenyButton.Emoji = LocalEmoji.Custom(SbuGlobals.Emote.Menu.STOP);
        }

        protected override async ValueTask CompleteAsync(bool result, ButtonEventArgs e)
        {
            Result = result;

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