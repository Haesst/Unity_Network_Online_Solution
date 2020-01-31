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
        private const bool DEBUG_PACKETS = true;
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
                clients.Add(socket, new ClientSocket(ref socket, Guid.NewGuid()));
            }

            serverSocket.BeginAccept(new AsyncCallback(ClientConnectCallback), null);
        }

        private static void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }
        public static void SendDataTo(ref Socket socket, ref ByteBuffer dataBuffer)
        {
            if (clients[socket].socket.Connected)
            {
                byte[] data = dataBuffer.ToArray();
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            }

            dataBuffer.Dispose();
        }

        public static void SendDataToAll(ref ByteBuffer dataBuffer)
        {
            foreach (var client in clients)
            {
                if (client.Value.socket.Connected)
                {
                    // Is there any benefit from using a ref for the client socket?
                    Socket socket = client.Key;
                    SendDataTo(ref socket, ref dataBuffer);
                }
            }
        }
        public static void SendDataToAll(ref Socket ignoreThisSocket, ref ByteBuffer dataBuffer)
        {
            foreach (var client in clients)
            {
                if (client.Value.socket.Connected)
                {
                    if (client.Key != ignoreThisSocket)
                    {
                        // Is there any benefit from using a ref for the client socket?
                        Socket socket = client.Key;
                        SendDataTo(ref socket, ref dataBuffer);
                    }
                }
            }
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
        public static void RemoveClientObject(Socket socket)
        {
            PACKET_SendRemovePlayer(ref socket);
            Socket closeSocket = socket;
            clients.Remove(socket);
            closeSocket.Close();
        }

        public static void PACKET_PingToClient(ref Socket socket)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_PingClient);

            SendDataTo(ref socket, ref buffer);
        }

        public static void PACKET_ChatmessageToClient(ref Socket socket, string message)
        {
            // Create a new buffer
            ByteBuffer buffer = new ByteBuffer();

            // Write the Package ID
            buffer.Write((int)ServerPackages.Server_SendChatMessageClient);
            // Write the connectionID
            buffer.Write(clients[socket].id.ToByteArray().Length);
            buffer.Write(clients[socket].id.ToByteArray()); // note should this be a string instead?????
            // Write the message
            buffer.Write(message);

            // Send it to all but socket
            SendDataToAll(ref socket, ref buffer);
        }

        public static void PACKET_SendGuid(ref Socket socket)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendGuid);

            buffer.Write(clients[socket].id.ToByteArray().Length);
            buffer.Write(clients[socket].id.ToByteArray());

            SendDataTo(ref socket, ref buffer);
        }

        public static void PACKET_SendNewPlayerToWorld(ref Socket socket)
        {
            // Send this player to all players on the server, except the player itself
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

            SendDataToAll(ref buffer);
        }

        public static void PACKET_SendWorldPlayersToNewPlayer(ref Socket socket)
        {
            if (clients.Count <= 1) { return; } // no need to send this packet if there is only 1 player online since there is nothing to retrive

            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendWorldPlayersToNewPlayer);

            buffer.Write(clients.Count - 1); // Send the amount of players connected to the server, minus the local player
            foreach (var client in clients)
            {
                if (client.Key != socket)
                {
                    buffer.Write(client.Value.player.Id.ToByteArray().Length);
                    buffer.Write(client.Value.player.Id.ToByteArray());
                    buffer.Write(client.Value.player.PosX);
                    buffer.Write(client.Value.player.PosY);
                    buffer.Write(client.Value.player.Rotation);
                    buffer.Write(client.Value.player.SpriteID);
                    buffer.Write(client.Value.player.Name);
                }
            }

            SendDataTo(ref socket, ref buffer);
        }

        public static void PACKET_SendPlayerMovement(ref Socket socket, float posX, float posY, float rotation)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendPlayerMovement);

            buffer.Write(clients[socket].id.ToByteArray().Length);
            buffer.Write(clients[socket].id.ToByteArray());

            buffer.Write(posX);
            buffer.Write(posY);
            buffer.Write(rotation);

            SendDataToAll(ref socket, ref buffer);
        }
        public static void PACKET_SendRemovePlayer(ref Socket socket)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendRemovePlayer);

            buffer.Write(clients[socket].id.ToByteArray().Length);
            buffer.Write(clients[socket].id.ToByteArray());

            SendDataToAll(ref socket, ref buffer);
        }
        public static void PACKET_ProjectileToClient(ref Socket socket, int bulletID)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendNewProjectile);

            buffer.Write(clients[socket].id.ToByteArray().Length);
            buffer.Write(clients[socket].id.ToByteArray());
            buffer.Write(bulletID);

            //SendDataToAll(buffer.ToArray());
            SendDataToAll(ref socket, ref buffer);
        }
        public static void PACKET_SendPlayerHealth(ref Socket socket, int health)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendPlayerHealth);

            buffer.Write(clients[socket].id.ToByteArray().Length);
            buffer.Write(clients[socket].id.ToByteArray());
            buffer.Write(health);

            SendDataToAll(ref buffer);
        }

        // No use at the moment
        public static void PACKET_SendPlayerDied(ref Socket socket, Player player)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ServerPackages.Server_SendPlayerDied);

            buffer.Write(player.Id.ToByteArray().Length);
            buffer.Write(player.Id.ToByteArray());

            buffer.Write(player.PosX);
            buffer.Write(player.PosY);
            buffer.Write(player.Rotation);

            buffer.Write(player.Health);

            SendDataToAll(ref buffer);
        }
    }
}
