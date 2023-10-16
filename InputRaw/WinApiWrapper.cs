using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using RawInputDataSinkServer.InputRaw.InternalTypes;
using Microsoft.Win32;
using static RawInputDataSinkServer.InputRaw.Enumerations;
using System.Runtime.Versioning;

namespace RawInputDataSinkServer.InputRaw
{
    [SuppressUnmanagedCodeSecurity]
    internal partial class WinApiWrapper
    {
        public const int WM_INPUT = 0x00FF;

        private const string UserDllName = "user32.dll";

        #region Public methods
        public static bool TryGetAllInputDevices(out List<InputDevice> devices, out string errorMessage)
        {
            devices = new List<InputDevice>();
            uint numberOfDevices = 0;
            var deviceListSize = (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICELIST));
            _ = GetRawInputDeviceList(IntPtr.Zero, ref numberOfDevices, deviceListSize);

            if (LastPInvokeWasError(out errorMessage))
                return false;

            var buffer = Marshal.AllocHGlobal((int)numberOfDevices * (int)deviceListSize);
            _ = GetRawInputDeviceList(buffer, ref numberOfDevices, deviceListSize);

            if (LastPInvokeWasError(out errorMessage))
            {
                Marshal.FreeHGlobal(buffer);
                return false;
            }

            for (var i = 0; i < numberOfDevices; i++)
            {
                var device = Marshal.PtrToStructure<RAWINPUTDEVICELIST>(IntPtr.Add(buffer, (int)(deviceListSize * i)));
                if (!Enum.IsDefined(typeof(Enumerations.RawInputDeviceType), device.Type))
                {
                    devices.Clear();
                    Marshal.FreeHGlobal(buffer);
                    errorMessage = $"Unknown device type {device.Type}";
                    return false;
                }
                devices.Add(new(device.Device, (Enumerations.RawInputDeviceType)device.Type));
            }
            Marshal.FreeHGlobal(buffer);
            return true;
        }

        [SupportedOSPlatform("windows")]
        public static bool TryGetDeviceInfoForDevice(InputDevice device, out DeviceInfoBase? deviceInformation, out string errorMessage)
        {
            deviceInformation = default;
            if (!TryGetDeviceName(device, out var deviceName, out errorMessage))
            {
                return false;
            }
            if (!TryGetDeviceInfo(device, out var deviceInfo, out errorMessage))
                return false;

            if (!TryGetDeviceDescriptionFromRegistry(deviceName, out var deviceDescription, out errorMessage))
                return false;

            deviceInformation = device.Type switch
            {
                Enumerations.RawInputDeviceType.RIM_TYPEMOUSE => new MouseDeviceInfo(device, deviceDescription, deviceInfo.mouse.Id)
                {
                    NumberOfButtons = deviceInfo.mouse.NumberOfButtons,
                    SampleRate = deviceInfo.mouse.SampleRate,
                    HasHorizontalWheel = deviceInfo.mouse.HasHorizontalWheel == 0x0001
                },
                Enumerations.RawInputDeviceType.RIM_TYPEKEYBOARD => new KeyboardDeviceInfo(device, deviceDescription, deviceInfo.keyboard.Type)
                {
                    VendorSubType = deviceInfo.keyboard.SubType,
                    ScanCodeMode = deviceInfo.keyboard.KeyboardMode,
                    NumberOfFunctionKeys = deviceInfo.keyboard.NumberOfFunctionKeys,
                    NumberOfIndicators = deviceInfo.keyboard.NumberOfIndicators,
                    NumberOfKeysTotal = deviceInfo.keyboard.NumberOfKeysTotal,
                },
                Enumerations.RawInputDeviceType.RIM_TYPEHID => new HidDeviceInfo(device, deviceDescription)
                {
                    VendorId = deviceInfo.hid.VendorId,
                    ProductId = deviceInfo.hid.ProductId,
                    VersionNumber = deviceInfo.hid.VersionNumber,
                    UsagePage = deviceInfo.hid.UsagePage,
                    Usage = deviceInfo.hid.Usage
                },
                _ => throw new NotImplementedException(),
            }; ;
            return true;
        }

        public static bool TryRegisterInputDevice(IntPtr windowHandle, UsagePageAndIdBase usagePageAndId, out string errorMessage, params DeviceRegistrationModeFlag[] flags)
        {
            ushort flagsValue = 0x0000;
            foreach (var flag in flags)
                flagsValue |= (ushort)flag;

            var device = new RAWINPUTDEVICE()
            {
                UsagePage = (ushort)usagePageAndId.UsagePage,
                Usage = usagePageAndId.UsageId,
                Flags = flagsValue,
                WindowHandle = windowHandle
            };

            return TryRegisterDevices(out errorMessage, device);
        }

