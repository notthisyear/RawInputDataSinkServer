using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using RawInputDataSinkServer.InputRaw;
using RawInputDataSinkServer.InputRaw.InternalTypes;
using static RawInputDataSinkServer.InputRaw.InputManager;

namespace RawInputDataSinkServer
{
    internal class InputWindow
    {
        public IntPtr WindowHandle { get; }

        private const string ClassName = "RawInputDataSinkServerInputWindow";
        private readonly WNDCLASS _wndClass;
        private readonly ConcurrentQueue<KeyboardEvent> _eventsToSend;

        public InputWindow(ConcurrentQueue<KeyboardEvent> eventsToSend)
        {
            _eventsToSend = eventsToSend;
            _wndClass = new WNDCLASS()
            {
                ClassName = Marshal.StringToHGlobalUni(ClassName),
                WndProc = (hWnd, msg, wParam, lParam) =>
                {
                    if (msg == WinApiWrapper.WM_INPUT &&
                        WinApiWrapper.TryGetRawInput(lParam, out var input, out _) &&
                        input is RawKeyboardInput keyboardInput)
                    {
                        _eventsToSend.Enqueue(new(keyboardInput.IsKeyUp ? KeyEvent.KeyUp : KeyEvent.KeyDown,
                        keyboardInput.Header.DeviceHandle,
                        keyboardInput.ScanCode));
                    }
                    return WinApiWrapper.DefWindowProc(hWnd, msg, wParam, lParam);
                }
            };

            var size = Marshal.SizeOf(typeof(WNDCLASS));
            var buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(_wndClass, buffer, false);

            if (WinApiWrapper.RegisterClassW(buffer) == 0)
                throw new Win32Exception();

            Marshal.FreeHGlobal(buffer);
            WindowHandle = WinApiWrapper.CreateWindowExW(
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
