namespace Loupedeck.CodexActionRingPlugin
{
    using System;

    // This class can be used to connect the Loupedeck plugin to an application.

    public class CodexActionRingApplication : ClientApplication
    {
        public CodexActionRingApplication()
        {
        }

        // Link the plugin to the Codex desktop executable.
        protected override String GetProcessName() => "ChatGPT";

        // Link the plugin to the Codex desktop bundle on macOS.
        protected override String GetBundleName() => "com.openai.codex";

        // This method can be used to check whether the application is installed or not.
        public override ClientApplicationStatus GetApplicationStatus() => ClientApplicationStatus.Unknown;
    }
}
