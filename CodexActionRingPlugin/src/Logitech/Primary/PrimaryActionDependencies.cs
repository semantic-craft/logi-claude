#nullable enable

namespace Loupedeck.CodexActionRingPlugin.Logitech.Primary
{
    using System;
    using Loupedeck.CodexActionRingPlugin.Core;

    public interface IPrimaryFeedbackAdapter
    {
        void Present(RingActionId actionId, DispatchResult result);
    }

    public interface IPrimaryActionDependencyProvider
    {
        IActionExecutor PrimaryActionExecutor { get; }

        IPrimaryFeedbackAdapter PrimaryActionFeedback { get; }
    }

    internal readonly record struct PrimaryActionServices(
        IActionExecutor Executor,
        IPrimaryFeedbackAdapter Feedback);
}
