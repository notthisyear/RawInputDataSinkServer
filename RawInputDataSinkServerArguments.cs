using CommandLine;

namespace RawInputDataSinkServer
{
    internal class RawInputDataSinkServerArguments
    {

        [Option(longName: "local-network-interface-preference", Required = false, Default = "*.*.*.*", HelpText = "The preferred local network interface (specified as an IPv4 address with * as wildcard)")]
        public string? LocalNetworkInterfacePreference { get; set; }

        [Option(shortName: 'p', longName: "broadcast-port", Required = false, Default = 5973, HelpText = "The preferred local network inteface (specified as an IPv4 address with * as wildcard)")]
        public int UdpBroadcastPort { get; set; }

        [Option(shortName: 'v', longName: "verbose", Required = false, Default = false, HelpText = "Make program more verbose")]
        public bool Verbose { get; set; }

        [Option(longName: "print-licenses", Required = false, Default = false, HelpText = "Prints licences for third-party software in the program")]
        public bool PrintThirdPartyLicenses { get; set; }
    }
}
