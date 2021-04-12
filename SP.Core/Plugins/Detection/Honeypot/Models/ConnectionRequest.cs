using System;
using System.Net;
using System.Net.Sockets;

namespace Plugins.Models
{
    public class ConnectionEventArgs : EventArgs
    {
        public ConnectionEventArgs(TcpClient client)
        {
            Client = client;

            if (client.Client.LocalEndPoint != null)
            {
                Port = ((IPEndPoint) client.Client.LocalEndPoint).Port;
            }

            if (client.Client.RemoteEndPoint != null)
            {
                IpAddress = ((IPEndPoint) client.Client.RemoteEndPoint).Address.ToString();
            }
        }

        public TcpClient Client { get; }
        public int Port { get; }
        public string IpAddress { get; }
    }
}