        public static bool TryGetRawInput(IntPtr rawInputHandle, out RawInputBase? input, out string errorMessage)
        {
            input = default;
            if (!TryGetHeader(rawInputHandle, out var rawHeader, out var rawHeaderSize, out errorMessage))
                return false;

            if (!Enum.IsDefined(typeof(Enumerations.RawInputDeviceType), rawHeader.Type))
            {
                errorMessage = $"Unknown device type {rawHeader.Type}";
                return false;
            }

            return TryGetData(rawInputHandle,
                new RawInputHeader((Enumerations.RawInputDeviceType)rawHeader.Type, rawHeader.DeviceHandle),
                rawHeaderSize,
                rawHeader.Size,
                out input,
                out errorMessage);
        }
        #endregion

        #region Private methods
        [SupportedOSPlatform("windows")]
        private static bool TryGetDeviceDescriptionFromRegistry(string deviceName, out string deviceDescription, out string errorMessage)
        {
            deviceDescription = string.Empty;
            errorMessage = string.Empty;

            var deviceParts = deviceName[4..].Split('#');
            var classCode = deviceParts[0];       // ACPI (Class code)
            var subClassCode = deviceParts[1];    // PNP0303 (SubClass code)
            var protocolCode = deviceParts[2];    // 3&13c0b0c5&0 (Protocol code)

            var key = Registry.LocalMachine.OpenSubKey($"System\\CurrentControlSet\\Enum\\{classCode}\\{subClassCode}\\{protocolCode}");
            if (key != default)
            {
                deviceDescription = key.GetValue("DeviceDesc")?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(deviceDescription) && deviceDescription.Contains(';'))
                    deviceDescription = deviceDescription[(deviceDescription.IndexOf(";") + 1)..];
                else
                    errorMessage = "DeviceDesc either missing or has unexpected format";
            }
            else
            {
                errorMessage = "Could not find matching key in registry";
            }

