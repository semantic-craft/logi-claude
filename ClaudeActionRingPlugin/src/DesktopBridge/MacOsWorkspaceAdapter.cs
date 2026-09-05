#nullable enable

namespace Loupedeck.ClaudeActionRingPlugin.DesktopBridge
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal sealed class MacOsWorkspaceAdapter : IFrontmostApplicationSource
    {
        private const String AppKitPath = "/System/Library/Frameworks/AppKit.framework/AppKit";
        private const String ObjectiveCLibrary = "/usr/lib/libobjc.A.dylib";

        private static readonly Object _frameworkLock = new();
        private static IntPtr _appKitHandle;

        public IReadOnlyList<FrontmostApplicationIdentity> Read()
        {
            EnsureMacOsAndAppKit();

            var workspace = Send(GetClass("NSWorkspace"), GetSelector("sharedWorkspace"));
            var application = Send(workspace, GetSelector("frontmostApplication"));

            if (application == IntPtr.Zero)
            {
                return Array.Empty<FrontmostApplicationIdentity>();
            }

            var executableUrl = Send(application, GetSelector("executableURL"));
            var processName = ReadString(Send(executableUrl, GetSelector("lastPathComponent")));
            var bundleIdentifier = ReadString(Send(application, GetSelector("bundleIdentifier")));

            return new[] { new FrontmostApplicationIdentity(processName, bundleIdentifier) };
        }

        private static void EnsureMacOsAndAppKit()
        {
            if (!OperatingSystem.IsMacOS())
            {
                throw new PlatformNotSupportedException("The production workspace adapter requires macOS.");
            }

            if (_appKitHandle != IntPtr.Zero)
            {
                return;
            }

            lock (_frameworkLock)
            {
                if (_appKitHandle == IntPtr.Zero)
                {
                    _appKitHandle = NativeLibrary.Load(AppKitPath);
                }
            }
        }

        private static IntPtr GetClass(String name)
        {
            var value = NativeMethods.objc_getClass(name);
            return value != IntPtr.Zero
                ? value
                : throw new InvalidOperationException("Required macOS class is unavailable.");
        }

        private static IntPtr GetSelector(String name)
        {
            var value = NativeMethods.sel_registerName(name);
            return value != IntPtr.Zero
                ? value
                : throw new InvalidOperationException("Required macOS selector is unavailable.");
        }

        private static String? ReadString(IntPtr nsString)
        {
            if (nsString == IntPtr.Zero)
            {
                return null;
            }

            var utf8 = Send(nsString, GetSelector("UTF8String"));
            return utf8 == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(utf8);
        }

        private static IntPtr Send(IntPtr receiver, IntPtr selector) =>
            NativeMethods.objc_msgSend(receiver, selector);

        private static class NativeMethods
        {
            [DllImport(ObjectiveCLibrary)]
            internal static extern IntPtr objc_getClass(String name);

            [DllImport(ObjectiveCLibrary)]
            internal static extern IntPtr sel_registerName(String name);

            [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
            internal static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);
        }
    }
}
