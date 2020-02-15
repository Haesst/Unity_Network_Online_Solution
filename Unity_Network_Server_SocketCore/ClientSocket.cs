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

        public ClientSocket(Socket socket, Guid id)
        {
            Console.WriteLine($"Incoming connection from {socket.RemoteEndPoint.ToString()} | connectionID: {id}");
            this.id = id;
            this.socket = socket;
            this.socket.NoDelay = true;
            buffer = new byte[4096];
            this.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), this.socket);
            player = new Player(id);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;

            try
            {
                if (this.socket.Connected)
                {
                    int received = socket.EndReceive(AR);
                    if (received > 0)
                    {
                        byte[] dataBuffer = new byte[received];
                        Array.Copy(buffer, dataBuffer, received);
                        ServerHandleData.HandleData(socket, dataBuffer);
                        try
                        {
                            this.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), this.socket);
                        }
                        catch (Exception)
                        {
                            // need this to avoid printing error message when client disconnects, should redo this to avoid try statement
                            return;
                        }
                    }
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

        public void CloseConnection()
        {
            if (ServerTCP.clients.ContainsKey(socket))
            {
                ServerTCP.PACKET_SendRemovePlayer(socket);
                //for (int i = 0; i < ServerTCP.highscorePlayers.Length; i++)
                //{
                //    if (ServerTCP.highscorePlayers[i] != null)
                //    {
                //        if (ServerTCP.highscorePlayers[i].Id != null && ServerTCP.highscorePlayers[i].Id == id)
                //        {
                //            ServerTCP.highscorePlayers[i] = null;
                //            break;
                //        }
                //    }
                //}
                Console.WriteLine($"User disconnected : {socket.RemoteEndPoint.ToString()} | ID: {id}");
                socket.Shutdown(SocketShutdown.Both);
                ServerTCP.clients.Remove(socket);
                socket.Close();

                if (ServerTCP.clients.Count <= 0) { Console.WriteLine($"Server is empty."); }
            }
        }
    }
}