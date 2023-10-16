using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using RawInputDataSinkServer.Communication;
using RawInputDataSinkServer.InputRaw;

namespace RawInputDataSinkServer
{
    internal class Program
    {
        private const int TargetPort = 5973;

        private static UdpBroadcaster? s_broadcaster;
        private static readonly Thread s_sendQueueMonitor = new(MonitorSendQueue) { IsBackground = true };
        private static readonly ConcurrentQueue<InputManager.KeyboardEvent> s_eventsToSend = new();
        private static readonly Dictionary<long, HashSet<Enumerations.KeyboardScanCode>> s_keysCurrentlyDown = new();
        private static bool s_shouldClose = false;
        public static void Main(string[] args)
        {
            var addresses = FetchLocalIpv4Addresses();
            while (addresses.Count == 0)
            {
                Thread.Sleep(1000);
                addresses = FetchLocalIpv4Addresses();
            }

            var localAddress = SelectServerAddress(addresses, args.Length > 1 ? args[1] : string.Empty);
            Console.WriteLine($"Local address: {localAddress}");
            s_broadcaster = new(localAddress, TargetPort);

            s_sendQueueMonitor.Start();

            var inputWindow = new InputWindow(s_eventsToSend);
            if (!InputManager.TryRegisterWindowForKeyboardInput(inputWindow.WindowHandle, out var errorMessage))
                throw new InvalidOperationException(errorMessage);

            Console.WriteLine($"Input sink setup, listening...");
            while (WinApiWrapper.GetMessage(out var msg, IntPtr.Zero, 0, 0))
            {
                WinApiWrapper.TranslateMessage(msg);
                WinApiWrapper.DispatchMessage(msg);
            }
            s_shouldClose = true;
            s_broadcaster.Dispose();
        }

        private static void MonitorSendQueue(object? obj)
        {
            while (!s_shouldClose)
            {
                if (s_eventsToSend.TryDequeue(out var newEvent))
                {
                    if (newEvent.Event == InputManager.KeyEvent.KeyDown)
                    {
                        if (s_keysCurrentlyDown.TryGetValue(newEvent.SourceId, out var keysHeldDown))
                        {
                            if (keysHeldDown.Contains(newEvent.Key))
                                continue;
                        }
                        else
                        {
                            s_keysCurrentlyDown.Add(newEvent.SourceId, new());
                            keysHeldDown = s_keysCurrentlyDown[newEvent.SourceId];
                        }
                        keysHeldDown.Add(newEvent.Key);
                    }
                    else
                    {
                        if (s_keysCurrentlyDown.TryGetValue(newEvent.SourceId, out var keysHeldDown) && keysHeldDown.Contains(newEvent.Key))
                            keysHeldDown.Remove(newEvent.Key);
                    }

                    var msg = new byte[11];
                    msg[0] = (byte)newEvent.Event;
                    for (var i = 0; i < 8; i++)
                        msg[i + 1] = (byte)((newEvent.SourceId >> (8 * i)) & 0xff);
                    msg[9] = (byte)((ushort)newEvent.Key & 0xff);
                    msg[10] = (byte)(((ushort)newEvent.Key >> 8) & 0xff);

                    if (!s_broadcaster!.TrySendMessage(msg))
                        Console.WriteLine("Failed to send message");
                }
            }
        }

        private static ReadOnlyCollection<IPAddress> FetchLocalIpv4Addresses()
        {
            List<IPAddress> localAddresses = new();
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    localAddresses.Add(ip);
            }
            return new ReadOnlyCollection<IPAddress>(localAddresses);
        }

        private static IPAddress SelectServerAddress(ReadOnlyCollection<IPAddress> addresses, string preferredAddressFilter)
        {
            if (string.IsNullOrEmpty(preferredAddressFilter))
                return addresses.First();

            var possibleAddresses = new List<IPAddress>();
            var preferredAddresseRegex = new Regex(preferredAddressFilter.Replace(".", @"\.").Replace("*", @"[0-9]{1,3}"));
            foreach (var address in addresses)
            {
                if (preferredAddresseRegex.IsMatch(address.ToString()))
                    possibleAddresses.Add(address);
            }
            return possibleAddresses.Count > 0 ? possibleAddresses.First() : addresses.First();

        }
    }
}
