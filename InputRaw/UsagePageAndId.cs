using static RawInputDataSinkServer.InputRaw.Enumerations;

namespace RawInputDataSinkServer.InputRaw
{
    internal abstract record UsagePageAndIdBase
    {
        public abstract HidUsagePage UsagePage { get; }

        public abstract ushort UsageId { get; }

        public static UsagePageAndIdBase GetGenericDesktopControlUsagePageAndFlag(HidGenericDesktopControls usageId)
            => new GenericDesktopControlUsagePageAndId(usageId);
    }

    internal record GenericDesktopControlUsagePageAndId : UsagePageAndIdBase
    {
        public override HidUsagePage UsagePage => HidUsagePage.GenericDesktopControls;

        public override ushort UsageId => (ushort)UsageIdType;

        public HidGenericDesktopControls UsageIdType { get; }

        public GenericDesktopControlUsagePageAndId(HidGenericDesktopControls usageIdType)
        {
            UsageIdType = usageIdType;
        }
    }
}
