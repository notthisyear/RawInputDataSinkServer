using System.Runtime.InteropServices;

namespace RawInputDataSinkServer.InputRaw
{
#pragma warning disable IDE1006 // Naming Styles
    [StructLayout(LayoutKind.Sequential)]
    internal struct RID_DEVICE_INFO_HID
    {
        /// <summary>
        /// The vendor identifier for the HID.
        /// </summary>
        public uint VendorId;

        /// <summary>
        /// The product identifier for the HID.
        /// </summary>
        public uint ProductId;

        /// <summary>
        /// The version number for the HID.
        /// </summary>
        public uint VersionNumber;

        /// <summary>
        /// The top-level collection Usage Page for the device.
        /// </summary>
        public ushort UsagePage;

        /// <summary>
        /// The top-level collection Usage for the device.
        /// </summary>
        public ushort Usage;
    }
#pragma warning restore IDE1006 // Naming Styles
}
