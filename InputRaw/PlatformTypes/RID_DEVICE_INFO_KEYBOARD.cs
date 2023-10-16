using System.Runtime.InteropServices;

namespace RawInputDataSinkServer.InputRaw
{
#pragma warning disable IDE1006 // Naming Styles
    [StructLayout(LayoutKind.Sequential)]
    internal struct RID_DEVICE_INFO_KEYBOARD
    {
        /// <summary>
        /// The type of keyboard. See <seealso cref="Enumerations.KeyboardType"/> for known values.
        /// </summary>
        public uint Type;

        /// <summary>
        /// The vendor-specific subtype of the keyboard.
        /// </summary>
        public uint SubType;

        /// <summary>
        /// The scan code mode. Usually 1, which means that Scan Code Set 1 is used. See <see href="https://download.microsoft.com/download/1/6/1/161ba512-40e2-4cc9-843a-923143f3456c/scancode.doc">Keyboard Scan Code Specification</see>.
        /// </summary>
        public uint KeyboardMode;

        /// <summary>
        /// The number of function keys on the keyboard.
        /// </summary>
        public uint NumberOfFunctionKeys;

        /// <summary>
        /// The number of LED indicators on the keyboard.
        /// </summary>
        public uint NumberOfIndicators;

        /// <summary>
        /// The total number of keys on the keyboard.
        /// </summary>
        public uint NumberOfKeysTotal;
    }
#pragma warning restore IDE1006 // Naming Styles
}
