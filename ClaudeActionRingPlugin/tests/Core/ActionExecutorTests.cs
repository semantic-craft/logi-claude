namespace Loupedeck.ClaudeActionRingPlugin.Core.Tests
{
    using System;
    using System.Linq;
    using Loupedeck.ClaudeActionRingPlugin.Core;
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

            var actual = fixture.Executor.Execute(RingActionId.NextSession);

            Assert.Equal(expected, actual);
            Assert.Single(fixture.Bridge.Shortcuts);
        }

        [Fact]
        public void EveryClaudeDirectedActionDispatchesExactlyOnce()
        {
            var claudeActions = RingActionCatalog.Definitions
                .Where(definition => definition.Delivery is RingActionDelivery.Desktop)
                .Select(definition => definition.Id);

            foreach (var action in claudeActions)
            {
                var fixture = new ExecutorFixture();

                var result = fixture.Executor.Execute(action);

                Assert.Equal(DispatchResult.DispatchRequested, result);
                Assert.Single(fixture.Bridge.Shortcuts);
            }
        }

        [Fact]
        public void FailedPreconditionClosesFailingActionWithoutDispatch()
        {
            var fixture = new ExecutorFixture();
            fixture.Preconditions.Satisfied = false;

            var result = fixture.Executor.Execute(RingActionId.ModelMenu);

            Assert.Equal(DispatchResult.NotDispatched, result);
            Assert.Equal(new[] { ActionPrecondition.ClaudeFrontmost }, fixture.Preconditions.Evaluations);
            Assert.Empty(fixture.Bridge.Shortcuts);
        }

        [Fact]
        public void EveryShortcutFailsClosedWhenClaudeIsNotFrontmost()
        {
            var shortcutActions = Enum.GetValues<RingActionId>();

            foreach (var action in shortcutActions)
            {
                var fixture = new ExecutorFixture();
                fixture.Preconditions.Satisfied = false;

                var result = fixture.Executor.Execute(action);

                Assert.Equal(DispatchResult.NotDispatched, result);
                Assert.Equal(new[] { ActionPrecondition.ClaudeFrontmost }, fixture.Preconditions.Evaluations);
                Assert.Empty(fixture.Bridge.Shortcuts);
            }
        }

        [Fact]
        public void PreconditionFailureIsFailClosed()
        {
            var fixture = new ExecutorFixture();
            fixture.Preconditions.ThrowOnEvaluation = true;

            var result = fixture.Executor.Execute(RingActionId.EffortMenu);

            Assert.Equal(DispatchResult.NotDispatched, result);
            Assert.Empty(fixture.Bridge.Shortcuts);
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
            Assert.Single(fixture.Bridge.Shortcuts);
        }

        [Fact]
        public void SynchronousBridgeExceptionBecomesFailureWithoutRetry()
        {
            var fixture = new ExecutorFixture();
            fixture.Bridge.ThrowOnDispatch = true;

            var result = fixture.Executor.Execute(RingActionId.PermissionMode);

            Assert.Equal(DispatchResult.DispatchFailed, result);
            Assert.Single(fixture.Bridge.Shortcuts);
        }

        [Fact]
        public void InvalidActionIdFailsClosed()
        {
            var fixture = new ExecutorFixture();

            var result = fixture.Executor.Execute((RingActionId)Int32.MaxValue);

            Assert.Equal(DispatchResult.NotDispatched, result);
            Assert.Empty(fixture.Bridge.Shortcuts);
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
