#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.DesktopBridge
{
    using System;
    using System.Collections.Generic;
    using Loupedeck.ClaudeActionRingPlugin.Core;

    internal static class ClaudeFrontmostPolicy
    {
        internal const String ProcessName = "Claude";
        internal const String BundleIdentifier = "com.anthropic.claudefordesktop";

        internal static Boolean IsUnambiguousMatch(
            IReadOnlyList<FrontmostApplicationIdentity>? applications) =>
            applications is { Count: 1 } &&
            String.Equals(applications[0].ProcessName, ProcessName, StringComparison.Ordinal) &&
            String.Equals(applications[0].BundleIdentifier, BundleIdentifier, StringComparison.Ordinal);
    }

    internal sealed class ClaudeFrontmostPreconditionEvaluator : IActionPreconditionEvaluator
    {
        private readonly IFrontmostApplicationSource _frontmostApplications;

        internal ClaudeFrontmostPreconditionEvaluator(IFrontmostApplicationSource frontmostApplications)
        {
            this._frontmostApplications = frontmostApplications ??
                throw new ArgumentNullException(nameof(frontmostApplications));
        }

        public Boolean IsSatisfied(ActionPrecondition precondition)
        {
            if (precondition == ActionPrecondition.None)
            {
                return true;
            }

            if (precondition != ActionPrecondition.ClaudeFrontmost)
            {
                return false;
            }

            try
            {
                return ClaudeFrontmostPolicy.IsUnambiguousMatch(this._frontmostApplications.Read());
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
