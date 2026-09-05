#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.DesktopBridge.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Loupedeck.ClaudeActionRingPlugin.Core;
    using Xunit;

    public sealed class ClaudeDesktopBridgeTests
    {
        private const String Version = "0.1.0";

        public static IEnumerable<Object[]> NonMatchingForegroundApplications()
        {
            yield return new Object[] { Array.Empty<FrontmostApplicationIdentity>() };
            yield return new Object[]
            {
                new[] { new FrontmostApplicationIdentity("Finder", "com.apple.finder") },
            };
            yield return new Object[]
            {
                new[] { new FrontmostApplicationIdentity("Claude", "com.example.other") },
            };
            yield return new Object[]
            {
                new[] { new FrontmostApplicationIdentity("Other", "com.anthropic.claudefordesktop") },
            };
            yield return new Object[]
            {
                new[] { new FrontmostApplicationIdentity("claude", "com.anthropic.claudefordesktop") },
            };
            yield return new Object[]
            {
                new[]
                {
                    new FrontmostApplicationIdentity("Claude", "com.anthropic.claudefordesktop"),
                    new FrontmostApplicationIdentity("Claude", "com.anthropic.claudefordesktop"),
                },
            };
        }

        [Theory]
        [MemberData(nameof(NonMatchingForegroundApplications))]
        internal void ShortcutFailsClosedUnlessForegroundIsAnUnambiguousExactMatch(
            IReadOnlyList<FrontmostApplicationIdentity> applications)
        {
            foreach (var definition in RingActionCatalog.Definitions)
            {
                var fixture = CreateFixture(applications);
                var shortcut = ((RingActionDelivery.Desktop)definition.Delivery).Shortcut;
                Assert.Equal(DispatchResult.NotDispatched, fixture.Bridge.Dispatch(shortcut));
                Assert.Empty(fixture.Shortcuts.Shortcuts);
                Assert.Single(fixture.Log.Entries);
            }
        }

        [Fact]
        internal void ForegroundQueryFailureSendsNothing()
        {
            var fixture = CreateFixture(ClaudeForeground());
            fixture.Frontmost.Exception = new InvalidOperationException("private detail");

            var result = fixture.Bridge.Dispatch(new KeyboardShortcut(
                DesktopModifiers.Control,
                DesktopKey.Tab));

            Assert.Equal(DispatchResult.NotDispatched, result);
            Assert.Empty(fixture.Shortcuts.Shortcuts);
            Assert.Equal("foreground_query_failed", Assert.Single(fixture.Log.Entries).ErrorCategory);
        }

        [Fact]
        internal void EveryCatalogShortcutEncodingIsSentExactlyOnceUnchanged()
        {
            var shortcuts = RingActionCatalog.Definitions
                .Select(definition => ((RingActionDelivery.Desktop)definition.Delivery).Shortcut)
                .ToArray();

            Assert.Equal(8, shortcuts.Length);

            foreach (var shortcut in shortcuts)
            {
                var fixture = CreateFixture(ClaudeForeground());

                var result = fixture.Bridge.Dispatch(shortcut);

                Assert.Equal(DispatchResult.DispatchRequested, result);
                Assert.Equal(shortcut, Assert.Single(fixture.Shortcuts.Shortcuts));
                Assert.Equal(1, fixture.Frontmost.Calls);
            }
        }

        [Theory]
        [InlineData(LocalDispatchOutcome.Accepted, DispatchResult.DispatchRequested)]
        [InlineData(LocalDispatchOutcome.Rejected, DispatchResult.DispatchFailed)]
        [InlineData(LocalDispatchOutcome.AcceptanceUnknown, DispatchResult.OutcomeUnknown)]
        internal void ShortcutMapsOnlyLocallyObservableOutcomes(
            LocalDispatchOutcome localOutcome,
            DispatchResult expected)
        {
            var fixture = CreateFixture(ClaudeForeground());
            fixture.Shortcuts.Outcome = localOutcome;

            var result = fixture.Bridge.Dispatch(new KeyboardShortcut(
                DesktopModifiers.Command | DesktopModifiers.Shift,
                DesktopKey.M));

            Assert.Equal(expected, result);
            Assert.Single(fixture.Shortcuts.Shortcuts);
        }

        [Fact]
        internal void ShortcutExceptionIsOutcomeUnknownWithoutRetry()
        {
            var fixture = CreateFixture(ClaudeForeground());
            fixture.Shortcuts.Exception = new InvalidOperationException("workspace/private/task");

            var result = fixture.Bridge.Dispatch(new KeyboardShortcut(
                DesktopModifiers.Command,
                DesktopKey.Semicolon));

            Assert.Equal(DispatchResult.OutcomeUnknown, result);
            Assert.Single(fixture.Shortcuts.Shortcuts);
            var entry = Assert.Single(fixture.Log.Entries);
            Assert.Equal(Version, entry.PluginVersion);
            Assert.Equal("dispatch_exception", entry.ErrorCategory);
            Assert.DoesNotContain("workspace", entry.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(DispatchResult.NotDispatched)]
        [InlineData(DispatchResult.DispatchRequested)]
        [InlineData(DispatchResult.DispatchFailed)]
        [InlineData(DispatchResult.OutcomeUnknown)]
        internal void RecordingAdapterPreservesAllFourCoreResults(DispatchResult expected)
        {
            var adapter = new RecordingDesktopBridge { Result = expected };
            var shortcut = new KeyboardShortcut(DesktopModifiers.Control, DesktopKey.Grave);

            var result = adapter.Dispatch(shortcut);

            Assert.Equal(expected, result);
            Assert.Equal(shortcut, Assert.Single(adapter.Shortcuts));
        }

        [Fact]
        internal void LoggingFailureDoesNotChangeDispatchResultOrCauseRetry()
        {
            var fixture = CreateFixture(ClaudeForeground());
            fixture.Shortcuts.Outcome = LocalDispatchOutcome.Rejected;
            fixture.Log.Exception = new InvalidOperationException("logging unavailable");

            var result = fixture.Bridge.Dispatch(new KeyboardShortcut(
                DesktopModifiers.Control,
                DesktopKey.Tab));

            Assert.Equal(DispatchResult.DispatchFailed, result);
            Assert.Single(fixture.Shortcuts.Shortcuts);
            Assert.Single(fixture.Log.Entries);
        }

        [Theory]
        [InlineData("")]
        [InlineData("0.1")]
        [InlineData("workspace/private/task")]
        internal void PluginVersionCannotCarryPrivateText(String pluginVersion)
        {
            Assert.Throws<ArgumentException>(() => new ClaudeDesktopBridge(
                new StubFrontmostApplicationSource(),
                new RecordingShortcutDispatcher(),
                pluginVersion,
                new RecordingLogSink()));
        }

        private static BridgeFixture CreateFixture(
            IReadOnlyList<FrontmostApplicationIdentity> applications)
        {
            var frontmost = new StubFrontmostApplicationSource { Applications = applications };
            var shortcuts = new RecordingShortcutDispatcher();
            var log = new RecordingLogSink();
            var bridge = new ClaudeDesktopBridge(frontmost, shortcuts, Version, log);
            return new BridgeFixture(bridge, frontmost, shortcuts, log);
        }

        private static IReadOnlyList<FrontmostApplicationIdentity> ClaudeForeground() =>
            new[]
            {
                new FrontmostApplicationIdentity(
                    ClaudeFrontmostPolicy.ProcessName,
                    ClaudeFrontmostPolicy.BundleIdentifier),
            };

        private sealed record BridgeFixture(
            ClaudeDesktopBridge Bridge,
            StubFrontmostApplicationSource Frontmost,
            RecordingShortcutDispatcher Shortcuts,
            RecordingLogSink Log);
    }
}
