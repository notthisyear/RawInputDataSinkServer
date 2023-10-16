using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using static RawInputDataSinkServer.InputRaw.Enumerations;

namespace RawInputDataSinkServer.InputRaw
{
    internal class InputManager
    {
        public enum KeyEvent
        {
            KeyUp,
            KeyDown
        };

        public record KeyboardEvent(KeyEvent Event, long SourceId, KeyboardScanCode Key);

        #region Private fields
        private readonly Dictionary<long, KeyboardDeviceInfo> _knownKeyboards;
        #endregion

        public InputManager()
        {
            _knownKeyboards = new();
        }

        #region Public methods
        [SupportedOSPlatform("Windows")]
        public bool TryEnumerateKeyboardDevices(out string errorMessage)
        {
            WinApiWrapper.TryGetAllInputDevices(out var devices, out errorMessage);

            if (!string.IsNullOrEmpty(errorMessage))
                return false;

            var keyboards = devices.Where(x => x.Type == Enumerations.RawInputDeviceType.RIM_TYPEKEYBOARD);
            List<DeviceInfoBase> deviceInfo = new();
            foreach (var keyboard in keyboards)
            {
                if (WinApiWrapper.TryGetDeviceInfoForDevice(keyboard, out var info, out errorMessage) && info != default)
                    deviceInfo.Add(info);
                else
                    break;
            }

            if (string.IsNullOrEmpty(errorMessage) && deviceInfo.Any())
            {
                _knownKeyboards.Clear();
                foreach (var device in deviceInfo)
                    _knownKeyboards.Add(device.Device.DeviceId, (KeyboardDeviceInfo)device);
            }

            return string.IsNullOrEmpty(errorMessage);
        }

        public static bool TryRegisterWindowForKeyboardInput(IntPtr windowHandle, out string errorMessage)
        {
            return WinApiWrapper.TryRegisterInputDevice(windowHandle,
                UsagePageAndIdBase.GetGenericDesktopControlUsagePageAndFlag(HidGenericDesktopControls.HID_USAGE_GENERIC_KEYBOARD),
                out errorMessage,
                DeviceRegistrationModeFlag.RIDEV_NOLEGACY, DeviceRegistrationModeFlag.RIDEV_INPUTSINK);
        }

        public bool TryGetInformationForKeyboard(long keyboardId, out KeyboardDeviceInfo? deviceInfo)
            => _knownKeyboards.TryGetValue(keyboardId, out deviceInfo);
        #endregion
    }
}
