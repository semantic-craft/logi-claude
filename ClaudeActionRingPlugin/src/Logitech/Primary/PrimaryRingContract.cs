#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.Logitech.Primary
{
    using System;
    using System.Collections.Generic;
    using Loupedeck.ClaudeActionRingPlugin.Core;

    internal sealed record PrimaryRingEntry(
        RingActionId Id,
        String StableId,
        String Label,
        String IconKey,
        Type WrapperType,
        String ActionName);

    internal static class PrimaryRingContract
    {
        private static readonly IReadOnlyDictionary<RingActionId, Type> _wrapperTypes =
            new Dictionary<RingActionId, Type>
            {
                [RingActionId.PermissionMode] = typeof(PermissionModeCommand),
                [RingActionId.ModelMenu] = typeof(ModelMenuCommand),
                [RingActionId.EffortMenu] = typeof(EffortMenuCommand),
                [RingActionId.SideChat] = typeof(SideChatCommand),
                [RingActionId.ToggleDiff] = typeof(ToggleDiffCommand),
                [RingActionId.ToggleTerminal] = typeof(ToggleTerminalCommand),
                [RingActionId.ViewMode] = typeof(ViewModeCommand),
                [RingActionId.NextSession] = typeof(NextSessionCommand),
            };

        private static readonly IReadOnlyList<PrimaryRingEntry> _entries = BuildEntries();

        internal static IReadOnlyList<PrimaryRingEntry> Entries => PrimaryRingContract._entries;

        internal static IReadOnlyList<RingActionId> Order => RingActionCatalog.PrimaryOrder;

        internal static RingActionDefinition GetDefinition(RingActionId actionId)
        {
            if (!RingActionCatalog.TryGetDefinition(actionId, out var definition)
                || !RingActionCatalog.PrimaryOrder.Contains(actionId))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(actionId),
                    actionId,
                    "Action is not part of the Primary Ring.");
            }

            return definition;
        }

        private static IReadOnlyList<PrimaryRingEntry> BuildEntries()
        {
            var entries = new List<PrimaryRingEntry>(RingActionCatalog.PrimaryOrder.Count);

            foreach (var actionId in RingActionCatalog.PrimaryOrder)
            {
                var definition = PrimaryRingContract.GetDefinition(actionId);

                var wrapperType = PrimaryRingContract._wrapperTypes[actionId];
                entries.Add(new PrimaryRingEntry(
                    definition.Id,
                    definition.StableId,
                    definition.Label,
                    definition.IconKey,
                    wrapperType,
                    wrapperType.FullName
                        ?? throw new InvalidOperationException("Primary wrapper must have a full action name.")));
            }

            return Array.AsReadOnly(entries.ToArray());
        }
    }
}
