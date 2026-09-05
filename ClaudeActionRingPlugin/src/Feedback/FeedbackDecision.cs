#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.Feedback
{
    using System;

    public enum FeedbackPresentationContext
    {
        PreDisabled,
        Selection,
    }

    public enum FeedbackCue
    {
        DispatchRequested,
        DispatchFailed,
        SelectionRejected,
    }

    public enum FeedbackProjectionKind
    {
        Unavailable,
        DispatchRequested,
        DispatchFailed,
        OutcomeUnknown,
        SelectionRejected,
    }

    public enum FeedbackProjectionLifetime
    {
        WhileKnownUnavailable,
        UntilNextRingInvocation,
    }

    public sealed record FeedbackProjection(
        FeedbackProjectionKind Kind,
        FeedbackProjectionLifetime Lifetime,
        Int32 OpacityPercent,
        Boolean PreservesGeometry,
        String? OverlayKey);

    public sealed record FeedbackDecision(
        FeedbackCue? Cue,
        FeedbackProjection? Projection,
        String? StatusText)
    {
        public static FeedbackDecision None { get; } = new(null, null, null);
    }
}
