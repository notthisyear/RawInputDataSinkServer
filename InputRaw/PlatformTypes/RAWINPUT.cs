using RawInputDataSinkServer.InputRaw;
using System.Runtime.InteropServices;

namespace RawInputDataSinkServer.InputRaw
{
#pragma warning disable IDE1006 // Naming Styles
    [StructLayout(LayoutKind.Explicit)]
    internal struct RAWINPUT
    {
        /// <summary>
        /// The raw input data.
        /// </summary>
        [FieldOffset(0)]
        RAWINPUTHEADER Header;

        /// <summary>
        /// If the data comes from a mouse, this is the raw input data.
        /// </summary>
        [FieldOffset(16)]
        public RAWMOUSE Mouse;

        /// <summary>
        /// If the data comes from a keyboard, this is the raw input data.
        /// </summary>
        [FieldOffset(16)]
        public RAWKEYBOARD Keyboard;

        /// <summary>
        /// If the data comes from an HID, this is the raw input data
        /// </summary>
        [FieldOffset(16)]
        public RAWHID Hid;
    }
#pragma warning restore IDE1006 // Naming Styles
}
