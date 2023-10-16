using System;
using System.Runtime.InteropServices;

namespace RawInputDataSinkServer.InputRaw
{
#pragma warning disable IDE1006 // Naming Styles
    [StructLayout(LayoutKind.Sequential)]
    internal struct RAWINPUTHEADER
    {
        /// <summary>
        /// The type of raw input. See <seealso cref="Enumerations.RawInputDeviceType"/> for possible values.
        /// </summary>
        public uint Type;
        /// <summary>
        /// The size, in bytes, of the entire input packet of data. This includes <see cref="RAWINPUT"/> plus possible extra input reports in the RAWHID variable length array.
        /// </summary>
        public uint Size;
        /// <summary>
        /// A handle to the device generating the raw input data.
        /// </summary>
        public IntPtr DeviceHandle;
        /// <summary>
        /// The value passed in the wParam parameter of the WM_INPUT message.
        /// </summary>
        public IntPtr WParam;
    }
#pragma warning restore IDE1006 // Naming Styles
}
