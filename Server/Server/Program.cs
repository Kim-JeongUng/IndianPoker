using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    class Server
    {
        private TcpListener server;
        private bool isRunning;

        public Server(string ipAddress, int port)
        {
            server = new TcpListener(IPAddress.Parse(ipAddress), port);
            server.Start();
            isRunning = true;

            Console.WriteLine($"Server started on {ipAddress}:{port}");

            StartListening();
        }

        public void StartListening()
        {
            while (isRunning)
            {
                // Accept a new client
                TcpClient newClient = server.AcceptTcpClient();
                Console.WriteLine("Client connected!");

                // Handle client in a new thread
                Thread clientThread = new Thread(() => HandleClient(newClient));
                clientThread.Start();
            }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received: {message}");

                // Echo the message back to the client
                byte[] response = Encoding.ASCII.GetBytes("Echo: " + message);
                stream.Write(response, 0, response.Length);
            }

            Console.WriteLine("Client disconnected.");
            client.Close();
        }

        public static void Main(string[] args)
        {
            string ipAddress = "127.0.0.1";
            int port = 5000;

            Server server = new Server(ipAddress, port);
        }
    }
}
