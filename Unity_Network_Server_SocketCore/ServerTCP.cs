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

        public static void SendDataToAll(ByteBuffer dataBuffer)
        {
            foreach (var client in clients)
            {
                SendDataTo(client.Key, dataBuffer, true);
            }
            dataBuffer.Dispose();
        }
        public static void SendDataToAll(Socket ignoreThisSocket, ByteBuffer dataBuffer)
        {
            foreach (var client in clients)
            {
                if (client.Key != ignoreThisSocket)
                {
                    SendDataTo(client.Key, dataBuffer, true);
                }
            }
            dataBuffer.Dispose();
        }
        public static Socket GetSocketByGuid(Guid id)
        {
            foreach (var client in clients)
            {
                if (client.Value.id == id)
                {
                    return client.Key;
                }
            }
            return null;
        }

        public static Guid GetGuidBySocket(Socket socket)
        {
            foreach (var client in clients)
            {
                if (client.Key == socket)
                {
                    return client.Value.id;
                }
            }
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

        public static void PACKET_SendPlayerMovement(Socket socket, float posX, float posY, float rotation)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendPlayerMovement);

            buffer.Write(clients[socket].id.ToByteArray().Length);
            buffer.Write(clients[socket].id.ToByteArray());

            buffer.Write(posX);
            buffer.Write(posY);
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
        public static void PACKET_ProjectileToClient(Socket socket, Guid bulletID)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendNewProjectile);

            buffer.Write(clients[socket].id.ToByteArray().Length);
            buffer.Write(clients[socket].id.ToByteArray());
            buffer.Write(bulletID.ToByteArray().Length);
            buffer.Write(bulletID.ToByteArray());

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
        }
    }
}
