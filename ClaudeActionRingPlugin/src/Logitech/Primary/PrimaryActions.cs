#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.Logitech.Primary
{
    using Loupedeck.ClaudeActionRingPlugin.Core;

    public sealed class PermissionModeCommand : PrimaryActionCommand
    {
        public PermissionModeCommand()
            : base(RingActionId.PermissionMode)
        {
        }

        internal PermissionModeCommand(IActionExecutor executor, IPrimaryFeedbackAdapter feedback)
            : base(RingActionId.PermissionMode, executor, feedback)
        {
        }
    }

    public sealed class ModelMenuCommand : PrimaryActionCommand
    {
        public ModelMenuCommand()
            : base(RingActionId.ModelMenu)
        {
        }

        internal ModelMenuCommand(IActionExecutor executor, IPrimaryFeedbackAdapter feedback)
            : base(RingActionId.ModelMenu, executor, feedback)
        {
        }
    }

    public sealed class EffortMenuCommand : PrimaryActionCommand
    {
        public EffortMenuCommand()
            : base(RingActionId.EffortMenu)
        {
        }

        internal EffortMenuCommand(IActionExecutor executor, IPrimaryFeedbackAdapter feedback)
            : base(RingActionId.EffortMenu, executor, feedback)
        {
        }
    }

    public sealed class SideChatCommand : PrimaryActionCommand
    {
        public SideChatCommand()
            : base(RingActionId.SideChat)
        {
        }

        internal SideChatCommand(IActionExecutor executor, IPrimaryFeedbackAdapter feedback)
            : base(RingActionId.SideChat, executor, feedback)
        {
        }
    }

    public sealed class ToggleBrowserCommand : PrimaryActionCommand
    {
        public ToggleBrowserCommand()
            : base(RingActionId.ToggleBrowser)
        {
        }

        internal ToggleBrowserCommand(IActionExecutor executor, IPrimaryFeedbackAdapter feedback)
            : base(RingActionId.ToggleBrowser, executor, feedback)
        {
        }
    }

    public sealed class ToggleTerminalCommand : PrimaryActionCommand
    {
        public ToggleTerminalCommand()
            : base(RingActionId.ToggleTerminal)
        {
        }

        internal ToggleTerminalCommand(IActionExecutor executor, IPrimaryFeedbackAdapter feedback)
            : base(RingActionId.ToggleTerminal, executor, feedback)
        {
        }
    }

    public sealed class ViewModeCommand : PrimaryActionCommand
    {
        public ViewModeCommand()
            : base(RingActionId.ViewMode)
        {
        }

        internal ViewModeCommand(IActionExecutor executor, IPrimaryFeedbackAdapter feedback)
            : base(RingActionId.ViewMode, executor, feedback)
        {
        }
    }

    public sealed class NextSessionCommand : PrimaryActionCommand
    {
        public NextSessionCommand()
            : base(RingActionId.NextSession)
        {
        }

        internal NextSessionCommand(IActionExecutor executor, IPrimaryFeedbackAdapter feedback)
            : base(RingActionId.NextSession, executor, feedback)
        {
        }
    }

    public sealed class StopResponseCommand : PrimaryActionCommand
    {
        public StopResponseCommand()
            : base(RingActionId.StopResponse)
        {
        }

        internal StopResponseCommand(IActionExecutor executor, IPrimaryFeedbackAdapter feedback)
            : base(RingActionId.StopResponse, executor, feedback)
        {
        }
    }

    public sealed class SelectElementCommand : PrimaryActionCommand
    {
        public SelectElementCommand()
            : base(RingActionId.SelectElement)
        {
        }

        internal SelectElementCommand(IActionExecutor executor, IPrimaryFeedbackAdapter feedback)
            : base(RingActionId.SelectElement, executor, feedback)
        {
        }
    }

    public sealed class NewSessionCommand : PrimaryActionCommand
    {
        public NewSessionCommand()
            : base(RingActionId.NewSession)
        {
        }

        internal NewSessionCommand(IActionExecutor executor, IPrimaryFeedbackAdapter feedback)
            : base(RingActionId.NewSession, executor, feedback)
        {
        }
    }

    public sealed class PreviousSessionCommand : PrimaryActionCommand
    {
        public PreviousSessionCommand()
            : base(RingActionId.PreviousSession)
        {
        }

        internal PreviousSessionCommand(IActionExecutor executor, IPrimaryFeedbackAdapter feedback)
            : base(RingActionId.PreviousSession, executor, feedback)
        {
        }
    }

    public sealed class ClosePaneCommand : PrimaryActionCommand
    {
        public ClosePaneCommand()
            : base(RingActionId.ClosePane)
        {
        }

        internal ClosePaneCommand(IActionExecutor executor, IPrimaryFeedbackAdapter feedback)
            : base(RingActionId.ClosePane, executor, feedback)
        {
        }
    }
}
