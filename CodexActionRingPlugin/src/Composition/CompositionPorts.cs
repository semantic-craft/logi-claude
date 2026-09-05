#nullable enable

namespace Loupedeck.CodexActionRingPlugin.Composition
{
    using System;
    using Loupedeck.CodexActionRingPlugin.Core;
    using Loupedeck.CodexActionRingPlugin.DesktopBridge;

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
        IDeepLinkDispatcher DeepLinks,
        IDesktopBridgeLogSink Log,
        IActionImageInvalidator Images,
        IHapticEventSink Haptics);
}
