using System;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;

namespace SbuBot.Commands.Views
{
    public sealed class ConfirmationView : ViewBase
    {
        public ConfirmationState State { get; private set; }

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

        public ConfirmationView(LocalMessage templateMessage) : base(templateMessage) { }

        private void SetConfirmation(bool confirmationState)
        {
            if (State != ConfirmationState.None)
                throw new InvalidOperationException("Cannot set state, it has already been set.");

            State = confirmationState ? ConfirmationState.Confirmed : ConfirmationState.Aborted;
            ReportChanges();
        }

        public override ValueTask UpdateAsync()
        {
            if (State == ConfirmationState.None)
                return default;

            foreach (ButtonViewComponent component in EnumerateComponents().OfType<ButtonViewComponent>())
                component.IsDisabled = true;

            Menu.Stop();

            return default;
        }

        [Button(Emoji = SbuGlobals.Emote.Menu.CONFIRM, Style = LocalButtonComponentStyle.Success)]
        public ValueTask ConfirmAsync(ButtonEventArgs e)
        {
            SetConfirmation(true);
            TemplateMessage.Embeds[0].Color = Color.Green;

            return default;
        }

        [Button(Emoji = SbuGlobals.Emote.Menu.STOP, Style = LocalButtonComponentStyle.Danger)]
        public ValueTask AbortAsync(ButtonEventArgs e)
        {
            SetConfirmation(false);
            TemplateMessage.Embeds[0].Color = Color.Red;

            return default;
        }
    }
}