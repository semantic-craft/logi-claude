#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.Composition
{
    using System;
    using Loupedeck.ClaudeActionRingPlugin.Core;
    using Loupedeck.ClaudeActionRingPlugin.DesktopBridge;
    using Loupedeck.ClaudeActionRingPlugin.Logitech.Primary;

    internal sealed class ActionRingComposition : IPrimaryActionDependencyProvider
    {
        internal const String PluginVersion = "0.1.0";

        private readonly FeedbackCoordinator _feedback;
        private readonly HapticFeedbackAdapter _haptics;

        internal ActionRingComposition(CompositionPorts ports)
        {
            ArgumentNullException.ThrowIfNull(ports);

            var bridge = new ClaudeDesktopBridge(
                ports.FrontmostApplications,
                ports.Shortcuts,
                PluginVersion,
                ports.Log);
            var executor = new ActionExecutor(
                bridge,
                new ClaudeFrontmostPreconditionEvaluator(ports.FrontmostApplications));

            this._haptics = new HapticFeedbackAdapter(ports.Haptics);
            this._feedback = new FeedbackCoordinator(ports.Images, this._haptics);
            this.PrimaryActionExecutor = executor;
            this.PrimaryActionFeedback = this._feedback;
        }

        public IActionExecutor PrimaryActionExecutor { get; }

        public IPrimaryFeedbackAdapter PrimaryActionFeedback { get; }

        internal void Load() => this._haptics.RegisterAll();

        internal static ActionRingComposition CreateProduction(ClaudeActionRingPlugin plugin)
        {
            ArgumentNullException.ThrowIfNull(plugin);

            return new ActionRingComposition(new CompositionPorts(
                new MacOsWorkspaceAdapter(),
                new LogitechShortcutDispatcher((key, modifiers) =>
                    plugin.ClientApplication.SendKeyboardShortcut(key, modifiers)),
                new PluginDesktopBridgeLogSink(),
                new LogitechActionImageInvalidator(plugin),
                new LogitechHapticEventSink(() => plugin.PluginEvents)));
        }
    }
}
