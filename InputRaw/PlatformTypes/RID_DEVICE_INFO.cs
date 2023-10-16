using System.Runtime.InteropServices;

namespace RawInputDataSinkServer.InputRaw
{
#pragma warning disable IDE1006 // Naming Styles
    [StructLayout(LayoutKind.Explicit)]
    internal struct RID_DEVICE_INFO
    {
        /// <summary>
        /// The size, in bytes, of the RID_DEVICE_INFO structure.
        /// </summary>
        [FieldOffset(0)]
        public uint Size;

        /// <summary>
        /// The type of raw input data. See <seealso cref="Enumerations.RawInputDeviceType"/> for possible values.
        /// </summary>
        [FieldOffset(4)]
        public uint Type;

        /// <summary>
        /// If Type is  <seealso cref="Enumerations.RawInputDeviceType.RIM_TYPEMOUSE"/>, this is the <see cref="RID_DEVICE_INFO_MOUSE"/> structure that defines the mouse.
        /// </summary>
        [FieldOffset(8)]
        public RID_DEVICE_INFO_MOUSE mouse;

        /// <summary>
        /// If Type is <seealso cref="Enumerations.RawInputDeviceType.RIM_TYPEKEYBOARD"/>, this is the <see cref="RID_DEVICE_INFO_KEYBOARD"/> structure that defines the keyboard.
        /// </summary>
        [FieldOffset(8)]
        public RID_DEVICE_INFO_KEYBOARD keyboard;

        /// <summary>
        /// If Type is <seealso cref="Enumerations.RawInputDeviceType.RIM_TYPEHID"/>, this is the <see cref="RID_DEVICE_INFO_HID"/> structure that defines the HID device.
        /// </summary>
        [FieldOffset(8)]
        public RID_DEVICE_INFO_HID hid;
    }
#pragma warning restore IDE1006 // Naming Styles
}
