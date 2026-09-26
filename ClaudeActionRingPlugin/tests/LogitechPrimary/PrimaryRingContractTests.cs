#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.Logitech.Primary
{
    using Loupedeck.ClaudeActionRingPlugin.Core;
    using Xunit;

    public sealed class PrimaryRingContractTests
    {
        private static readonly RingActionId[] ExpectedOrder =
        {
            RingActionId.PermissionMode,
            RingActionId.ModelMenu,
            RingActionId.EffortMenu,
            RingActionId.SideChat,
            RingActionId.ToggleBrowser,
            RingActionId.ToggleTerminal,
            RingActionId.ViewMode,
            RingActionId.NextSession,
            RingActionId.StopResponse,
            RingActionId.SelectElement,
            RingActionId.NewSession,
            RingActionId.PreviousSession,
            RingActionId.ClosePane,
        };

        [Fact]
        public void PrimaryOrderMatchesTheLockedCatalogContract()
        {
            Assert.Equal(PrimaryRingContractTests.ExpectedOrder, PrimaryRingContract.Order);
            Assert.Equal(
                PrimaryRingContractTests.ExpectedOrder,
                PrimaryRingContract.Entries.Select(entry => entry.Id));
        }

        [Fact]
        public void AllEntriesMapToUniqueSdkActionsAndSemanticIconKeys()
        {
            var commands = PrimaryRingContract.Entries.ToArray();

            var expected = new[]
            {
                (RingActionId.PermissionMode, "permission_mode", "Loupedeck.ClaudeActionRingPlugin.Logitech.Primary.PermissionModeCommand"),
                (RingActionId.ModelMenu, "model_menu", "Loupedeck.ClaudeActionRingPlugin.Logitech.Primary.ModelMenuCommand"),
                (RingActionId.EffortMenu, "effort_menu", "Loupedeck.ClaudeActionRingPlugin.Logitech.Primary.EffortMenuCommand"),
                (RingActionId.SideChat, "side_chat", "Loupedeck.ClaudeActionRingPlugin.Logitech.Primary.SideChatCommand"),
                (RingActionId.ToggleBrowser, "toggle_browser", "Loupedeck.ClaudeActionRingPlugin.Logitech.Primary.ToggleBrowserCommand"),
                (RingActionId.ToggleTerminal, "toggle_terminal", "Loupedeck.ClaudeActionRingPlugin.Logitech.Primary.ToggleTerminalCommand"),
                (RingActionId.ViewMode, "view_mode", "Loupedeck.ClaudeActionRingPlugin.Logitech.Primary.ViewModeCommand"),
                (RingActionId.NextSession, "next_session", "Loupedeck.ClaudeActionRingPlugin.Logitech.Primary.NextSessionCommand"),
                (RingActionId.StopResponse, "stop_response", "Loupedeck.ClaudeActionRingPlugin.Logitech.Primary.StopResponseCommand"),
                (RingActionId.SelectElement, "select_element", "Loupedeck.ClaudeActionRingPlugin.Logitech.Primary.SelectElementCommand"),
                (RingActionId.NewSession, "new_session", "Loupedeck.ClaudeActionRingPlugin.Logitech.Primary.NewSessionCommand"),
                (RingActionId.PreviousSession, "previous_session", "Loupedeck.ClaudeActionRingPlugin.Logitech.Primary.PreviousSessionCommand"),
                (RingActionId.ClosePane, "close_pane", "Loupedeck.ClaudeActionRingPlugin.Logitech.Primary.ClosePaneCommand"),
            };

            Assert.Equal(13, commands.Length);
            Assert.Equal(13, commands.Select(entry => entry.WrapperType).Distinct().Count());
            Assert.Equal(13, commands.Select(entry => entry.ActionName).Distinct().Count());
            Assert.Equal(
                expected,
                commands.Select(entry => (entry.Id, entry.IconKey, entry.ActionName)));

            foreach (var entry in commands)
            {
                Assert.True(typeof(PrimaryActionCommand).IsAssignableFrom(entry.WrapperType));
                Assert.Equal(entry.WrapperType.FullName, entry.ActionName);
                Assert.Equal(entry.StableId, entry.IconKey);
                Assert.DoesNotContain("Approve", entry.ActionName, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("Decline", entry.ActionName, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void EntryMetadataComesFromTheCoreCatalog()
        {
            foreach (var entry in PrimaryRingContract.Entries)
            {
                Assert.True(RingActionCatalog.TryGetDefinition(entry.Id, out var definition));
                Assert.Equal(definition.StableId, entry.StableId);
                Assert.Equal(definition.Label, entry.Label);
                Assert.Equal(definition.IconKey, entry.IconKey);
            }
        }
    }
}
