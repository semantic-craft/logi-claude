#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.Feedback
{
    using System;
    using Loupedeck.ClaudeActionRingPlugin.Core;

    public static class FeedbackPolicy
    {
        private const Int32 UnavailableOpacityPercent = 38;

        public static FeedbackDecision Decide(
            RingActionId actionId,
            DispatchResult result,
            FeedbackPresentationContext context)
        {
            return IsSupported(actionId, result, context)
                ? DecideClaudeDirected(result, context)
                : FeedbackDecision.None;
        }

        private static FeedbackDecision DecideClaudeDirected(
            DispatchResult result,
            FeedbackPresentationContext context) =>
            result switch
            {
                DispatchResult.NotDispatched when context == FeedbackPresentationContext.PreDisabled =>
                    new FeedbackDecision(null, PersistentUnavailable(), null),
                DispatchResult.NotDispatched =>
                    new FeedbackDecision(
                        FeedbackCue.SelectionRejected,
                        Transient(FeedbackProjectionKind.SelectionRejected, "not_dispatched_race"),
                        "Not sent — action became unavailable"),
                DispatchResult.DispatchRequested =>
                    new FeedbackDecision(
                        FeedbackCue.DispatchRequested,
                        Transient(FeedbackProjectionKind.DispatchRequested, "dispatch_requested"),
                        "Sent to Claude — result unknown"),
                DispatchResult.DispatchFailed =>
                    new FeedbackDecision(
                        FeedbackCue.DispatchFailed,
                        Transient(FeedbackProjectionKind.DispatchFailed, "dispatch_failed"),
                        "Couldn’t send to Claude"),
                DispatchResult.OutcomeUnknown =>
                    new FeedbackDecision(
                        null,
                        Transient(FeedbackProjectionKind.OutcomeUnknown, "outcome_unknown"),
                        "No delivery confirmation"),
                _ => FeedbackDecision.None,
            };

        private static Boolean IsSupported(
            RingActionId actionId,
            DispatchResult result,
            FeedbackPresentationContext context) =>
            Enum.IsDefined(typeof(RingActionId), actionId)
            && Enum.IsDefined(typeof(DispatchResult), result)
            && Enum.IsDefined(typeof(FeedbackPresentationContext), context);

        private static FeedbackProjection PersistentUnavailable() =>
            new(
                FeedbackProjectionKind.Unavailable,
                FeedbackProjectionLifetime.WhileKnownUnavailable,
                UnavailableOpacityPercent,
                true,
                null);

        private static FeedbackProjection Transient(FeedbackProjectionKind kind, String overlayKey) =>
            new(
                kind,
                FeedbackProjectionLifetime.UntilNextRingInvocation,
                100,
                true,
                overlayKey);
    }
}
