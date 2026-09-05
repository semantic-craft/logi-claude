#nullable enable

namespace Loupedeck.CodexActionRingPlugin.DesktopBridge
{
    using System;
    using System.Collections.Generic;
    using Loupedeck.CodexActionRingPlugin.Core;

    internal static class CodexFrontmostPolicy
    {
        internal const String ProcessName = "ChatGPT";
        internal const String BundleIdentifier = "com.openai.codex";

        internal static Boolean IsUnambiguousMatch(
            IReadOnlyList<FrontmostApplicationIdentity>? applications) =>
            applications is { Count: 1 } &&
            String.Equals(applications[0].ProcessName, ProcessName, StringComparison.Ordinal) &&
            String.Equals(applications[0].BundleIdentifier, BundleIdentifier, StringComparison.Ordinal);
    }

    internal sealed class CodexFrontmostPreconditionEvaluator : IActionPreconditionEvaluator
    {
        private readonly IFrontmostApplicationSource _frontmostApplications;

        internal CodexFrontmostPreconditionEvaluator(IFrontmostApplicationSource frontmostApplications)
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

            if (precondition != ActionPrecondition.CodexFrontmost)
            {
                return false;
            }

            try
            {
                return CodexFrontmostPolicy.IsUnambiguousMatch(this._frontmostApplications.Read());
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
