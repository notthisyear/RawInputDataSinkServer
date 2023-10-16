using System.Runtime.InteropServices;

namespace RawInputDataSinkServer.InputRaw
{
#pragma warning disable IDE1006 // Naming Styles
    [StructLayout(LayoutKind.Sequential)]
    internal struct RAWMOUSE
    {
        /// <summary>
        /// The mouse state. See <seealso cref="Enumerations.MouseStateFlag"/> for possible values.
        /// </summary>
        public ushort Flags;

        [StructLayout(LayoutKind.Explicit)]
        public struct ButtonFlagsAndData
        {
            /// <summary>
            /// Reserved.
            /// </summary>
            [FieldOffset(0)]
            public uint Buttons;
            /// <summary>
            /// The transition state of the mouse buttons. See <seealso cref="Enumerations.MouseButtonFlag"/> for possible values.
            /// </summary>
            [FieldOffset(0)]
            public ushort ButtonFlags;
            /// <summary>
            /// If ButtonFlags has RI_MOUSE_WHEEL or RI_MOUSE_HWHEEL, this member specifies the distance the wheel is rotated.
            /// </summary>
            [FieldOffset(2)]
            public ushort ButtonData;
        }

        public ButtonFlagsAndData Data;

        /// <summary>
        /// The raw state of the mouse buttons. The Win32 subsystem does not use this member.
        /// </summary>
        public uint RawButtons;
        /// <summary>
        /// The motion in the X direction. This is signed relative motion or absolute motion, depending on the value of Flags.
        /// </summary>
        public int LastX;
        /// <summary>
        /// The motion in the Y direction. This is signed relative motion or absolute motion, depending on the value of Flags.
        /// </summary>
        public int LastY;
        /// <summary>
        /// The device-specific additional information for the event.
        /// </summary>
        public uint ExtraInformation;
    }
#pragma warning restore IDE1006 // Naming Styles
}
