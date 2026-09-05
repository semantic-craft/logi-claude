namespace Loupedeck.ClaudeActionRingPlugin.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal sealed record RingActionDefinition(
        RingActionId Id,
        String StableId,
        String Label,
        String IconKey,
        ActionPrecondition Precondition,
        RingActionDelivery Delivery);

    internal static class RingActionCatalog
    {
        private const DesktopModifiers Command = DesktopModifiers.Command;
        private const DesktopModifiers Control = DesktopModifiers.Control;
        private const DesktopModifiers Shift = DesktopModifiers.Shift;

        private static readonly IReadOnlyList<RingActionDefinition> _definitions =
            Array.AsReadOnly(new[]
            {
                Shortcut(RingActionId.PermissionMode, "permission_mode", "Permission Mode", Command | Shift, DesktopKey.M),
                Shortcut(RingActionId.ModelMenu, "model_menu", "Model", Command | Shift, DesktopKey.I),
                Shortcut(RingActionId.EffortMenu, "effort_menu", "Effort", Command | Shift, DesktopKey.E),
                Shortcut(RingActionId.SideChat, "side_chat", "Side Chat", Command, DesktopKey.Semicolon),
                Shortcut(RingActionId.ToggleDiff, "toggle_diff", "Toggle Diff", Command | Shift, DesktopKey.D),
                Shortcut(RingActionId.ToggleTerminal, "toggle_terminal", "Toggle Terminal", Control, DesktopKey.Grave),
                Shortcut(RingActionId.ViewMode, "view_mode", "View Mode", Control, DesktopKey.O),
                Shortcut(RingActionId.NextSession, "next_session", "Next Session", Control, DesktopKey.Tab),
            });

        private static readonly IReadOnlyDictionary<RingActionId, RingActionDefinition> _byId =
            BuildIndex(RingActionCatalog._definitions);

        private static readonly IReadOnlyList<RingActionId> _primaryOrder = Array.AsReadOnly(new[]
        {
            RingActionId.PermissionMode,
            RingActionId.ModelMenu,
            RingActionId.EffortMenu,
            RingActionId.SideChat,
            RingActionId.ToggleDiff,
            RingActionId.ToggleTerminal,
            RingActionId.ViewMode,
            RingActionId.NextSession,
        });

        internal static IReadOnlyList<RingActionDefinition> Definitions => RingActionCatalog._definitions;

        internal static IReadOnlyList<RingActionId> PrimaryOrder => RingActionCatalog._primaryOrder;

        internal static Boolean TryGetDefinition(RingActionId id, out RingActionDefinition definition) =>
            RingActionCatalog._byId.TryGetValue(id, out definition!);

        private static RingActionDefinition Shortcut(
            RingActionId id,
            String stableId,
            String label,
            DesktopModifiers modifiers,
            DesktopKey key) =>
            new(
                id,
                stableId,
                label,
                stableId,
                ActionPrecondition.ClaudeFrontmost,
                new RingActionDelivery.Desktop(new KeyboardShortcut(modifiers, key)));

        private static ReadOnlyDictionary<RingActionId, RingActionDefinition> BuildIndex(
            IReadOnlyList<RingActionDefinition> definitions)
        {
            var index = new Dictionary<RingActionId, RingActionDefinition>(definitions.Count);

            foreach (var definition in definitions)
            {
                index.Add(definition.Id, definition);
            }

            return new ReadOnlyDictionary<RingActionId, RingActionDefinition>(index);
        }
    }
}
