namespace Loupedeck.CodexActionRingPlugin.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using Loupedeck.CodexActionRingPlugin.Core;

    internal sealed class RecordingDesktopBridge : IDesktopBridge
    {
        internal DispatchResult Result { get; set; } = DispatchResult.DispatchRequested;

        internal Boolean ThrowOnDispatch { get; set; }

        internal List<DesktopInvocation> Invocations { get; } = new();

        public DispatchResult Dispatch(DesktopInvocation invocation)
        {
            this.Invocations.Add(invocation);

            if (this.ThrowOnDispatch)
            {
                throw new InvalidOperationException("recording bridge failure");
            }

            return this.Result;
        }
    }

    internal sealed class RecordingPreconditionEvaluator : IActionPreconditionEvaluator
    {
        internal Boolean Satisfied { get; set; } = true;

        internal Boolean ThrowOnEvaluation { get; set; }

        internal List<ActionPrecondition> Evaluations { get; } = new();

        public Boolean IsSatisfied(ActionPrecondition precondition)
        {
            this.Evaluations.Add(precondition);

            if (this.ThrowOnEvaluation)
            {
                throw new InvalidOperationException("recording precondition failure");
            }

            return this.Satisfied;
        }
    }
}
