using System;
using System.Net.Sockets;

namespace Unity_Network_Server_SocketCore
{
    class ClientSocket
    {
        public readonly Socket socket;
        public Guid id;
        public byte[] buffer;
        public Player player;

        public ClientSocket(ref Socket socket, Guid id)
        {
            this.id = id;
            this.socket = socket;
            this.socket.NoDelay = true;
            buffer = new byte[4096];
            this.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), this.socket);
            Console.WriteLine($"Incoming connection from {socket.RemoteEndPoint.ToString()} | connectionID: {id}");
            player = new Player(id);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                Socket socket = (Socket)AR.AsyncState;
                if (socket.Connected)
                {
                    int received = socket.EndReceive(AR);
                    byte[] dataBuffer = new byte[received];
                    Array.Copy(buffer, dataBuffer, received);
                    ServerHandleData.HandleData(ref socket, ref dataBuffer);
                    this.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), this.socket);
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
            Console.WriteLine($"User disconnected : {socket.RemoteEndPoint.ToString()} | ID: {id}");
            ServerTCP.RemoveClientObject(socket);
        }
    }
}
