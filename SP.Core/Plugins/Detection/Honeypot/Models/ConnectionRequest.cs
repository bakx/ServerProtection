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
			Port = ((IPEndPoint) client.Client.LocalEndPoint).Port;
			IpAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
		}

		public TcpClient Client { get; }
		public int Port { get; }
		public string IpAddress { get; }
	}
}