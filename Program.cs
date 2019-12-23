using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace TCPHolePunch
{
    class Program
    {
        static void Main(string[] args)
        {
            string defaultServerHost="127.0.0.1";
            Console.Write("Main server hostname [" + defaultServerHost + "] Enter Server Host IP: ");
            string serverHost = Console.ReadLine();
            TcpClient client = ConnectTcpClient(serverHost.Length > 0 ? serverHost : defaultServerHost, 9009);

            TcpListener listener = CreateTcpListener(client.Client.LocalEndPoint as IPEndPoint);
            listener.Start();
            Task<TcpClient> incommingTraverse = TraverseIncoming(listener);

            StreamReader reader = new StreamReader(client.GetStream());
            string publicAddress = reader.ReadLine();
            Console.WriteLine("Public endpoint: " + publicAddress);

            Console.Write("Remote endpoint: ");
            string[] remoteEndPointSplit = Console.ReadLine().Split(':');

            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteEndPointSplit[0]), int.Parse(remoteEndPointSplit[1]));

            TcpClient publicClient = CreateTcpClient(client.Client.LocalEndPoint as IPEndPoint);

            bool publicTraverse = TraverseOutgoing(publicClient, remoteEndPoint).Result;

            if (publicTraverse)
            {
                Console.WriteLine("Connected via public connection");
            }
            else
            {
                Console.WriteLine("Connection failed");
            }

            Console.Read();
        }

        static TcpClient CreateTcpClient(IPEndPoint localEndPoint)
        {
            TcpClient client = new TcpClient();
            client.Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(localEndPoint);
            return client;
        }

        static TcpListener CreateTcpListener(IPEndPoint localEndPoint)
        {
            TcpListener listener = new TcpListener(localEndPoint);
            listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            return listener;
        }

        static TcpClient ConnectTcpClient(string host, int port)
        {
            TcpClient client = new TcpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Connect(host, port);
            return client;
        }

        static async Task<TcpClient> TraverseIncoming(TcpListener listener)
        {
            try
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Accepted client");
                return client;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        static async Task<bool> TraverseOutgoing(TcpClient client, IPEndPoint remoteEndPoint)
        {
            try
            {
                await client.ConnectAsync(remoteEndPoint.Address.ToString(), remoteEndPoint.Port);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
