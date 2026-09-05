#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.Core
{
    using System;

    [Flags]
    internal enum DesktopModifiers
    {
        None = 0,
        Command = 1,
        Control = 2,
        Option = 4,
        Shift = 8,
    }

    internal enum DesktopKey
    {
        M,
        I,
        E,
        Semicolon,
        D,
        Grave,
        O,
        Tab,
    }

    internal readonly record struct KeyboardShortcut(DesktopModifiers Modifiers, DesktopKey Key);

    internal enum ActionPrecondition
    {
        None,
        ClaudeFrontmost,
    }

    internal abstract record RingActionDelivery
    {
        private RingActionDelivery()
        {
        }

        internal sealed record Desktop(KeyboardShortcut Shortcut) : RingActionDelivery;
    }

    internal interface IDesktopBridge
    {
        DispatchResult Dispatch(KeyboardShortcut shortcut);
    }

    internal interface IActionPreconditionEvaluator
    {
        Boolean IsSatisfied(ActionPrecondition precondition);
    }
}
