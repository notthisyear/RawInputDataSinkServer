using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using WindowsNativeRawInputWrapper;
using WindowsNativeRawInputWrapper.Types;

namespace RawInputDataSinkServer
{
    internal class InputWindow
    {
        public IntPtr WindowHandle { get; }

        private const string ClassName = "RawInputDataSinkServerInputWindow";
        private readonly WNDCLASS _wndClass;
        private readonly ConcurrentQueue<RawKeyboardInput> _eventsToSend;
        private readonly WndProc _wndProc;
        public InputWindow(ConcurrentQueue<RawKeyboardInput> eventsToSend)
        {
            _eventsToSend = eventsToSend;
            _wndProc = (hWnd, msg, wParam, lParam) =>
            {
                if (WinApiWrapper.IsInputMessage(msg) &&
                    WinApiWrapper.TryGetRawInput(lParam, out var input, out _) &&
                    input is RawKeyboardInput keyboardInput)
                {
                    _eventsToSend.Enqueue(keyboardInput);
                }
                return User32Interops.DefWindowProc(hWnd, msg, wParam, lParam);
            };
            _wndClass = new WNDCLASS()
            {
                ClassName = Marshal.StringToHGlobalUni(ClassName),
                WndProc = _wndProc
            };

            var size = Marshal.SizeOf(typeof(WNDCLASS));
            var buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(_wndClass, buffer, false);

            if (User32Interops.RegisterClassW(buffer) == 0)
                throw new Win32Exception();

            Marshal.FreeHGlobal(buffer);
            WindowHandle = User32Interops.CreateWindowExW(
                0x08000000, // WS_EX_NOACTIVATE
                ClassName,
                "",
                0x80000000, // WS_POPUP
                0, 0, 0, 0,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero
            );

            if (WindowHandle == IntPtr.Zero)
                throw new Win32Exception();
        }
    }
}
