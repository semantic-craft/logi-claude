#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.DesktopBridge.Tests
{
    using System;
    using System.Collections.Generic;
    using Loupedeck.ClaudeActionRingPlugin.Core;

    internal sealed class RecordingDesktopBridge : IDesktopBridge
    {
        internal DispatchResult Result { get; set; } = DispatchResult.DispatchRequested;

        internal List<KeyboardShortcut> Shortcuts { get; } = new();

        public DispatchResult Dispatch(KeyboardShortcut shortcut)
        {
            this.Shortcuts.Add(shortcut);
            return this.Result;
        }
    }

    internal sealed class StubFrontmostApplicationSource : IFrontmostApplicationSource
    {
        internal IReadOnlyList<FrontmostApplicationIdentity> Applications { get; set; } =
            Array.Empty<FrontmostApplicationIdentity>();

        internal Exception? Exception { get; set; }

        internal Int32 Calls { get; private set; }

        public IReadOnlyList<FrontmostApplicationIdentity> Read()
        {
            this.Calls++;

            if (this.Exception is not null)
            {
                throw this.Exception;
            }

            return this.Applications;
        }
    }

    internal sealed class RecordingShortcutDispatcher : IShortcutDispatcher
    {
        internal LocalDispatchOutcome Outcome { get; set; } = LocalDispatchOutcome.Accepted;

        internal Exception? Exception { get; set; }

        internal List<KeyboardShortcut> Shortcuts { get; } = new();

        public LocalDispatchOutcome Dispatch(KeyboardShortcut shortcut)
        {
            this.Shortcuts.Add(shortcut);

            if (this.Exception is not null)
            {
                throw this.Exception;
            }

            return this.Outcome;
        }
    }

    internal sealed class RecordingLogSink : IDesktopBridgeLogSink
    {
        internal Exception? Exception { get; set; }

        internal List<DesktopBridgeLogEntry> Entries { get; } = new();

        public void Write(DesktopBridgeLogEntry entry)
        {
            this.Entries.Add(entry);

            if (this.Exception is not null)
            {
                throw this.Exception;
            }
        }
    }
}
