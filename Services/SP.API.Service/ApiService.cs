using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SP.API.Service
{
    internal class ApiService : BackgroundService
    {
        private TcpListener server;
        private ILogger log;
        public event EventHandler<ConnectionEventArgs> ConnectionRequest;
        public event EventHandler<DataEventArgs> CommandReceived;
        public event EventHandler<DataEventArgs> DataReceived;
        public event EventHandler<DataEventArgs> DataSend;

        protected virtual void OnConnectionRequest(ConnectionEventArgs e)
        {
            ConnectionRequest?.Invoke(this, e);
        }

        protected virtual void OnCommandReceived(DataEventArgs e)
        {
            CommandReceived?.Invoke(this, e);
        }

        protected virtual void OnDataReceived(DataEventArgs e)
        {
            DataReceived?.Invoke(this, e);
        }

        protected virtual void OnDataSend(DataEventArgs e)
        {
            DataSend?.Invoke(this, e);
        }

        public void Start(string ip, int port)
        {
            try
            {
                IConfigurationRoot config = new ConfigurationBuilder()
                    .AddJsonFile("appSettings.json", true, true)
                    .Build();

                log = new LoggerConfiguration()
                    .ReadFrom.Configuration(config)
                    .CreateLogger()
                    .ForContext(typeof(ApiService));

                // Initialize the TcpListener.
                server = new TcpListener(IPAddress.Parse(ip), port);

                // Start listening for client requests.
                server.Start();

                // Diagnostics.
                log.Information("Server started at {0} : {1}", ip, port);

                // Enter the listening loop.
                while (true)
                {
                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();

                    // Update Events.
                    OnConnectionRequest(new ConnectionEventArgs(client));

                    // Create thread for connected client.
                    ThreadPool.QueueUserWorkItem(ThreadProc, client);
                }
            }
            catch (SocketException e)
            {
                log.Error("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                log.Error("Exception: {0}", e);
            }
            finally
            {
                Stop();
            }
        }

        private void ThreadProc(object state)
        {
            TcpClient client = (TcpClient)state;

            // Buffer for reading data.
            byte[] bytes = new byte[128];

            // Get a stream object for reading and writing.
            NetworkStream stream = client.GetStream();

            try
            {
                // Loop to receive all the data sent by the client.
                int i;
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string
                    string data = Encoding.ASCII.GetString(bytes, 0, i);

                    // Update Events.
                    OnDataReceived(new DataEventArgs(client, stream, data));
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }

            // Shutdown and end connection
            client.Close();
        }

        public void Stop()
        {
            server?.Stop();
        }
    }
}