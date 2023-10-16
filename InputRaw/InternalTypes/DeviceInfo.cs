using System;
using System.Collections.Generic;

namespace RawInputDataSinkServer.InputRaw
{
    internal abstract record DeviceInfoBase(InputDevice Device, string DeviceDescription);

    internal record KeyboardDeviceInfo : DeviceInfoBase
    {
        public Enumerations.KeyboardType KeyboardType { get; init; }

        public uint VendorSubType { get; init; }

        public uint ScanCodeMode { get; init; }

        public uint NumberOfFunctionKeys { get; init; }

        public uint NumberOfIndicators { get; init; }

        public uint NumberOfKeysTotal { get; init; }

        public KeyboardDeviceInfo(InputDevice device, string deviceDescription, uint type) : base(device, deviceDescription)
        {
            if (Enum.IsDefined(typeof(Enumerations.KeyboardType), type))
                KeyboardType = (Enumerations.KeyboardType)type;
        }
    }

    internal record MouseDeviceInfo : DeviceInfoBase
    {
        public List<Enumerations.MouseDeviceIdentificationFlag> DeviceIdentification { get; init; }

        public uint NumberOfButtons { get; init; }

        public uint SampleRate { get; init; }

        public bool HasHorizontalWheel { get; init; }

        internal MouseDeviceInfo(InputDevice device, string deviceDescription, uint rawIdentificationBitfield) : base(device, deviceDescription)
        {
            DeviceIdentification = new();
            foreach (var flag in Enum.GetValues<Enumerations.MouseDeviceIdentificationFlag>())
            {
                if ((rawIdentificationBitfield & (uint)flag) == (uint)flag)
                    DeviceIdentification.Add(flag);
            }
        }
    }

    internal record HidDeviceInfo : DeviceInfoBase
    {
        public uint VendorId { get; init; }

        public uint ProductId { get; init; }

        public uint VersionNumber { get; init; }

        public ushort UsagePage { get; init; }

        public ushort Usage { get; init; }

        public HidDeviceInfo(InputDevice device, string deviceDescription) : base(device, deviceDescription)
        { }
    }
}
