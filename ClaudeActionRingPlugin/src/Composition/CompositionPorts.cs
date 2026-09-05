#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.Composition
{
    using System;
    using Loupedeck.ClaudeActionRingPlugin.Core;
    using Loupedeck.ClaudeActionRingPlugin.DesktopBridge;

    internal sealed record HapticEventDefinition(
        String Name,
        String DisplayName,
        String Description);

    internal interface IHapticEventSink
    {
        void Register(HapticEventDefinition definition);

        void Raise(String eventName);
    }

    internal interface IActionImageInvalidator
    {
        void PrimaryProjectionChanged(RingActionId actionId);
    }

    internal sealed record CompositionPorts(
        IFrontmostApplicationSource FrontmostApplications,
        IShortcutDispatcher Shortcuts,
        IDesktopBridgeLogSink Log,
        IActionImageInvalidator Images,
        IHapticEventSink Haptics);
}