            return string.IsNullOrEmpty(errorMessage);
        }

        private static bool TryGetDeviceName(InputDevice device, out string deviceName, out string errorMessage)
        {
            deviceName = string.Empty;
            uint size = 0;
            _ = GetRawInputDeviceInfo(device.DeviceId, (uint)Enumerations.GetDeviceInfoCommand.RIDI_DEVICENAME, IntPtr.Zero, ref size);

            if (LastPInvokeWasError(out errorMessage))
                return false;

            var nameBuffer = Marshal.AllocHGlobal((int)size);
            _ = GetRawInputDeviceInfo(device.DeviceId, (uint)Enumerations.GetDeviceInfoCommand.RIDI_DEVICENAME, nameBuffer, ref size);

            if (LastPInvokeWasError(out errorMessage))
            {
                Marshal.FreeHGlobal(nameBuffer);
                return false;
            }

            deviceName = Marshal.PtrToStringAnsi(nameBuffer) ?? string.Empty;
            Marshal.FreeHGlobal(nameBuffer);

            if (string.IsNullOrEmpty(deviceName))
                errorMessage = "Marshal.PtrToStringAnsi() failed";
            return string.IsNullOrEmpty(errorMessage);
        }

        private static bool TryGetDeviceInfo(InputDevice device, out RID_DEVICE_INFO deviceInfo, out string errorMessage)
        {
            var deviceInfoSize = (uint)Marshal.SizeOf(typeof(RID_DEVICE_INFO));
            deviceInfo = new RID_DEVICE_INFO() { Size = deviceInfoSize };

            var deviceInfoBuffer = Marshal.AllocHGlobal((int)deviceInfoSize);
            Marshal.StructureToPtr(deviceInfo, deviceInfoBuffer, false);

            _ = GetRawInputDeviceInfo(device.DeviceId, (uint)Enumerations.GetDeviceInfoCommand.RIDI_DEVICEINFO, deviceInfoBuffer, ref deviceInfoSize);
            if (LastPInvokeWasError(out errorMessage))
            {
                Marshal.FreeHGlobal(deviceInfoBuffer);
                return false;
            }

            deviceInfo = Marshal.PtrToStructure<RID_DEVICE_INFO>(deviceInfoBuffer);
            Marshal.FreeHGlobal(deviceInfoBuffer);
            return true;
        }

        private static bool TryRegisterDevices(out string errorMessage, params RAWINPUTDEVICE[] devices)
        {
            var numberOfDevices = (uint)devices.Length;
            var deviceSize = (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE));
            var devicesBuffer = Marshal.AllocHGlobal((int)(numberOfDevices * deviceSize));

            var ptr = new IntPtr(devicesBuffer);
            for (var i = 0; i < devices.Length; i++)
            {
                Marshal.StructureToPtr(devices[i], ptr, false);
                ptr += Marshal.SizeOf(typeof(RAWINPUTDEVICE));
            }

            _ = RegisterRawInputDevices(devicesBuffer, numberOfDevices, deviceSize);
            Marshal.FreeHGlobal(devicesBuffer);
            return !LastPInvokeWasError(out errorMessage);
        }

        private static bool TryGetHeader(IntPtr inputHandle, out RAWINPUTHEADER header, out uint rawHeaderSize, out string errorMessage)
        {
            rawHeaderSize = (uint)Marshal.SizeOf<RAWINPUTHEADER>();
            var headerBuffer = Marshal.AllocHGlobal((int)rawHeaderSize);

            _ = GetRawInputData(inputHandle, (uint)Enumerations.RawInputReadCommand.RID_HEADER, headerBuffer, ref rawHeaderSize, rawHeaderSize);
            if (LastPInvokeWasError(out errorMessage))
            {
                Marshal.FreeHGlobal(headerBuffer);
                header = default;
                return false;
            }

            header = Marshal.PtrToStructure<RAWINPUTHEADER>(headerBuffer);
            Marshal.FreeHGlobal(headerBuffer);
            return true;
        }

        private static bool TryGetData(IntPtr inputHandle, RawInputHeader header, uint rawHeaderSize, uint dataSize, out RawInputBase? rawInput, out string errorMessage)
        {
            rawInput = default;
            var dataBuffer = Marshal.AllocHGlobal((int)dataSize);
            _ = GetRawInputData(inputHandle, (uint)Enumerations.RawInputReadCommand.RID_INPUT, dataBuffer, ref dataSize, rawHeaderSize);
            if (LastPInvokeWasError(out errorMessage))
            {
                Marshal.FreeHGlobal(dataBuffer);
                return false;
            }

            switch (header.DeviceType)
            {
                case Enumerations.RawInputDeviceType.RIM_TYPEMOUSE:
                    break;

                case Enumerations.RawInputDeviceType.RIM_TYPEKEYBOARD:
                    var keyboard = Marshal.PtrToStructure<RAWKEYBOARD>(IntPtr.Add(dataBuffer, (int)rawHeaderSize));
                    rawInput = new RawKeyboardInput(header, keyboard);
                    break;

                case Enumerations.RawInputDeviceType.RIM_TYPEHID:
                    break;
                default:
                    throw new NotImplementedException();
            }
            return true;
        }

        private static bool LastPInvokeWasError(out string errorMessage)
        {
            errorMessage = string.Empty;
            var errorCode = Marshal.GetLastPInvokeError();
            if (errorCode != 0x00)
                errorMessage = $"PInvoke error {errorCode} - '{Marshal.GetLastPInvokeErrorMessage()}'";
            return !string.IsNullOrEmpty(errorMessage);
        }
        #endregion

        #region WinAPI imported methods
        /// <summary>
        /// Enumerates the raw input devices attached to the system.
        /// </summary>
        /// <param name="pRawInputDeviceList">An array of <see cref="RAWINPUTDEVICELIST"/> 
        /// structures for the devices attached to the system. 
        /// If NULL, the number of devices are returned in *puiNumDevices.</param>
        /// <param name="puiNumDevices">If pRawInputDeviceList is NULL, the function populates
        /// this variable with the number of devices attached to the system; 
        /// otherwise, this variable specifies the number of <see cref="RAWINPUTDEVICELIST"/> 
        /// structures that can be contained in the buffer to which pRawInputDeviceList points.
        /// If this value is less than the number of devices attached to the system,
        /// the function returns the actual number of devices in this variable and
        /// fails with <see cref="Win32SystemErrorCodes.ERROR_INSUFFICIENT_BUFFER"/>.
        /// If this value is greater than or equal to the number of devices attached to the system,
        /// then the value is unchanged, and the number of devices is reported as the return value.</param>
        /// <param name="cbSize">The size of a <see cref="RAWINPUTDEVICELIST"/> structure, in bytes.</param>
        /// <returns>
        /// <para>If the function is successful, the return value is the number of devices stored in the
        /// buffer pointed to by pRawInputDeviceList.</para>
        /// <para>On any other error, the function returns(UINT) -1 and GetLastError returns the error indication.</para>
        /// </returns>
        /// <remarks>
        /// <para>The devices returned from this function are the mouse, the keyboard, and other
        /// Human Interface Device (HID) devices.</para>
        /// <para>To get more detailed information about the attached devices,
        /// call GetRawInputDeviceInfo using the hDevice from <see cref="RAWINPUTDEVICELIST"/>.</para>
        /// </remarks>
        [LibraryImport(UserDllName, EntryPoint = "GetRawInputDeviceList", SetLastError = true)]
        private static partial uint GetRawInputDeviceList(IntPtr pRawInputDeviceList, ref uint puiNumDevices, uint cbSize);

        /// <summary>
        /// Retrieves information about the raw input device.
        /// </summary>
        /// <param name="hDevice">A handle to the raw input device. 
        /// This comes from the hDevice member of <see cref="RAWINPUTDEVICELIST"/> or from <see cref="GetRawInputDeviceList"/>.</param>
        /// <param name="uiCommand">Specifies what data will be returned in pData. See <seealso cref="Enumerations.GetDeviceInfoCommand"/> for valid values.</param>
        /// <param name="pData"><para>A pointer to a buffer that contains the information specified by uiCommand.</para>
        /// <para>If uiCommand is <seealso cref="Enumerations.GetDeviceInfoCommand.RIDI_DEVICEINFO"/>, set the cbSize member of <see cref="RID_DEVICE_INFO"/> to sizeof(<see cref="RID_DEVICE_INFO"/>) before calling GetRawInputDeviceInfo.</para></param>
        /// <param name="pcbSize">The size, in bytes, of the data in pData.</param>
        /// <returns><para>If successful, this function returns a non-negative number indicating the number of bytes copied to pData.</para>
        /// <para>If pData is not large enough for the data, the function returns -1. If pData is NULL, the function
        /// returns a value of zero. In both of these cases, pcbSize is set to the minimum size required for the pData buffer.</para>
        /// <para>Call GetLastError to identify any other errors</para></returns>
        [LibraryImport(UserDllName, EntryPoint = "GetRawInputDeviceInfoA", SetLastError = true)]
        private static partial uint GetRawInputDeviceInfo(IntPtr hDevice, uint uiCommand, IntPtr pData, ref uint pcbSize);

        /// <summary>
        /// Registers the devices that supply the raw input data.
        /// </summary>
        /// <param name="pRawInputDevices">An array of <see cref="RAWINPUTDEVICE "/> structures that represent the devices that supply the raw input.</param>
        /// <param name="uiNumDevices">The number of <see cref="RAWINPUTDEVICE "/> structures pointed to by pRawInputDevices.</param>
        /// <param name="cbSize">The size, in bytes, of a <see cref="RAWINPUTDEVICE "/> structure.</param>
        /// <returns>TRUE if the function succeeds; otherwise, FALSE. If the function fails, call GetLastError for more information.</returns>
        /// <remarks><para>To receive WM_INPUT messages, an application must first register the raw input devices using RegisterRawInputDevices. By default, an application does not receive raw input.</para>
        /// <para>To receive WM_INPUT_DEVICE_CHANGE messages, an application must specify the RIDEV_DEVNOTIFY flag for each device class that is specified by the usUsagePage and usUsage fields of the RAWINPUTDEVICE structure.By default, an application does not receive WM_INPUT_DEVICE_CHANGE notifications for raw input device arrival and removal.</para>
        /// <para>If a RAWINPUTDEVICE structure has the RIDEV_REMOVE flag set and the hwndTarget parameter is not set to NULL, then parameter validation will fail.</para>
        /// <para>Only one window per raw input device class may be registered to receive raw input within a process(the window passed in the last call to RegisterRawInputDevices). Because of this, RegisterRawInputDevices should not be used from a library, as it may interfere with any raw input processing logic already present in applications that load it.</para></remarks>
        [LibraryImport(UserDllName, EntryPoint = "RegisterRawInputDevices", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool RegisterRawInputDevices(IntPtr pRawInputDevices, uint uiNumDevices, uint cbSize);

        /// <summary>
        /// Retrieves the information about the raw input devices for the current application.
        /// </summary>
        /// <param name="pRawInputDevices">An array of <see cref="RAWINPUTDEVICE "/> structures for the application.</param>
        /// <param name="puiNumDevices">The number of <see cref="RAWINPUTDEVICE "/> structures in pRawInputDevices.</param>
        /// <param name="cbSize">The size, in bytes, of a <see cref="RAWINPUTDEVICE "/> structure.</param>
        /// <returns><para>If successful, the function returns a non-negative number that is the number of RAWINPUTDEVICE structures written to the buffer.</para>
        /// <para>If the pRawInputDevices buffer is too small or NULL, the function sets the last error as ERROR_INSUFFICIENT_BUFFER, returns -1, and sets puiNumDevices to the required number of devices.If the function fails for any other reason, it returns -1. For more details, call GetLastError.</para></returns>
        [LibraryImport(UserDllName, EntryPoint = "GetRegisteredRawInputDevices", SetLastError = true)]
        private static partial uint GetRegisteredRawInputDevices(IntPtr pRawInputDevices, ref uint puiNumDevices, uint cbSize);

        /// <summary>
        /// Retrieves the raw input from the specified device.
        /// </summary>
        /// <param name="hRawInput">A handle to the <see cref="RAWINPUT"/> structure. This comes from the lParam in WM_INPUT.</param>
        /// <param name="uiCommand">The command flag.<seealso cref="Enumerations.RawInputReadCommand"/> for possible values.</param>
        /// <param name="pData">A pointer to the data that comes from the <see cref="RAWINPUT"/> structure. This depends on the value of uiCommand. If pData is NULL, the required size of the buffer is returned in pcbSize.</param>
        /// <param name="pcbSize">The size, in bytes, of the data in pData.</param>
        /// <param name="cbSizeHeader">The size, in bytes, of the <see cref="RAWINPUTHEADER"/> structure.</param>
        /// <returns><para>If pData is NULL and the function is successful, the return value is 0. If pData is not NULL and the function is successful, the return value is the number of bytes copied into pData.</para>
        /// <para>If there is an error, the return value is (UINT)-1.</para></returns>
        /// <remarks>GetRawInputData gets the raw input one <see cref="RAWINPUT"/> structure at a time. In contrast, GetRawInputBuffer gets an array of <see cref="RAWINPUT"/> structures.</remarks>
        [LibraryImport(UserDllName, EntryPoint = "GetRawInputData", SetLastError = true)]
        private static partial uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        /// <summary>
        /// Performs a buffered read of the raw input messages data found in the calling thread's message queue.
        /// </summary>
        /// <param name="pData"><para>A pointer to a buffer of <see cref="RAWINPUT"/> structures that contain the raw input data. Buffer should be aligned on a pointer boundary, which is a DWORD on 32-bit architectures and a QWORD on 64-bit architectures.</para>
        /// <para>If NULL, size of the first raw input message data(minimum required buffer), in bytes, is returned in *pcbSize.</para></param>
        /// <param name="pcbSize">The size, in bytes, of the provided <see cref="RAWINPUT"/> buffer.</param>
        /// <param name="cbSizeHeader">The size, in bytes, of the <see cref="RAWINPUTHEADER"/> structure.></param>
        /// <returns><para>If pData is NULL and the function is successful, the return value is 0. If pData is not NULL and the function is successful,  the return value is the number of <see cref="RAWINPUT"/> structures written to pData.</para>
        /// <para>If there is an error, the return value is (UINT)-1. Call GetLastError for the error code.</para></returns>
        /// <remarks><para>When an application receives raw input, its message queue gets a WM_INPUT message and the queue status flag QS_RAWINPUT is set.</para>
        /// <para>Using GetRawInputBuffer, the raw input data is read in the array of variable size RAWINPUT structures and corresponding WM_INPUT messages are removed from the calling thread's message queue. You can call this method several times with buffer that cannot fit all message's data until all raw input messages have been read.</para>
        /// <para>The NEXTRAWINPUTBLOCK macro allows an application to traverse an array of RAWINPUT structures.</para>
        /// <para>If all raw input messages have been successfully read from message queue then QS_RAWINPUT flag is cleared from the calling thread's message queue status.</para></remarks>
        [LibraryImport(UserDllName, EntryPoint = "GetRawInputBuffer", SetLastError = true)]
        private static partial uint GetRawInputBuffer(IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        [DllImport(UserDllName)]
        public static extern bool GetMessage(out IntPtr lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);


        [LibraryImport(UserDllName, EntryPoint = "TranslateMessage", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool TranslateMessage(in IntPtr lpMsg);

        [DllImport(UserDllName)]
        public static extern IntPtr DispatchMessage(in IntPtr lpMsg);


        [LibraryImport(UserDllName, EntryPoint = "RegisterClassW", SetLastError = true)]
        public static partial ushort RegisterClassW(IntPtr lpWndClass);

        [LibraryImport(UserDllName, EntryPoint = "CreateWindowExW", SetLastError = true)]
        public static partial IntPtr CreateWindowExW(uint dwExStyle,
            [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
            uint dwStyle, int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [LibraryImport(UserDllName, EntryPoint = "DefWindowProcA", SetLastError = true)]
        public static partial IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        #endregion
    }
}
