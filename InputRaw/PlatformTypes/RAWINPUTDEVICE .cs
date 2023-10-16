using System;
using System.Runtime.InteropServices;

namespace RawInputDataSinkServer.InputRaw
{
#pragma warning disable IDE1006 // Naming Styles
    [StructLayout(LayoutKind.Sequential)]
    internal struct RAWINPUTDEVICE
    {
        /// <summary>
        /// Top level collection Usage page for the raw input device.
        /// </summary>
        public ushort UsagePage;
        /// <summary>
        /// Top level collection Usage for the raw input device.
        /// </summary>
        public ushort Usage;
        /// <summary>
        /// Mode flag that specifies how to interpret the information provided by UsagePage and Usage. See <seealso cref="Enumerations.DeviceRegistrationModeFlag"/> for details.
        /// </summary>
        public uint Flags;
        /// <summary>
        /// Handle to the target device. If NULL, it follows the keyboard focus.
        /// </summary>
        public IntPtr WindowHandle;
    }
#pragma warning restore IDE1006 // Naming Styles
}
