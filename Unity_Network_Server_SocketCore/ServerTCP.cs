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
        public static Dictionary<Socket, ClientSocket> _clientSockets = new Dictionary<Socket, ClientSocket>();
        public static int onlinePlayerCount;
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static int port = 7171;

        public static void SetupServer()
        {
            //Console.WriteLine("Setting up server...");
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            _serverSocket.Listen(10);
            _serverSocket.BeginAccept(new AsyncCallback(ClientConnectCallback), null);
            Console.WriteLine("Server is running...");
        }

        private static int GetPositiveHashCode()
        {
            Object obj = new Object();
            int num = obj.GetHashCode();
            num = (num < 0) ? -num : num;
            return num;
        }

        private static void ClientConnectCallback(IAsyncResult AR)
        {
            Socket socket = _serverSocket.EndAccept(AR);
            int id = GetPositiveHashCode();
            bool idSet = false;

            //Make sure we get a unique id
            do
            {
                foreach (var item in _clientSockets)
                {
                    if (item.Value.connectionID == id)
                    {
                        id = GetPositiveHashCode();
                        Console.WriteLine($"ServerTCP::ClientConnectCallback | ID '{item.Value.connectionID}' did already exist! generated a new one.");
                        // still possible to get same id as an past iteration, even though its unlikely
                    }
                }
                idSet = true;
            } while (!idSet);

            _clientSockets.Add(socket, new ClientSocket(ref socket, id));
            onlinePlayerCount++;
            _serverSocket.BeginAccept(new AsyncCallback(ClientConnectCallback), null);
        }
        private static void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }
        public static void SendDataTo(ref Socket socket, byte[] data)
        {
            if (_clientSockets[socket].isConnected)
            {
                ByteBuffer buffer = new ByteBuffer();
                buffer.WriteBytes(data);
                socket.BeginSend(buffer.ToArray(), 0, buffer.ToArray().Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                if (DEBUG_PACKETS)
                {
                    int packageID = buffer.ReadInteger();
                    if (packageID <= 1) { return; } // packageID 1 = ping package, anoying to debug...
                    Console.WriteLine(String.Format($"PACKET '{packageID}' sent to connectionID '{_clientSockets[socket].connectionID}'"));
                }
                buffer.Dispose();
            }
        }

        public static void SendDataToAll(byte[] data)
        {
            foreach (var client in _clientSockets)
            {
                if (client.Value != null && client.Value.isConnected == true)
                {
                    // Is there any benefit from using a ref for the client socket?
                    Socket socket = client.Key;
                    SendDataTo(ref socket, data);
                    //SendDataTo(client.Key, data);
                }
            }
        }
        public static void SendDataToAllBut(ref Socket socket, byte[] data)
        {
            foreach (var client in _clientSockets)
            {
                if (client.Value != null && client.Value.isConnected == true)
                {
                    if (client.Key != socket)
                    {
                        // Is there any benefit from using a ref for the client socket?
                        Socket tempSocket = client.Key;
                        SendDataTo(ref tempSocket, data);
                        //SendDataTo(client.Key, data);
                    }
                }
            }
        }
        public static Socket GetSocketByConnectionID(int connectionID)
        {
            foreach (var client in _clientSockets)
            {
                if (client.Value.connectionID == connectionID)
                {
                    return client.Key;
                }
            }
            return null;
        }
        public static void RemoveClientObject(ref Socket socket)
        {
            if (onlinePlayerCount > 0) { onlinePlayerCount--; }
            
            PACKET_SendRemovePlayer(ref socket);
            Socket closeSocket = socket;
            _clientSockets.Remove(socket);
            closeSocket.Close();
        }

        public static void PACKET_PingToClient(ref Socket socket)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.Server_PingClient);

            buffer.WriteInteger(onlinePlayerCount);

            SendDataTo(ref socket, buffer.ToArray());
            buffer.Dispose();
        }

        public static void PACKET_ChatmessageToClient(ref Socket socket, string message)
        {
            // Create a new buffer
            ByteBuffer buffer = new ByteBuffer();

            // Write the Package ID
            buffer.WriteInteger((int)ServerPackages.Server_SendChatMessageClient);
            // Write the connectionID
            buffer.WriteInteger(_clientSockets[socket].connectionID);
            // Write the message
            buffer.WriteString(message);

            // Send it to all but socket
            SendDataToAllBut(ref socket, buffer.ToArray());
            // Dispose the buffer
            buffer.Dispose();
        }

        public static void PACKET_SendConnectionID(ref Socket socket)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.Server_SendConnectionID);

            buffer.WriteInteger(_clientSockets[socket].connectionID);

            SendDataTo(ref socket, buffer.ToArray());
            buffer.Dispose();
        }

        public static void PACKET_SendNewPlayerToWorld(ref Socket socket)
        {
            // Send this player to all players on the server, except the player itself
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.Server_SendNewPlayerToWorldPlayers);

            buffer.WriteInteger(_clientSockets[socket].connectionID);
            buffer.WriteInteger(_clientSockets[socket].player.SpriteID);

            SendDataToAllBut(ref socket, buffer.ToArray());
            buffer.Dispose();
        }

        public static void PACKET_SendWorldPlayersToNewPlayer(ref Socket socket)
        {
            if (_clientSockets.Count <= 1) { return; } // no need to send this packet if there is only 1 player online since there is nothing to retrive

            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.Server_SendWorldPlayersToNewPlayer);

            buffer.WriteInteger(_clientSockets.Count - 1); // Send the amount of players connected to the server, minus the local player
            foreach (var client in _clientSockets)
            {
                if (client.Key != socket)
                {
                    buffer.WriteInteger(client.Value.player.ConnectionID); // could just use key, but to be more specific, i take the values connection id
                    buffer.WriteFloat(client.Value.player.PosX);
                    buffer.WriteFloat(client.Value.player.PosY);
                    buffer.WriteFloat(client.Value.player.Rotation);
                    buffer.WriteInteger(client.Value.player.SpriteID);
                }
            }

            SendDataTo(ref socket, buffer.ToArray());
            buffer.Dispose();
        }

        public static void PACKET_SendPlayerMovement(ref Socket socket, float posX, float posY, float rotation)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.Server_SendPlayerMovement);

            buffer.WriteInteger(_clientSockets[socket].connectionID);

            buffer.WriteFloat(posX);
            buffer.WriteFloat(posY);
            buffer.WriteFloat(rotation);

            SendDataToAllBut(ref socket, buffer.ToArray());
            buffer.Dispose();
        }
        public static void PACKET_SendRemovePlayer(ref Socket socket)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.Server_SendRemovePlayer);

            buffer.WriteInteger(_clientSockets[socket].connectionID);

            SendDataToAllBut(ref socket, buffer.ToArray());
            buffer.Dispose();
        }
        public static void PACKET_ProjectileToClient(ref Socket socket, int bulletID)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.Server_SendNewProjectile);

            buffer.WriteInteger(_clientSockets[socket].connectionID);
            buffer.WriteInteger(bulletID);

            //SendDataToAll(buffer.ToArray());
            SendDataToAllBut(ref socket, buffer.ToArray());
            buffer.Dispose();
        }

        public static void PACKET_RemoveProjectileFromClient(ref Socket socket, int bulletID)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.Server_SendRemoveProjectile);
            buffer.WriteInteger(bulletID);

            SendDataToAllBut(ref socket, buffer.ToArray());
            buffer.Dispose();
        }
        
    }
}
