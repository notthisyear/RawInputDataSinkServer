using System;
using System.Runtime.InteropServices;

namespace RawInputDataSinkServer.InputRaw
{
#pragma warning disable IDE1006 // Naming Styles
    [StructLayout(LayoutKind.Sequential)]
    internal struct RAWINPUTDEVICELIST
    {
        /// <summary>
        /// A handle to the raw input device.
        /// </summary>
        public IntPtr Device;

        /// <summary>
        /// The type of device. See <seealso cref="Enumerations.RawInputDeviceType"/> for possible values.
        /// </summary>
        public uint Type;
    }
#pragma warning restore IDE1006 // Naming Styles
}
