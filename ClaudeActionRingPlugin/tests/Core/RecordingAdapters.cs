namespace Loupedeck.ClaudeActionRingPlugin.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using Loupedeck.ClaudeActionRingPlugin.Core;

    internal sealed class RecordingDesktopBridge : IDesktopBridge
    {
        internal DispatchResult Result { get; set; } = DispatchResult.DispatchRequested;

        internal Boolean ThrowOnDispatch { get; set; }

        internal List<KeyboardShortcut> Shortcuts { get; } = new();

        public DispatchResult Dispatch(KeyboardShortcut shortcut)
        {
            this.Shortcuts.Add(shortcut);

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
