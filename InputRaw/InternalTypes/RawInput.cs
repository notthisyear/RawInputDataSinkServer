using System;
using static RawInputDataSinkServer.InputRaw.Enumerations;

namespace RawInputDataSinkServer.InputRaw.InternalTypes
{
    internal abstract record RawInputBase
    {
        public RawInputHeader Header { get; }

        protected RawInputBase(RawInputHeader header)
        {
            Header = header;
        }
    }

    internal record RawKeyboardInput : RawInputBase
    {
        public KeyboardScanCode ScanCode { get; }

        public bool IsKeyDown { get; }

        public bool IsKeyUp { get; }

        public RawKeyboardInput(RawInputHeader header, RAWKEYBOARD rawKeyboard) : base(header)
        {
            ScanCode = Enum.IsDefined(typeof(KeyboardScanCode), rawKeyboard.MakeCode)
                ? (KeyboardScanCode)rawKeyboard.MakeCode : KeyboardScanCode.UnknownScanCode;

            IsKeyDown = (rawKeyboard.Flags & (ushort)ScanCodeFlag.RI_KEY_MAKE) == (ushort)ScanCodeFlag.RI_KEY_MAKE;
            IsKeyUp = (rawKeyboard.Flags & (ushort)ScanCodeFlag.RI_KEY_BREAK) == (ushort)ScanCodeFlag.RI_KEY_BREAK;
        }
    }
}
