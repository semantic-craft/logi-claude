namespace Loupedeck.CodexActionRingPlugin.Core.Tests
{
    using System;
    using System.Linq;
    using Loupedeck.CodexActionRingPlugin.Core;
    using Xunit;

    public sealed class ActionExecutorTests
    {
        public static TheoryData<DispatchResult> DispatchResults => new()
        {
            DispatchResult.NotDispatched,
            DispatchResult.DispatchRequested,
            DispatchResult.DispatchFailed,
            DispatchResult.OutcomeUnknown,
        };

        [Theory]
        [MemberData(nameof(DispatchResults))]
        public void ExecuteReturnsEachNormativeBridgeResult(DispatchResult expected)
        {
            var fixture = new ExecutorFixture();
            fixture.Bridge.Result = expected;

            var actual = fixture.Executor.Execute(RingActionId.RecentlyViewed);

            Assert.Equal(expected, actual);
            Assert.Single(fixture.Bridge.Invocations);
        }

        [Fact]
        public void EveryCodexDirectedActionDispatchesExactlyOnce()
        {
            var codexActions = RingActionCatalog.Definitions
                .Where(definition => definition.Delivery is RingActionDelivery.Desktop)
                .Select(definition => definition.Id);

            foreach (var action in codexActions)
            {
                var fixture = new ExecutorFixture();

                var result = fixture.Executor.Execute(action);

                Assert.Equal(DispatchResult.DispatchRequested, result);
                Assert.Single(fixture.Bridge.Invocations);
            }
        }

        [Fact]
        public void FailedPreconditionClosesFailingActionWithoutDispatch()
        {
            var fixture = new ExecutorFixture();
            fixture.Preconditions.Satisfied = false;

            var result = fixture.Executor.Execute(RingActionId.QuickChat);

            Assert.Equal(DispatchResult.NotDispatched, result);
            Assert.Equal(new[] { ActionPrecondition.CodexFrontmost }, fixture.Preconditions.Evaluations);
            Assert.Empty(fixture.Bridge.Invocations);
        }

        [Fact]
        public void EveryShortcutFailsClosedWhenCodexIsNotFrontmost()
        {
            var shortcutActions = Enum.GetValues<RingActionId>();

            foreach (var action in shortcutActions)
            {
                var fixture = new ExecutorFixture();
                fixture.Preconditions.Satisfied = false;

                var result = fixture.Executor.Execute(action);

                Assert.Equal(DispatchResult.NotDispatched, result);
                Assert.Equal(new[] { ActionPrecondition.CodexFrontmost }, fixture.Preconditions.Evaluations);
                Assert.Empty(fixture.Bridge.Invocations);
            }
        }

        [Fact]
        public void PreconditionFailureIsFailClosed()
        {
            var fixture = new ExecutorFixture();
            fixture.Preconditions.ThrowOnEvaluation = true;

            var result = fixture.Executor.Execute(RingActionId.Dictation);

            Assert.Equal(DispatchResult.NotDispatched, result);
            Assert.Empty(fixture.Bridge.Invocations);
        }

        [Fact]
        public void NewChatUsesOnlyTheDeepLinkWithForegroundPrecondition()
        {
            var fixture = new ExecutorFixture();
            fixture.Preconditions.Satisfied = true;

            var result = fixture.Executor.Execute(RingActionId.NewChat);

            var invocation = Assert.Single(fixture.Bridge.Invocations);
            Assert.Equal(DispatchResult.DispatchRequested, result);
            Assert.Equal(DesktopInvocationKind.Uri, invocation.Kind);
            Assert.Equal("codex://threads/new", invocation.Uri!.AbsoluteUri);
            Assert.Equal(new[] { ActionPrecondition.CodexFrontmost }, fixture.Preconditions.Evaluations);
        }

        [Theory]
        [InlineData(DispatchResult.DispatchFailed)]
        [InlineData(DispatchResult.OutcomeUnknown)]
        public void FailureAndUncertaintyDoNotRetryOrFallback(DispatchResult bridgeResult)
        {
            var fixture = new ExecutorFixture();
            fixture.Bridge.Result = bridgeResult;

            var result = fixture.Executor.Execute(RingActionId.SideChat);

            Assert.Equal(bridgeResult, result);
            Assert.Single(fixture.Bridge.Invocations);
        }

        [Fact]
        public void SynchronousBridgeExceptionBecomesFailureWithoutRetry()
        {
            var fixture = new ExecutorFixture();
            fixture.Bridge.ThrowOnDispatch = true;

            var result = fixture.Executor.Execute(RingActionId.NextAttention);

            Assert.Equal(DispatchResult.DispatchFailed, result);
            Assert.Single(fixture.Bridge.Invocations);
        }

        [Fact]
        public void InvalidActionIdFailsClosed()
        {
            var fixture = new ExecutorFixture();

            var result = fixture.Executor.Execute((RingActionId)Int32.MaxValue);

            Assert.Equal(DispatchResult.NotDispatched, result);
            Assert.Empty(fixture.Bridge.Invocations);
        }

        private sealed class ExecutorFixture
        {
            internal ExecutorFixture()
            {
                this.Executor = new ActionExecutor(this.Bridge, this.Preconditions);
            }

            internal RecordingDesktopBridge Bridge { get; } = new();

            internal RecordingPreconditionEvaluator Preconditions { get; } = new();

            internal IActionExecutor Executor { get; }
        }
    }
}
