namespace Loupedeck.ClaudeActionRingPlugin.Core.Tests
{
    using System;
    using System.Linq;
    using Loupedeck.ClaudeActionRingPlugin.Core;
    using Xunit;

    public sealed class RingActionCatalogTests
    {
        [Fact]
        public void CatalogContainsTheExactLockedStaticMetadata()
        {
            var expected = new[]
            {
                Metadata(RingActionId.PermissionMode, "permission_mode", "Permission Mode"),
                Metadata(RingActionId.ModelMenu, "model_menu", "Model"),
                Metadata(RingActionId.EffortMenu, "effort_menu", "Effort"),
                Metadata(RingActionId.SideChat, "side_chat", "Side Chat"),
                Metadata(RingActionId.ToggleDiff, "toggle_diff", "Toggle Diff"),
                Metadata(RingActionId.ToggleTerminal, "toggle_terminal", "Toggle Terminal"),
                Metadata(RingActionId.ViewMode, "view_mode", "View Mode"),
                Metadata(RingActionId.NextSession, "next_session", "Next Session"),
            };

            var actual = RingActionCatalog.Definitions.Select(definition =>
                new ExpectedMetadata(definition.Id, definition.StableId, definition.Label, definition.IconKey));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ExecutorResolvesTheExactLockedDeliveryMappings()
        {
            var expected = new[]
            {
                Shortcut(RingActionId.PermissionMode, DesktopModifiers.Command | DesktopModifiers.Shift, DesktopKey.M),
                Shortcut(RingActionId.ModelMenu, DesktopModifiers.Command | DesktopModifiers.Shift, DesktopKey.I),
                Shortcut(RingActionId.EffortMenu, DesktopModifiers.Command | DesktopModifiers.Shift, DesktopKey.E),
                Shortcut(RingActionId.SideChat, DesktopModifiers.Command, DesktopKey.Semicolon),
                Shortcut(RingActionId.ToggleDiff, DesktopModifiers.Command | DesktopModifiers.Shift, DesktopKey.D),
                Shortcut(RingActionId.ToggleTerminal, DesktopModifiers.Control, DesktopKey.Grave),
                Shortcut(RingActionId.ViewMode, DesktopModifiers.Control, DesktopKey.O),
                Shortcut(RingActionId.NextSession, DesktopModifiers.Control, DesktopKey.Tab),
            };

            foreach (var delivery in expected)
            {
                var bridge = new RecordingDesktopBridge();
                var preconditions = new RecordingPreconditionEvaluator();
                IActionExecutor executor = new ActionExecutor(bridge, preconditions);

                var result = executor.Execute(delivery.ActionId);

                var shortcut = Assert.Single(bridge.Shortcuts);
                Assert.Equal(DispatchResult.DispatchRequested, result);
                Assert.Equal(new KeyboardShortcut(delivery.Modifiers, delivery.Key), shortcut);
            }
        }

        [Fact]
        public void EveryActionIsADocumentedCodeTabShortcutWithForegroundPrecondition()
        {
            Assert.All(RingActionCatalog.Definitions, definition =>
            {
                Assert.IsType<RingActionDelivery.Desktop>(definition.Delivery);
                Assert.Equal(ActionPrecondition.ClaudeFrontmost, definition.Precondition);
            });
            Assert.Equal(
                8,
                RingActionCatalog.Definitions
                    .Select(definition => ((RingActionDelivery.Desktop)definition.Delivery).Shortcut)
                    .Distinct()
                    .Count());
        }

        [Fact]
        public void CatalogHasExactlyEightUniqueIdsAndIconKeys()
        {
            Assert.Equal(8, RingActionCatalog.Definitions.Count);
            Assert.Equal(8, Enum.GetValues<RingActionId>().Length);
            Assert.Equal(8, RingActionCatalog.Definitions.Select(definition => definition.Id).Distinct().Count());
            Assert.Equal(8, RingActionCatalog.Definitions.Select(definition => definition.StableId).Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(8, RingActionCatalog.Definitions.Select(definition => definition.IconKey).Distinct(StringComparer.Ordinal).Count());
            Assert.All(RingActionCatalog.Definitions, definition => Assert.Equal(definition.StableId, definition.IconKey));
        }

        [Fact]
        public void PrimaryOrderIsFixedClockwiseFromTheTop()
        {
            Assert.Equal(
                new[]
                {
                    RingActionId.PermissionMode,
                    RingActionId.ModelMenu,
                    RingActionId.EffortMenu,
                    RingActionId.SideChat,
                    RingActionId.ToggleDiff,
                    RingActionId.ToggleTerminal,
                    RingActionId.ViewMode,
                    RingActionId.NextSession,
                },
                RingActionCatalog.PrimaryOrder);
        }

        [Fact]
        public void ApprovalActionsCannotBeRepresented()
        {
            var names = Enum.GetNames<RingActionId>();
            var stableIds = RingActionCatalog.Definitions.Select(definition => definition.StableId);

            Assert.DoesNotContain(names, name => String.Equals(name, "Approve", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(names, name => String.Equals(name, "Decline", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(stableIds, id => id.Contains("approve", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(stableIds, id => id.Contains("decline", StringComparison.OrdinalIgnoreCase));
        }

        private static ExpectedMetadata Metadata(RingActionId id, String stableId, String label) =>
            new(id, stableId, label, stableId);

        private static ExpectedDelivery Shortcut(
            RingActionId id,
            DesktopModifiers modifiers,
            DesktopKey key) =>
            new(id, modifiers, key);

        private sealed record ExpectedMetadata(
            RingActionId Id,
            String StableId,
            String Label,
            String IconKey);

        private sealed record ExpectedDelivery(
            RingActionId ActionId,
            DesktopModifiers Modifiers,
            DesktopKey Key);
    }
}
