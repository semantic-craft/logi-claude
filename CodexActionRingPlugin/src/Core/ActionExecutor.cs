namespace Loupedeck.CodexActionRingPlugin.Core
{
    using System;

    public interface IActionExecutor
    {
        DispatchResult Execute(RingActionId actionId);
    }

    internal sealed class ActionExecutor : IActionExecutor
    {
        private readonly IDesktopBridge _desktopBridge;
        private readonly IActionPreconditionEvaluator _preconditions;

        internal ActionExecutor(
            IDesktopBridge desktopBridge,
            IActionPreconditionEvaluator preconditions)
        {
            this._desktopBridge = desktopBridge ?? throw new ArgumentNullException(nameof(desktopBridge));
            this._preconditions = preconditions ?? throw new ArgumentNullException(nameof(preconditions));
        }

        public DispatchResult Execute(RingActionId actionId)
        {
            if (!RingActionCatalog.TryGetDefinition(actionId, out var definition))
            {
                return DispatchResult.NotDispatched;
            }

            if (!this.PreconditionSatisfied(definition.Precondition))
            {
                return DispatchResult.NotDispatched;
            }

            var desktop = (RingActionDelivery.Desktop)definition.Delivery;

            try
            {
                var result = this._desktopBridge.Dispatch(desktop.Invocation);
                return IsKnownResult(result) ? result : DispatchResult.OutcomeUnknown;
            }
            catch (Exception)
            {
                return DispatchResult.DispatchFailed;
            }
        }

        private Boolean PreconditionSatisfied(ActionPrecondition precondition)
        {
            if (precondition == ActionPrecondition.None)
            {
                return true;
            }

            try
            {
                return this._preconditions.IsSatisfied(precondition);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static Boolean IsKnownResult(DispatchResult result) => result is
            DispatchResult.NotDispatched or
            DispatchResult.DispatchRequested or
            DispatchResult.DispatchFailed or
            DispatchResult.OutcomeUnknown;
    }
}
