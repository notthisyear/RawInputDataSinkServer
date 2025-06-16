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
using WindowsNativeRawInputWrapper;
using WindowsNativeRawInputWrapper.Types;

namespace RawInputDataSinkServer
{
    internal class EntryPoint
    {
        #region Private fields
        private static UdpBroadcaster? s_broadcaster;
        private static readonly Thread s_sendQueueMonitor = new(MonitorSendQueue) { IsBackground = true };
        private static readonly ConcurrentQueue<RawKeyboardInput> s_eventsToSend = new();
        private static readonly Dictionary<long, HashSet<RawKeyboardInput.KeyboardScanCode>> s_keysCurrentlyDown = new();
        private static bool s_shouldClose = false;
        private static bool s_isVerbose;
        #endregion

        public static void RunProgram(RawInputDataSinkServerArguments arguments)
        {
            s_isVerbose = arguments.Verbose;
            var addresses = FetchLocalIpv4Addresses();
            while (addresses.Count == 0)
            {
                Thread.Sleep(1000);
                addresses = FetchLocalIpv4Addresses();
            }

            var localAddress = SelectServerAddress(addresses, arguments.LocalNetworkInterfacePreference!);
            Console.WriteLine($"Local address: {localAddress}");
            s_broadcaster = new(localAddress, arguments.UdpBroadcastPort);

            s_sendQueueMonitor.Start();

            var inputWindow = new InputWindow(s_eventsToSend);
            if (!WinApiWrapper.TryRegisterForKeyboardInput(inputWindow.WindowHandle, out var errorMessage))
                throw new InvalidOperationException(errorMessage);

            Console.WriteLine($"Input sink setup, listening...");
            while (User32Interops.GetMessage(out var msg, IntPtr.Zero, 0, 0))
            {
                User32Interops.TranslateMessage(msg);
                User32Interops.DispatchMessage(msg);
            }
            s_shouldClose = true;
            s_broadcaster.Dispose();
        }

        #region Private methods
        private static void MonitorSendQueue(object? obj)
        {
            while (!s_shouldClose)
            {
                if (s_eventsToSend.TryDequeue(out var newInput))
                {
                    var key = newInput.ScanCode;
                    var deviceId = newInput.Header.DeviceHandle;

                    if (newInput.IsKeyDown)
                    {
                        if (s_keysCurrentlyDown.TryGetValue(deviceId, out var keysHeldDown))
                        {
                            if (keysHeldDown.Contains(key))
                                continue;
                        }
                        else
                        {
                            s_keysCurrentlyDown.Add(deviceId, new());
                            keysHeldDown = s_keysCurrentlyDown[deviceId];
                        }
                        keysHeldDown.Add(key);
                    }
                    else
                    {
                        if (s_keysCurrentlyDown.TryGetValue(deviceId, out var keysHeldDown) && keysHeldDown.Contains(key))
                            keysHeldDown.Remove(key);
                    }

                    var msg = new byte[11];
                    Serialize(msg, (byte)(newInput.IsKeyDown ? 0x01 : 0x00), deviceId, (ushort)key);

                    if (!s_broadcaster!.TrySendMessage(msg))
                        Console.WriteLine("Failed to send message");

                    if (s_isVerbose)
                        Console.WriteLine($"Sent key event {(newInput.IsKeyDown ? "down" : "up")} for key {key} ({deviceId})");
                }
                Thread.Sleep(10);
            }
        }

        private static void Serialize(byte[] data, byte eventType, long id, ushort key)
        {
            var idx = 0;
            data[idx++] = eventType;
            for (var i = 0; i < 8; i++)
                data[idx++] = (byte)((id >> (8 * i)) & 0xff);

            data[idx++] = (byte)(key & 0xff);
            data[idx] = (byte)((key >> 8) & 0xff);
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
        #endregion
    }
}
