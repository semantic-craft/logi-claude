#nullable enable

namespace Loupedeck.CodexActionRingPlugin.Integration.Tests
{
    using System;
    using System.Collections.Generic;
    using Loupedeck.CodexActionRingPlugin.Composition;
    using Loupedeck.CodexActionRingPlugin.Core;
    using Loupedeck.CodexActionRingPlugin.DesktopBridge;

    internal sealed class RecordingFrontmostApplicationSource : IFrontmostApplicationSource
    {
        internal IReadOnlyList<FrontmostApplicationIdentity> Applications { get; set; } =
            new[] { new FrontmostApplicationIdentity("ChatGPT", "com.openai.codex") };

        public IReadOnlyList<FrontmostApplicationIdentity> Read() => this.Applications;
    }

    internal sealed class RecordingShortcutDispatcher : IShortcutDispatcher
    {
        internal List<KeyboardShortcut> Calls { get; } = new();

        internal LocalDispatchOutcome Result { get; set; } = LocalDispatchOutcome.Accepted;

        public LocalDispatchOutcome Dispatch(KeyboardShortcut shortcut)
        {
            this.Calls.Add(shortcut);
            return this.Result;
        }
    }

    internal sealed class RecordingDeepLinkDispatcher : IDeepLinkDispatcher
    {
        internal List<Uri> Calls { get; } = new();

        internal LocalDispatchOutcome Result { get; set; } = LocalDispatchOutcome.Accepted;

        public LocalDispatchOutcome Dispatch(Uri uri)
        {
            this.Calls.Add(uri);
            return this.Result;
        }
    }

    internal sealed class RecordingLogSink : IDesktopBridgeLogSink
    {
        internal List<DesktopBridgeLogEntry> Entries { get; } = new();

        public void Write(DesktopBridgeLogEntry entry) => this.Entries.Add(entry);
    }

    internal sealed class RecordingImageInvalidator : IActionImageInvalidator
    {
        internal Boolean ThrowOnPrimary { get; set; }

        internal List<RingActionId> Primary { get; } = new();

        public void PrimaryProjectionChanged(RingActionId actionId)
        {
            this.Primary.Add(actionId);
            if (this.ThrowOnPrimary)
            {
                throw new InvalidOperationException("test image invalidation failure");
            }
        }

    }

    internal sealed class RecordingHapticEventSink : IHapticEventSink
    {
        internal Boolean ThrowOnRaise { get; set; }

        internal List<HapticEventDefinition> Registrations { get; } = new();

        internal List<String> Raised { get; } = new();

        public void Register(HapticEventDefinition definition) => this.Registrations.Add(definition);

        public void Raise(String eventName)
        {
            this.Raised.Add(eventName);
            if (this.ThrowOnRaise)
            {
                throw new InvalidOperationException("test haptic failure");
            }
        }
    }

    internal sealed class IntegrationFixture
    {
        internal IntegrationFixture()
        {
            this.Composition = new ActionRingComposition(new CompositionPorts(
                this.Frontmost,
                this.Shortcuts,
                this.DeepLinks,
                this.Log,
                this.Images,
                this.Haptics));
        }

        internal RecordingFrontmostApplicationSource Frontmost { get; } = new();

        internal RecordingShortcutDispatcher Shortcuts { get; } = new();

        internal RecordingDeepLinkDispatcher DeepLinks { get; } = new();

        internal RecordingLogSink Log { get; } = new();

        internal RecordingImageInvalidator Images { get; } = new();

        internal RecordingHapticEventSink Haptics { get; } = new();

        internal ActionRingComposition Composition { get; }
    }
}
