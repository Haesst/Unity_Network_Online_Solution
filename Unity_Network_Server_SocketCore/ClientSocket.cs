using System;
using System.Net.Sockets;

namespace Unity_Network_Server_SocketCore
{
    class ClientSocket
    {
        public int connectionID;
        public byte[] _buffer;
        private Socket _socket;
        public bool isConnected;
        public Player player;

        public ClientSocket(ref Socket socket, int connectionID)
        {
            this.connectionID = connectionID;
            _socket = socket;
            _socket.NoDelay = true;
            _buffer = new byte[4096];
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), _socket);
            Console.WriteLine($"Incoming connection from {_socket.RemoteEndPoint.ToString()} | connectionID: {connectionID}");
            isConnected = true;
            player = new Player(connectionID);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                Socket socket = (Socket)AR.AsyncState;
                int received = socket.EndReceive(AR);
                byte[] dataBuffer = new byte[received];
                Array.Copy(_buffer, dataBuffer, received);
                try
                {
                    ServerHandleData.HandleData(socket, dataBuffer);
                    _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), _socket);
                }
                catch (Exception)
                {
                    CloseConnection();
                }
            }
            catch (Exception ex)
            {
                CloseConnection();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n{ex.ToString()}\n");
                Console.ResetColor();
                return;
            }

        }

        private void CloseConnection()
        {
            isConnected = false;
            Console.WriteLine($"User disconnected : {_socket.RemoteEndPoint.ToString()} | ID: {connectionID}");
            ServerTCP.RemoveClientObject(ref _socket);
        }
    }
}
