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

    public sealed class ToggleDiffCommand : PrimaryActionCommand
    {
        public ToggleDiffCommand()
            : base(RingActionId.ToggleDiff)
        {
        }

        internal ToggleDiffCommand(IActionExecutor executor, IPrimaryFeedbackAdapter feedback)
            : base(RingActionId.ToggleDiff, executor, feedback)
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
}
