namespace Loupedeck.CodexActionRingPlugin.Feedback
{
    using System;
    using System.Collections.Generic;

    public static class HapticEventNames
    {
        public const String DispatchRequested = "DispatchRequested";
        public const String DispatchFailed = "DispatchFailed";
        public const String SelectionRejected = "SelectionRejected";

        public static IReadOnlyList<String> All { get; } = Array.AsReadOnly(
            new[]
            {
                DispatchRequested,
                DispatchFailed,
                SelectionRejected,
            });

        public static String For(FeedbackCue cue) => cue switch
        {
            FeedbackCue.DispatchRequested => DispatchRequested,
            FeedbackCue.DispatchFailed => DispatchFailed,
            FeedbackCue.SelectionRejected => SelectionRejected,
            _ => throw new ArgumentOutOfRangeException(nameof(cue), cue, "Unknown feedback cue."),
        };
    }
}
