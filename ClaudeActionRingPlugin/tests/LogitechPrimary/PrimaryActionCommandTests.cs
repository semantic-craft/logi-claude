#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.Logitech.Primary
{
    using System.Reflection;
    using Loupedeck.ClaudeActionRingPlugin.Core;
    using Xunit;

    public sealed class PrimaryActionCommandTests
    {
        public static IEnumerable<Object[]> Wrappers()
        {
            yield return Case(
                RingActionId.PermissionMode,
                static (executor, feedback) => new PermissionModeCommand(executor, feedback));
            yield return Case(
                RingActionId.ModelMenu,
                static (executor, feedback) => new ModelMenuCommand(executor, feedback));
            yield return Case(
                RingActionId.EffortMenu,
                static (executor, feedback) => new EffortMenuCommand(executor, feedback));
            yield return Case(
                RingActionId.SideChat,
                static (executor, feedback) => new SideChatCommand(executor, feedback));
            yield return Case(
                RingActionId.ToggleDiff,
                static (executor, feedback) => new ToggleDiffCommand(executor, feedback));
            yield return Case(
                RingActionId.ToggleTerminal,
                static (executor, feedback) => new ToggleTerminalCommand(executor, feedback));
            yield return Case(
                RingActionId.ViewMode,
                static (executor, feedback) => new ViewModeCommand(executor, feedback));
            yield return Case(
                RingActionId.NextSession,
                static (executor, feedback) => new NextSessionCommand(executor, feedback));
        }

        [Theory]
        [MemberData(nameof(Wrappers))]
        public void EachWrapperDelegatesExactlyOnceAndForwardsUnchangedResult(
            RingActionId expectedId,
            Func<IActionExecutor, IPrimaryFeedbackAdapter, PrimaryActionCommand> create)
        {
            foreach (var result in Enum.GetValues<DispatchResult>())
            {
                var executor = new RecordingExecutor { Result = result };
                var feedback = new RecordingFeedbackAdapter();
                var command = create(executor, feedback);

                Assert.True(command.TryRunCommand(String.Empty));
                Assert.Equal(new[] { expectedId }, executor.Calls);
                Assert.Equal(new[] { (expectedId, result) }, feedback.Calls);
            }
        }

        [Theory]
        [MemberData(nameof(Wrappers))]
        public void EachWrapperIsAnAssignableExtendedFamilySdkAction(
            RingActionId expectedId,
            Func<IActionExecutor, IPrimaryFeedbackAdapter, PrimaryActionCommand> create)
        {
            var command = create(new RecordingExecutor(), new RecordingFeedbackAdapter());

            Assert.Equal(expectedId, command.ActionId);
            Assert.Equal(DeviceType.LoupedeckExtendedFamily, command.SupportedDevices);
            Assert.NotNull(command.GetType().GetConstructor(Type.EmptyTypes));
            Assert.True(command.GetType().IsSealed);
        }

        [Fact]
        public void WrappersOverrideOnlyTheSdkSelectionCallback()
        {
            var sdkOverrides = typeof(PrimaryActionCommand)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(method => method.GetBaseDefinition().DeclaringType != method.DeclaringType)
                .Select(method => method.Name)
                .ToArray();

            Assert.Equal(new[] { "RunCommand" }, sdkOverrides);
        }

        [Fact]
        public void SdkCanDiscoverEveryPublicWrapperBeforeCompositionAndResolveAtSelectionTime()
        {
            var commands = PrimaryRingContract.Entries
                .Select(entry => (entry.Id, Command: Assert.IsAssignableFrom<PrimaryActionCommand>(
                    Activator.CreateInstance(entry.WrapperType))))
                .ToArray();
            var executor = new RecordingExecutor();
            var feedback = new RecordingFeedbackAdapter();
            var plugin = new TestPlugin(executor, feedback);

            foreach (var (_, command) in commands)
            {
                SetSdkPlugin(command, plugin);
                Assert.True(command.TryRunCommand(String.Empty));
            }

            Assert.Equal(commands.Select(item => item.Id), executor.Calls);
            Assert.Equal(
                commands.Select(item => (item.Id, DispatchResult.DispatchRequested)),
                feedback.Calls);
        }

        [Fact]
        public void UncomposedSdkWrapperFailsClosedWithZeroDispatchAndFeedback()
        {
            var command = new SideChatCommand();

            Assert.True(command.TryRunCommand(String.Empty));
        }

        private static Object[] Case(
            RingActionId actionId,
            Func<IActionExecutor, IPrimaryFeedbackAdapter, PrimaryActionCommand> create) =>
            new Object[] { actionId, create };

        private static void SetSdkPlugin(PrimaryActionCommand command, Plugin plugin)
        {
            var pluginProperty = typeof(PluginDynamicAction).GetProperty(
                "Plugin",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(pluginProperty);
            pluginProperty.SetValue(command, plugin);
        }
    }
}
