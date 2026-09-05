#nullable enable

namespace Loupedeck.CodexActionRingPlugin.Logitech.Primary
{
    using System;
    using Loupedeck.CodexActionRingPlugin.Core;

    internal sealed class RecordingExecutor : IActionExecutor
    {
        internal DispatchResult Result { get; set; } = DispatchResult.DispatchRequested;

        internal List<RingActionId> Calls { get; } = new();

        public DispatchResult Execute(RingActionId actionId)
        {
            this.Calls.Add(actionId);
            return this.Result;
        }
    }

    internal sealed class RecordingFeedbackAdapter : IPrimaryFeedbackAdapter
    {
        internal List<(RingActionId ActionId, DispatchResult Result)> Calls { get; } = new();

        public void Present(RingActionId actionId, DispatchResult result) =>
            this.Calls.Add((actionId, result));
    }

    internal sealed class TestPlugin : Plugin, IPrimaryActionDependencyProvider
    {
        internal TestPlugin(IActionExecutor executor, IPrimaryFeedbackAdapter feedback)
        {
            this.PrimaryActionExecutor = executor;
            this.PrimaryActionFeedback = feedback;
        }

        public IActionExecutor PrimaryActionExecutor { get; }

        public IPrimaryFeedbackAdapter PrimaryActionFeedback { get; }

        public override Boolean UsesApplicationApiOnly => false;

        public override Boolean HasNoApplication => false;

        public override void Load()
        {
        }

        public override void Unload()
        {
        }
    }
}
