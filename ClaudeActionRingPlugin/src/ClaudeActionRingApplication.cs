namespace Loupedeck.ClaudeActionRingPlugin
{
    using System;

    // This class can be used to connect the Loupedeck plugin to an application.

    public class ClaudeActionRingApplication : ClientApplication
    {
        public ClaudeActionRingApplication()
        {
        }

        // Link the plugin to the Claude desktop executable.
        protected override String GetProcessName() => "Claude";

        // Link the plugin to the Claude desktop bundle on macOS.
        protected override String GetBundleName() => "com.anthropic.claudefordesktop";

        // This method can be used to check whether the application is installed or not.
        public override ClientApplicationStatus GetApplicationStatus() => ClientApplicationStatus.Unknown;
    }
}
