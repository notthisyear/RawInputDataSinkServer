using System.Runtime.InteropServices;

namespace RawInputDataSinkServer.InputRaw
{
#pragma warning disable IDE1006 // Naming Styles
    [StructLayout(LayoutKind.Sequential)]
    internal struct RID_DEVICE_INFO_MOUSE
    {
        /// <summary>
        /// The bitfield of the mouse device identification properties. See <seealso cref="Enumerations.MouseDeviceIdentificationFlag"/> for known values.
        /// </summary>
        public uint Id;

        /// <summary>
        /// The number of buttons for the mouse.
        /// </summary>
        public uint NumberOfButtons;

        /// <summary>
        /// The number of data points per second. This information may not be applicable for every mouse device.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// TRUE if the mouse has a wheel for horizontal scrolling; otherwise, FALSE.
        /// </summary>
        public uint HasHorizontalWheel;
    }

#pragma warning restore IDE1006 // Naming Styles
}
