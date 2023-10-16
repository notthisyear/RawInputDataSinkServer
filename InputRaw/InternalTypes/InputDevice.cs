using System;

namespace RawInputDataSinkServer.InputRaw
{
    internal record InputDevice(IntPtr DeviceId, Enumerations.RawInputDeviceType Type);
}
