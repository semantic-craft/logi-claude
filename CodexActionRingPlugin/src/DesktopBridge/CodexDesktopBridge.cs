#nullable enable

namespace Loupedeck.CodexActionRingPlugin.DesktopBridge
{
    using System;
    using System.Collections.Generic;
    using Loupedeck.CodexActionRingPlugin.Core;

    internal sealed class CodexDesktopBridge : IDesktopBridge
    {
        internal const String NewChatUri = "codex://threads/new";

        private readonly IFrontmostApplicationSource _frontmostApplications;
        private readonly IShortcutDispatcher _shortcuts;
        private readonly IDeepLinkDispatcher _deepLinks;
        private readonly IDesktopBridgeLogSink _log;
        private readonly String _pluginVersion;

        internal CodexDesktopBridge(
            IFrontmostApplicationSource frontmostApplications,
            IShortcutDispatcher shortcuts,
            IDeepLinkDispatcher deepLinks,
            String pluginVersion,
            IDesktopBridgeLogSink? log = null)
        {
            this._frontmostApplications = frontmostApplications ??
                throw new ArgumentNullException(nameof(frontmostApplications));
            this._shortcuts = shortcuts ?? throw new ArgumentNullException(nameof(shortcuts));
            this._deepLinks = deepLinks ?? throw new ArgumentNullException(nameof(deepLinks));
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

        public DispatchResult Dispatch(DesktopInvocation invocation)
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

            if (!CodexFrontmostPolicy.IsUnambiguousMatch(frontmost))
            {
                var category = frontmost.Count switch
                {
                    0 => "foreground_unavailable",
                    > 1 => "foreground_ambiguous",
                    _ => "foreground_mismatch",
                };

                return this.FailClosed(category);
            }

            return invocation.Kind switch
            {
                DesktopInvocationKind.Shortcut => this.DispatchShortcut(invocation.Shortcut),
                DesktopInvocationKind.Uri => this.DispatchDeepLink(invocation.Uri),
                _ => this.FailClosed("invalid_invocation"),
            };
        }

        private DispatchResult DispatchShortcut(KeyboardShortcut shortcut)
        {
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

        private DispatchResult DispatchDeepLink(Uri? uri)
        {
            if (uri is null || !String.Equals(uri.AbsoluteUri, NewChatUri, StringComparison.Ordinal))
            {
                return this.FailClosed("unsupported_uri");
            }

            try
            {
                return this.MapOutcome(this._deepLinks.Dispatch(uri));
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
