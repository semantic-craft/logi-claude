namespace Loupedeck.ClaudeActionRingPlugin.Feedback.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Loupedeck.ClaudeActionRingPlugin.Core;
    using Xunit;

    public sealed class FeedbackPolicyTests
    {
        private static IEnumerable<RingActionId> ClaudeDirectedActions =>
            Enum.GetValues<RingActionId>();

        [Fact]
        public void EveryClaudeDirectedActionUsesTheSameHonestFourStatePolicy()
        {
            foreach (var actionId in ClaudeDirectedActions)
            {
                AssertPreDisabled(FeedbackPolicy.Decide(
                    actionId,
                    DispatchResult.NotDispatched,
                    FeedbackPresentationContext.PreDisabled));

                AssertRaceRejected(FeedbackPolicy.Decide(
                    actionId,
                    DispatchResult.NotDispatched,
                    FeedbackPresentationContext.Selection));

                AssertTransient(
                    FeedbackPolicy.Decide(actionId, DispatchResult.DispatchRequested, FeedbackPresentationContext.Selection),
                    FeedbackCue.DispatchRequested,
                    FeedbackProjectionKind.DispatchRequested,
                    "dispatch_requested",
                    "Sent to Claude — result unknown");

                AssertTransient(
                    FeedbackPolicy.Decide(actionId, DispatchResult.DispatchFailed, FeedbackPresentationContext.Selection),
                    FeedbackCue.DispatchFailed,
                    FeedbackProjectionKind.DispatchFailed,
                    "dispatch_failed",
                    "Couldn’t send to Claude");

                AssertTransient(
                    FeedbackPolicy.Decide(actionId, DispatchResult.OutcomeUnknown, FeedbackPresentationContext.Selection),
                    null,
                    FeedbackProjectionKind.OutcomeUnknown,
                    "outcome_unknown",
                    "No delivery confirmation");
            }
        }

        [Fact]
        public void ResultPolicyDoesNotDependOnStalePreDisabledContextAfterDispatch()
        {
            var dispatchedResults = new[]
            {
                DispatchResult.DispatchRequested,
                DispatchResult.DispatchFailed,
                DispatchResult.OutcomeUnknown,
            };

            foreach (var actionId in ClaudeDirectedActions)
            {
                foreach (var result in dispatchedResults)
                {
                    var selected = FeedbackPolicy.Decide(actionId, result, FeedbackPresentationContext.Selection);
                    var stalePreDisabled = FeedbackPolicy.Decide(actionId, result, FeedbackPresentationContext.PreDisabled);

                    Assert.Equal(selected, stalePreDisabled);
                }
            }
        }

        [Fact]
        public void InvalidInputsFailSilent()
        {
            Assert.Equal(
                FeedbackDecision.None,
                FeedbackPolicy.Decide((RingActionId)Int32.MaxValue, DispatchResult.DispatchRequested, FeedbackPresentationContext.Selection));
            Assert.Equal(
                FeedbackDecision.None,
                FeedbackPolicy.Decide(RingActionId.SideChat, (DispatchResult)Int32.MaxValue, FeedbackPresentationContext.Selection));
            Assert.Equal(
                FeedbackDecision.None,
                FeedbackPolicy.Decide(RingActionId.SideChat, DispatchResult.DispatchRequested, (FeedbackPresentationContext)Int32.MaxValue));
        }

        [Fact]
        public void NoDecisionContainsSuccessLanguageOrCheckmarks()
        {
            var forbidden = new[]
            {
                "opened",
                "switched",
                "started",
                "shown",
                "hidden",
                "recording",
                "completed",
                "approved",
                "declined",
                "success",
                "✓",
                "✔",
                "✅",
            };

            var decisions =
                from actionId in Enum.GetValues<RingActionId>()
                from result in Enum.GetValues<DispatchResult>()
                from context in Enum.GetValues<FeedbackPresentationContext>()
                select FeedbackPolicy.Decide(actionId, result, context);

            foreach (var decision in decisions)
            {
                var visibleSemantics = String.Join(
                    " ",
                    new[]
                    {
                        decision.StatusText,
                        decision.Projection?.OverlayKey,
                        decision.Cue?.ToString(),
                    }.Where(value => value is not null));

                Assert.DoesNotContain(forbidden, word => visibleSemantics.Contains(word, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public void EveryCueHasExactlyOneCaseSensitiveEventName()
        {
            Assert.Equal(3, HapticEventNames.All.Count);
            Assert.Equal(3, HapticEventNames.All.Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(HapticEventNames.DispatchRequested, HapticEventNames.For(FeedbackCue.DispatchRequested));
            Assert.Equal(HapticEventNames.DispatchFailed, HapticEventNames.For(FeedbackCue.DispatchFailed));
            Assert.Equal(HapticEventNames.SelectionRejected, HapticEventNames.For(FeedbackCue.SelectionRejected));
        }

        private static void AssertPreDisabled(FeedbackDecision decision)
        {
            Assert.Null(decision.Cue);
            Assert.Null(decision.StatusText);

            var projection = Assert.IsType<FeedbackProjection>(decision.Projection);
            Assert.Equal(FeedbackProjectionKind.Unavailable, projection.Kind);
            Assert.Equal(FeedbackProjectionLifetime.WhileKnownUnavailable, projection.Lifetime);
            Assert.Equal(38, projection.OpacityPercent);
            Assert.True(projection.PreservesGeometry);
            Assert.Null(projection.OverlayKey);
        }

        private static void AssertRaceRejected(FeedbackDecision decision)
        {
            AssertTransient(
                decision,
                FeedbackCue.SelectionRejected,
                FeedbackProjectionKind.SelectionRejected,
                "not_dispatched_race",
                "Not sent — action became unavailable");
        }

        private static void AssertTransient(
            FeedbackDecision decision,
            FeedbackCue? expectedCue,
            FeedbackProjectionKind expectedKind,
            String expectedOverlayKey,
            String expectedStatusText)
        {
            Assert.Equal(expectedCue, decision.Cue);
            Assert.Equal(expectedStatusText, decision.StatusText);

            var projection = Assert.IsType<FeedbackProjection>(decision.Projection);
            Assert.Equal(expectedKind, projection.Kind);
            Assert.Equal(FeedbackProjectionLifetime.UntilNextRingInvocation, projection.Lifetime);
            Assert.Equal(100, projection.OpacityPercent);
            Assert.True(projection.PreservesGeometry);
            Assert.Equal(expectedOverlayKey, projection.OverlayKey);
        }
    }
}
