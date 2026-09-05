#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.DesktopBridge
{
    using System;
    using System.Collections.Generic;
    using Loupedeck.ClaudeActionRingPlugin.Core;

    internal sealed class ClaudeDesktopBridge : IDesktopBridge
    {
        private readonly IFrontmostApplicationSource _frontmostApplications;
        private readonly IShortcutDispatcher _shortcuts;
        private readonly IDesktopBridgeLogSink _log;
        private readonly String _pluginVersion;

        internal ClaudeDesktopBridge(
            IFrontmostApplicationSource frontmostApplications,
            IShortcutDispatcher shortcuts,
            String pluginVersion,
            IDesktopBridgeLogSink? log = null)
        {
            this._frontmostApplications = frontmostApplications ??
                throw new ArgumentNullException(nameof(frontmostApplications));
            this._shortcuts = shortcuts ?? throw new ArgumentNullException(nameof(shortcuts));
            if (!Version.TryParse(pluginVersion, out var parsedVersion) ||
                parsedVersion.Major < 0 ||
                parsedVersion.Minor < 0 ||
                parsedVersion.Build < 0)
            {
                throw new ArgumentException("A three-part plugin version is required.", nameof(pluginVersion));
            }

            this._pluginVersion = parsedVersion.ToString(3);
            this._log = log ?? NullDesktopBridgeLogSink.Instance;
        }

        public DispatchResult Dispatch(KeyboardShortcut shortcut)
        {
            IReadOnlyList<FrontmostApplicationIdentity> frontmost;

            try
            {
                frontmost = this._frontmostApplications.Read();
            }
            catch (Exception)
            {
                return this.FailClosed("foreground_query_failed");
            }

            if (!ClaudeFrontmostPolicy.IsUnambiguousMatch(frontmost))
            {
                var category = frontmost.Count switch
                {
                    0 => "foreground_unavailable",
                    > 1 => "foreground_ambiguous",
                    _ => "foreground_mismatch",
                };

                return this.FailClosed(category);
            }

            try
            {
                return this.MapOutcome(this._shortcuts.Dispatch(shortcut));
            }
            catch (Exception)
            {
                this.WriteLog("dispatch_exception");
                return DispatchResult.OutcomeUnknown;
            }
        }

        private DispatchResult MapOutcome(LocalDispatchOutcome outcome)
        {
            switch (outcome)
            {
                case LocalDispatchOutcome.Accepted:
                    return DispatchResult.DispatchRequested;
                case LocalDispatchOutcome.Rejected:
                    this.WriteLog("dispatch_rejected");
                    return DispatchResult.DispatchFailed;
                case LocalDispatchOutcome.AcceptanceUnknown:
                default:
                    this.WriteLog("dispatch_acceptance_unknown");
                    return DispatchResult.OutcomeUnknown;
            }
        }

        private DispatchResult FailClosed(String category)
        {
            this.WriteLog(category);
            return DispatchResult.NotDispatched;
        }

        private void WriteLog(String category)
        {
            try
            {
                this._log.Write(new DesktopBridgeLogEntry(this._pluginVersion, category));
            }
            catch (Exception)
            {
                // Diagnostics must never change dispatch behavior.
            }
        }
    }
}
