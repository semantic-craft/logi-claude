#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.DesktopBridge
{
    using System;
    using System.Collections.Generic;
    using Loupedeck.ClaudeActionRingPlugin.Core;

    internal enum LocalDispatchOutcome
    {
        Accepted,
        Rejected,
        AcceptanceUnknown,
    }

    internal readonly record struct FrontmostApplicationIdentity(
        String? ProcessName,
        String? BundleIdentifier);

    internal interface IFrontmostApplicationSource
    {
        IReadOnlyList<FrontmostApplicationIdentity> Read();
    }

    internal interface IShortcutDispatcher
    {
        LocalDispatchOutcome Dispatch(KeyboardShortcut shortcut);
    }

    internal readonly record struct DesktopBridgeLogEntry(
        String PluginVersion,
        String ErrorCategory);

    internal interface IDesktopBridgeLogSink
    {
        void Write(DesktopBridgeLogEntry entry);
    }

    internal sealed class NullDesktopBridgeLogSink : IDesktopBridgeLogSink
    {
        internal static NullDesktopBridgeLogSink Instance { get; } = new();

        private NullDesktopBridgeLogSink()
        {
        }

        public void Write(DesktopBridgeLogEntry entry)
        {
        }
    }
}
