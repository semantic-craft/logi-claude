#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.Composition
{
    using System;
    using Loupedeck.ClaudeActionRingPlugin.Core;
    using Loupedeck.ClaudeActionRingPlugin.Feedback;
    using Loupedeck.ClaudeActionRingPlugin.Logitech.Primary;

    internal sealed class FeedbackCoordinator : IPrimaryFeedbackAdapter
    {
        private readonly IActionImageInvalidator _images;
        private readonly HapticFeedbackAdapter _haptics;

        internal FeedbackCoordinator(
            IActionImageInvalidator images,
            HapticFeedbackAdapter haptics)
        {
            this._images = images ?? throw new ArgumentNullException(nameof(images));
            this._haptics = haptics ?? throw new ArgumentNullException(nameof(haptics));
        }

        public void Present(RingActionId actionId, DispatchResult result)
        {
            var decision = FeedbackPolicy.Decide(
                actionId,
                result,
                FeedbackPresentationContext.Selection);

            try
            {
                this._images.PrimaryProjectionChanged(actionId);
            }
            catch (Exception)
            {
                // Image feedback cannot change or retry the completed dispatch.
            }

            if (decision.Cue is not null)
            {
                try
                {
                    this._haptics.Raise(decision.Cue.Value);
                }
                catch (Exception)
                {
                    // Haptic feedback cannot change or retry the completed dispatch.
                }
            }
        }

    }
}
