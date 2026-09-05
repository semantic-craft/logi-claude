#nullable enable

namespace Loupedeck.CodexActionRingPlugin.DesktopBridge.Tests
{
    using System;
    using Loupedeck.CodexActionRingPlugin.Core;
    using Xunit;

    public sealed class CodexFrontmostPreconditionEvaluatorTests
    {
        [Fact]
        internal void NoneNeedsNoForegroundRead()
        {
            var source = new StubFrontmostApplicationSource
            {
                Exception = new InvalidOperationException("must not be called"),
            };
            var evaluator = new CodexFrontmostPreconditionEvaluator(source);

            Assert.True(evaluator.IsSatisfied(ActionPrecondition.None));
            Assert.Equal(0, source.Calls);
        }

        [Fact]
        internal void CodexForegroundRequiresBothExactIdentifiers()
        {
            var source = new StubFrontmostApplicationSource
            {
                Applications = new[]
                {
                    new FrontmostApplicationIdentity("ChatGPT", "com.openai.codex"),
                },
            };
            var evaluator = new CodexFrontmostPreconditionEvaluator(source);

            Assert.True(evaluator.IsSatisfied(ActionPrecondition.CodexFrontmost));
            Assert.Equal(1, source.Calls);
        }

        [Fact]
        internal void QueryFailureFailsClosed()
        {
            var source = new StubFrontmostApplicationSource
            {
                Exception = new InvalidOperationException("unavailable"),
            };
            var evaluator = new CodexFrontmostPreconditionEvaluator(source);

            Assert.False(evaluator.IsSatisfied(ActionPrecondition.CodexFrontmost));
        }

        [Fact]
        internal void UnknownPreconditionFailsClosed()
        {
            var source = new StubFrontmostApplicationSource();
            var evaluator = new CodexFrontmostPreconditionEvaluator(source);

            Assert.False(evaluator.IsSatisfied((ActionPrecondition)999));
            Assert.Equal(0, source.Calls);
        }
    }
}
