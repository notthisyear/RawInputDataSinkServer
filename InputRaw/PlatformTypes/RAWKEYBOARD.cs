using System.Runtime.InteropServices;

namespace RawInputDataSinkServer.InputRaw
{
#pragma warning disable IDE1006 // Naming Styles
    [StructLayout(LayoutKind.Sequential)]
    internal struct RAWKEYBOARD
    {
        /// <summary>
        /// Specifies the scan code associated with a key press.
        /// </summary>
        public ushort MakeCode;
        /// <summary>
        /// Flags for scan code information. See <seealso cref="Enumerations.ScanCodeFlag"/> for possible values.
        /// </summary>
        public ushort Flags;
        /// <summary>
        /// Reserved; must be zero.
        /// </summary>
        public ushort Reserved;
        /// <summary>
        /// The corresponding <see href="https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes">legacy virtual-key code</see>.
        /// </summary>
        public ushort VKey;
        /// <summary>
        /// The corresponding <see href="https://learn.microsoft.com/en-us/windows/win32/inputdev/keyboard-input-notifications">legacy keyboard window message</see>, for example WM_KEYDOWN, WM_SYSKEYDOWN, and so forth.
        /// </summary>
        public uint Message;
        /// <summary>
        /// The device-specific additional information for the event.
        /// </summary>
        public uint ExtraInformation;
    }
#pragma warning restore IDE1006 // Naming Styles
}
