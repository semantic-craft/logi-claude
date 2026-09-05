#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.Integration.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Loupedeck.ClaudeActionRingPlugin.Composition;
    using Loupedeck.ClaudeActionRingPlugin.Core;
    using Loupedeck.ClaudeActionRingPlugin.DesktopBridge;
    using Loupedeck.ClaudeActionRingPlugin.Feedback;
    using Loupedeck.ClaudeActionRingPlugin.Logitech.Primary;
    using Xunit;

    public sealed class CompositionIntegrationTests
    {
        [Fact]
        public void CompositionExposesThePrimaryExecutorAndFeedback()
        {
            var fixture = new IntegrationFixture();

            Assert.NotNull(fixture.Composition.PrimaryActionExecutor);
            Assert.NotNull(fixture.Composition.PrimaryActionFeedback);
        }

        [Fact]
        public void CompleteEightActionCatalogFlowsThroughTheSingleExecutorWithoutFallback()
        {
            var fixture = new IntegrationFixture();
            var results = RingActionCatalog.Definitions.ToDictionary(
                definition => definition.Id,
                definition => fixture.Composition.PrimaryActionExecutor.Execute(definition.Id));

            Assert.Equal(8, results.Count);
            Assert.All(results.Values, result => Assert.Equal(DispatchResult.DispatchRequested, result));
            Assert.Equal(8, fixture.Shortcuts.Calls.Count);
        }

        [Fact]
        public void AllShortcutEncodingIsSentExactlyOnceUnchanged()
        {
            var sent = new List<(VirtualKeyCode Key, ModifierKey Modifiers)>();
            var adapter = new LogitechShortcutDispatcher((key, modifiers) =>
                sent.Add((key, modifiers)));

            foreach (var definition in RingActionCatalog.Definitions)
            {
                var desktop = Assert.IsType<RingActionDelivery.Desktop>(definition.Delivery);
                Assert.Equal(LocalDispatchOutcome.Accepted, adapter.Dispatch(desktop.Shortcut));
            }

            Assert.Equal(
                new[]
                {
                    (VirtualKeyCode.KeyM, ModifierKey.Command | ModifierKey.Shift),
                    (VirtualKeyCode.KeyI, ModifierKey.Command | ModifierKey.Shift),
                    (VirtualKeyCode.KeyE, ModifierKey.Command | ModifierKey.Shift),
                    (VirtualKeyCode.Oem1, ModifierKey.Command),
                    (VirtualKeyCode.KeyD, ModifierKey.Command | ModifierKey.Shift),
                    (VirtualKeyCode.Oem3, ModifierKey.Ctrl),
                    (VirtualKeyCode.KeyO, ModifierKey.Ctrl),
                    (VirtualKeyCode.Tab, ModifierKey.Ctrl),
                },
                sent);
        }

        [Fact]
        public void HostDiscoverySurfaceContainsOnlyEightPrimaryCommands()
        {
            var plugin = new ClaudeActionRingPlugin();
            var provider = Assert.IsAssignableFrom<IPrimaryActionDependencyProvider>(plugin);
            Assert.NotNull(provider.PrimaryActionExecutor);

            var assembly = typeof(ClaudeActionRingPlugin).Assembly;
            Assert.Equal(
                8,
                assembly.GetTypes().Count(type =>
                    type.IsPublic
                    && type.IsSealed
                    && typeof(PrimaryActionCommand).IsAssignableFrom(type)));
            Assert.DoesNotContain(
                assembly.GetTypes(),
                type => !type.IsAbstract && typeof(PluginDynamicFolder).IsAssignableFrom(type));

            var application = new ClaudeActionRingApplication();
            Assert.Equal("Claude", InvokeProtectedString(application, "GetProcessName"));
            Assert.Equal("com.anthropic.claudefordesktop", InvokeProtectedString(application, "GetBundleName"));
        }

        [Fact]
        public void HapticRegistrationRaisingAndSourceAssetsUseTheSameExactNames()
        {
            var fixture = new IntegrationFixture();
            fixture.Composition.Load();

            Assert.Equal(
                HapticEventNames.All.Order(),
                fixture.Haptics.Registrations.Select(definition => definition.Name).Order());

            var feedback = fixture.Composition.PrimaryActionFeedback;
            feedback.Present(RingActionId.NextSession, DispatchResult.NotDispatched);
            feedback.Present(RingActionId.NextSession, DispatchResult.DispatchRequested);
            feedback.Present(RingActionId.NextSession, DispatchResult.DispatchFailed);

            Assert.Equal(
                new[]
                {
                    HapticEventNames.SelectionRejected,
                    HapticEventNames.DispatchRequested,
                    HapticEventNames.DispatchFailed,
                },
                fixture.Haptics.Raised);

            var eventSource = File.ReadAllText(
                Path.Combine(AppContext.BaseDirectory, "assets", "haptics", "DefaultEventSource.yaml"));
            var eventMapping = File.ReadAllText(
                Path.Combine(AppContext.BaseDirectory, "assets", "haptics", "extra", "eventMapping.yaml"));
            foreach (var eventName in HapticEventNames.All)
            {
                Assert.Contains($"name: {eventName}", eventSource, StringComparison.Ordinal);
                Assert.Contains($"  {eventName}:", eventMapping, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void PrimaryFeedbackRealizesOnlyEvidenceBackedImageAndHapticEffects()
        {
            var fixture = new IntegrationFixture();
            var feedback = fixture.Composition.PrimaryActionFeedback;

            foreach (var result in Enum.GetValues<DispatchResult>())
            {
                feedback.Present(RingActionId.NextSession, result);
            }

            Assert.Equal(
                Enumerable.Repeat(RingActionId.NextSession, 4),
                fixture.Images.Primary);
            Assert.Equal(
                new[]
                {
                    HapticEventNames.SelectionRejected,
                    HapticEventNames.DispatchRequested,
                    HapticEventNames.DispatchFailed,
                },
                fixture.Haptics.Raised);
        }

        [Fact]
        public void PrimaryFeedbackFailuresNeverEscapeOrRetryDispatchEffects()
        {
            var fixture = new IntegrationFixture();
            fixture.Images.ThrowOnPrimary = true;
            fixture.Haptics.ThrowOnRaise = true;

            var exception = Record.Exception(() => fixture.Composition.PrimaryActionFeedback.Present(
                RingActionId.NextSession,
                DispatchResult.DispatchFailed));

            Assert.Null(exception);
            Assert.Equal(new[] { RingActionId.NextSession }, fixture.Images.Primary);
            Assert.Equal(new[] { HapticEventNames.DispatchFailed }, fixture.Haptics.Raised);
        }

        [Fact]
        public void DesktopLogFormattingContainsOnlyVersionAndAnonymousCategory()
        {
            var text = PluginDesktopBridgeLogSink.Format(
                new DesktopBridgeLogEntry("0.1.0", "foreground_mismatch"));

            Assert.Equal("0.1.0 foreground_mismatch", text);
            Assert.DoesNotContain("/", text, StringComparison.Ordinal);
            Assert.DoesNotContain("prompt", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("task", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("credential", text, StringComparison.OrdinalIgnoreCase);
        }

        private static String InvokeProtectedString(Object target, String methodName)
        {
            var method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return Assert.IsType<String>(method.Invoke(target, null));
        }
    }
}
