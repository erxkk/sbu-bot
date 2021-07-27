using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Extensions.Interactivity.Menus;

using SbuBot.Evaluation.Inspection.Inspector;

namespace SbuBot.Commands.Views.Inspection
{
    // TODO: lazy or cached page provider or something
    // TODO: unfinished, pagination for long inspections, main navigation etc
    public class InspectionView : CustomPagedView
    {
        private object? _parent;
        private object[]? _children;

        private object? Parent
        {
            get => _parent;
            set
            {
                _parent = value;

                if (value is null)
                    GotToParentComponent.IsDisabled = true;

                ReportChanges();
            }
        }

        private object[]? Children
        {
            get => _children;
            set
            {
                _children = value;

                if (value is null)
                    GotToParentChild.IsDisabled = true;

                ReportChanges();
            }
        }

        public IInspector Inspector { get; }

        public SelectionViewComponent GotToParentChild { get; }
        public ButtonViewComponent GotToParentComponent { get; }

        public InspectionView(IInspector inspector) : base(null, new LocalMessage().WithEmbeds(new LocalEmbed()))
        {
            GotToParentComponent = new(GoToParent)
            {
                // TODO: emote revamp, add down and up emotes
                Emoji = LocalEmoji.Custom(SbuGlobals.Guild.Emote.Menu.BACK),
                Style = LocalButtonComponentStyle.Secondary,
                Row = 4,
                Position = 0,
                IsDisabled = inspector is not IChildInspector,
            };

            AddComponent(GotToParentComponent);

            // TODO: need selection options here already (max 25)
            GotToParentChild = new(GoToChild)
            {
                Row = 4,
                Position = 1,
                IsDisabled = inspector is not IParentInspector,
            };

            AddComponent(GotToParentChild);
        }

        public ValueTask GoToParent(ButtonEventArgs e)
        {
            if (Inspector is IChildInspector childInspector)
                Menu.View = new InspectionView(childInspector.Parent);

            return default;
        }

        public ValueTask GoToChild(SelectionEventArgs e)
        {
            if (e.Interaction.SelectedValues.Count != 1)
                return default;

            if (Inspector is not IParentInspector parentInspector)
                throw new InvalidOperationException("Selection on non IParentInspector.");

            var selection = Convert.ToInt32(e.Interaction.SelectedValues[0]);
            Menu.View = new InspectionView(parentInspector);

            return default;
        }
    }
}