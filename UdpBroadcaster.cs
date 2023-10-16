using System;
using System.Net;
using System.Net.Sockets;

namespace RawInputDataSinkServer.Communication
{
    internal class UdpBroadcaster : IDisposable
    {
        #region Private fields
        private readonly UdpClient _client;
        private readonly IPEndPoint _broadcastEndPoint;
        private readonly int _udpTargetPort;
        private bool _disposedValue;
        #endregion

        public UdpBroadcaster(IPAddress localAddress, int udpTargetPort)
        {
            if (udpTargetPort < 1)
                throw new InvalidOperationException($"{udpTargetPort} port must be positive and non-zero!");

            _udpTargetPort = udpTargetPort;

            _client = new UdpClient(new IPEndPoint(localAddress, 0))
            {
                EnableBroadcast = true
            };
            _broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, _udpTargetPort);
        }

        #region Public interface methods
        public bool TrySendMessage(byte[] data)
        {
            try
            {
                _client.Send(data, data.Length, _broadcastEndPoint);
                return true;
            }
            catch (Exception e) when (e is SocketException or ObjectDisposedException)
            {
                return false;
            }
        }
        #endregion

        #region Disposal
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                    _client.Close();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
