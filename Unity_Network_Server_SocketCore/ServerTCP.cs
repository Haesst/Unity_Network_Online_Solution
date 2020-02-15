using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Unity_Network_Server_SocketCore
{
    class ServerTCP
    {
        private const bool DEBUG_PACKETS_ALL = false;
        private const bool DEBUG_PACKETS = false;

        private static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static Dictionary<Socket, ClientSocket> clients = new Dictionary<Socket, ClientSocket>();

        private static int port = 7171;

        public static void InitTCP()
        {
            //Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            serverSocket.Listen(10);
            serverSocket.BeginAccept(new AsyncCallback(ClientConnectCallback), null);
            Console.WriteLine("Server is running...");
        }

        private static void ClientConnectCallback(IAsyncResult AR)
        {
            Socket socket = serverSocket.EndAccept(AR);

            if (!clients.ContainsKey(socket))
            {
                clients.Add(socket, new ClientSocket(socket, Guid.NewGuid()));
            }

            serverSocket.BeginAccept(new AsyncCallback(ClientConnectCallback), null);
        }

        private static void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }
        public static void SendDataTo(Socket socket, ByteBuffer dataBuffer, bool isSendingToAll = false)
        {
            if (dataBuffer.Length() > 0)
            {
                if (clients[socket].socket.Connected)
                {
                    byte[] data = dataBuffer.ToArray();
                    socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                }

                if (DEBUG_PACKETS_ALL || DEBUG_PACKETS)
                {
                    ByteBuffer buffer = dataBuffer;
                    int id = buffer.ReadInteger();
                    if (id == 1 && DEBUG_PACKETS_ALL == false)
                    {
                        buffer.Dispose();
                        return;
                    }
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Debug: server packageID: '{id}' sent to '{clients[socket].id}' whit a size of '{dataBuffer.Length()}' bytes.");
                    Console.ResetColor();
                    buffer.Dispose();
                }

            }
            if (!isSendingToAll)
            {
                dataBuffer.Dispose();
            }
        }

        public static /*async Task*/ void SendDataToAll(ByteBuffer dataBuffer)
        {
            foreach (ClientSocket client in clients.Values)
            {
                SendDataTo(client.socket, dataBuffer, true);
                //await Task.Delay(1);
            }

            //for (int i = 0; i < clients.Count; i++)
            //{
            //    SendDataTo(clients.ElementAt(i).Key, dataBuffer, true);
            //    //await Task.Delay(1);
            //}

            dataBuffer.Dispose();
        }
        public static /*async Task*/ void SendDataToAll(Socket ignoreThisSocket, ByteBuffer dataBuffer)
        {
            foreach (ClientSocket client in clients.Values)
            {
                if (client.socket != ignoreThisSocket)
                {
                    SendDataTo(client.socket, dataBuffer, true);
                    //await Task.Delay(1);
                }
            }

            //for (int i = 0; i < clients.Count; i++)
            //{
            //    if (clients.ElementAt(i).Key != ignoreThisSocket)
            //    {
            //        SendDataTo(clients.ElementAt(i).Key, dataBuffer, true);
            //        //await Task.Delay(1);
            //    }
            //}

            dataBuffer.Dispose();
        }
        public static Socket GetSocketByGuid(Guid id)
        {
            foreach (ClientSocket client in clients.Values)
            {
                if (client.id == id)
                {
                    return client.socket;
                }
            }

            //for (int i = 0; i < clients.Count; i++)
            //{
            //    if (clients.ElementAt(i).Value.id == id)
            //    {
            //        return clients.ElementAt(i).Key;
            //    }
            //}

            return null;
        }

        public static Guid GetGuidBySocket(Socket socket)
        {
            foreach (ClientSocket client in clients.Values)
            {
                if (client.socket == socket)
                {
                    return client.id;
                }
            }

            //for (int i = 0; i < clients.Count; i++)
            //{
            //    if (clients.ElementAt(i).Key == socket)
            //    {
            //        return clients.ElementAt(i).Value.id;
            //    }
            //}

            return Guid.Empty;
        }

        public static void PACKET_PingToClient(Socket socket)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_PingClient);

            SendDataTo(socket, buffer);
        }

        public static void PACKET_SendGuid(Socket socket)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendGuid);

            buffer.Write(clients[socket].id.ToByteArray().Length);
            buffer.Write(clients[socket].id.ToByteArray());

            SendDataTo(socket, buffer);
        }

        public static void PACKET_SendNewPlayerToWorld(Socket socket)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendNewPlayerToWorld);

            Player player = clients[socket].player;
            buffer.Write(player.Id.ToByteArray().Length);
            buffer.Write(player.Id.ToByteArray());
            buffer.Write(player.SpriteID);
            buffer.Write(player.Health);
            buffer.Write(player.Name);
            buffer.Write(player.PosX);
            buffer.Write(player.PosY);
            buffer.Write(player.Rotation);

            SendDataToAll(buffer);
        }

        public static void PACKET_SendPlayerRotation(Socket socket, float rotation)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendPlayerRotation);

            buffer.Write(clients[socket].id.ToByteArray().Length);
            buffer.Write(clients[socket].id.ToByteArray());

            buffer.Write(rotation);

            SendDataToAll(socket, buffer);
        }
        public static void PACKET_SendRemovePlayer(Socket socket)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendRemovePlayer);

            buffer.Write(clients[socket].id.ToByteArray().Length);
            buffer.Write(clients[socket].id.ToByteArray());

            SendDataToAll(socket, buffer);
        }
        public static void PACKET_ProjectileToClient(Socket socket, Guid bulletID, float rotation)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendNewProjectile);

            buffer.Write(clients[socket].id.ToByteArray().Length);
            buffer.Write(clients[socket].id.ToByteArray());
            buffer.Write(bulletID.ToByteArray().Length);
            buffer.Write(bulletID.ToByteArray());
            buffer.Write(rotation);

            SendDataToAll(socket, buffer);
        }
        public static void PACKET_SendPlayerHealth(Socket socket, int health)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendPlayerHealth);

            buffer.Write(clients[socket].id.ToByteArray().Length);
            buffer.Write(clients[socket].id.ToByteArray());
            buffer.Write(health);

            SendDataTo(socket, buffer);
        }

        public static void PACKET_SendPlayerDied(Socket socket, Player player)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendPlayerDied);

            buffer.Write(player.Id.ToByteArray().Length);
            buffer.Write(player.Id.ToByteArray());

            buffer.Write(player.PosX);
            buffer.Write(player.PosY);
            buffer.Write(player.Rotation);

            buffer.Write(player.Health);

            SendDataToAll(buffer);

            PACKET_SendHighscore();
        }

        public static void PACKET_SendHighscore()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendHighscore);

            int playerAmount = 0;
            Player[] highscorePlayers = new Player[5];
            for (int i = 0; i < clients.Count; i++)
            {
                Player player = clients.ElementAt(i).Value.player;
                for (int j = 0; j < highscorePlayers.Length; j++)
                {
                    if (highscorePlayers[j] != null)
                    {
                        if (player.Kills > highscorePlayers[j].Kills && !highscorePlayers.Contains(player))
                        {
                            Player tempPlayer = highscorePlayers[j];
                            highscorePlayers[j] = player;
                            if (j < highscorePlayers.Length - 1)
                            {
                                highscorePlayers[j + 1] = tempPlayer;
                            }
                            playerAmount++;
                        }
                    }
                    else
                    {
                        if (!highscorePlayers.Contains(player))
                        {
                            highscorePlayers[j] = player;
                            playerAmount++;
                        }
                    }
                }
            }
            if (playerAmount > highscorePlayers.Length)
            {
                playerAmount = highscorePlayers.Length;
            }

            buffer.Write(playerAmount);
            for (int i = 0; i < playerAmount; i++)
            {
                if (highscorePlayers[i] != null)
                {
                    Player player = highscorePlayers[i];
                    buffer.Write(player.Name);
                    buffer.Write(player.Kills);
                }
            }
            SendDataToAll(buffer);
        }

        public static void PACKET_SendPlayerNewMovement(Socket socket, float posX, float posY, float rotation, float thrust)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendPlayerNewMovement);

            buffer.Write(clients[socket].id.ToByteArray().Length);
            buffer.Write(clients[socket].id.ToByteArray());

            buffer.Write(posX);
            buffer.Write(posY);
            buffer.Write(rotation);
            buffer.Write(thrust);

            SendDataToAll(socket, buffer);
        }

    }
}
