using System;
using System.Runtime.InteropServices;
using System.Security;

namespace RawInputDataSinkServer
{
    [SuppressUnmanagedCodeSecurity]
    internal static partial class User32Interops
    {
        private const string User32DllName = "user32.dll";

        [DllImport(User32DllName)]
        public static extern bool GetMessage(out IntPtr lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);


        [LibraryImport(User32DllName, EntryPoint = "TranslateMessage", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool TranslateMessage(in IntPtr lpMsg);

        [DllImport(User32DllName)]
        public static extern IntPtr DispatchMessage(in IntPtr lpMsg);


        [LibraryImport(User32DllName, EntryPoint = "RegisterClassW", SetLastError = true)]
        public static partial ushort RegisterClassW(IntPtr lpWndClass);

        [LibraryImport(User32DllName, EntryPoint = "CreateWindowExW", SetLastError = true)]
        public static partial IntPtr CreateWindowExW(uint dwExStyle,
            [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
            uint dwStyle, int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [LibraryImport(User32DllName, EntryPoint = "DefWindowProcA", SetLastError = true)]
        public static partial IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
    }
}
