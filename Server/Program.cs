using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Server
{
   class Program
    {
      
        static List<Socket> _clientSockets = new List<Socket>();
        static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private const int BUFFER_SIZE = 600;
        private static readonly byte[] _buffer = new byte[BUFFER_SIZE];

        static void Main(string[] args)
        {
            SetupServer();
            Console.ReadLine();
        }

        private static void SetupServer()
        {
            const int Port = 7899;
            const string Host = "127.0.0.1"; 
            IPAddress address = IPAddress.Parse(Host);
            _serverSocket.Bind(new IPEndPoint(address, Port));
            _serverSocket.Listen(100);
            _serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine($"Server has been started on: ip:{Host} port:{Port}");
            Console.WriteLine("Waiting connections....");
        }
        
        private static void AcceptCallback(IAsyncResult ar)
        {
            Socket socket;
            try
            {
                socket = _serverSocket.EndAccept(ar);

            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }

            _clientSockets.Add(socket);
            Console.WriteLine("Client connected");
            socket.BeginReceive(_buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket); //Асинхронный Приём данных с подключенного пользователя
            _serverSocket.BeginAccept(AcceptCallback, null); //Принимает попытку входящего подключения

        }
        
        private static void ReceiveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            int received = 0;
            try
            {
                received = socket.EndReceive(ar);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                socket.Close(); // Dont shutdown because the socket may be disposed and its disconnected anyway
                _clientSockets.Remove(socket);
                return;
            }
            byte[] dataBuf = new byte[received]; //запись принятых байтов в массив 
            Array.Copy(_buffer, dataBuf, received);
            string text = Encoding.UTF8.GetString(dataBuf); //запись массива в строку
            Console.WriteLine("Message received: " + text);
            foreach (Socket clientSocket in _clientSockets)  //отправка сообщения остальным пользователям
            {
                //if (clientSocket != socket)
                //{
                    clientSocket.Send(dataBuf);
                //}

            }
            socket.BeginReceive(_buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }
























    }
}