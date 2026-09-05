#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.DesktopBridge.Tests
{
    using System;
    using Loupedeck.ClaudeActionRingPlugin.Core;
    using Xunit;

    public sealed class ClaudeFrontmostPreconditionEvaluatorTests
    {
        [Fact]
        internal void NoneNeedsNoForegroundRead()
        {
            var source = new StubFrontmostApplicationSource
            {
                Exception = new InvalidOperationException("must not be called"),
            };
            var evaluator = new ClaudeFrontmostPreconditionEvaluator(source);

            Assert.True(evaluator.IsSatisfied(ActionPrecondition.None));
            Assert.Equal(0, source.Calls);
        }

        [Fact]
        internal void ClaudeForegroundRequiresBothExactIdentifiers()
        {
            var source = new StubFrontmostApplicationSource
            {
                Applications = new[]
                {
                    new FrontmostApplicationIdentity("Claude", "com.anthropic.claudefordesktop"),
                },
            };
            var evaluator = new ClaudeFrontmostPreconditionEvaluator(source);

            Assert.True(evaluator.IsSatisfied(ActionPrecondition.ClaudeFrontmost));
            Assert.Equal(1, source.Calls);
        }

        [Fact]
        internal void QueryFailureFailsClosed()
        {
            var source = new StubFrontmostApplicationSource
            {
                Exception = new InvalidOperationException("unavailable"),
            };
            var evaluator = new ClaudeFrontmostPreconditionEvaluator(source);

            Assert.False(evaluator.IsSatisfied(ActionPrecondition.ClaudeFrontmost));
        }

        [Fact]
        internal void UnknownPreconditionFailsClosed()
        {
            var source = new StubFrontmostApplicationSource();
            var evaluator = new ClaudeFrontmostPreconditionEvaluator(source);

            Assert.False(evaluator.IsSatisfied((ActionPrecondition)999));
            Assert.Equal(0, source.Calls);
        }
    }
}
