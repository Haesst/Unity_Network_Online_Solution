using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Unity_Network_Server
{
    class ServerTCP
    {
        private const bool DEBUG_PACKETS = false;
        private static TcpListener serverSocket;

        public static Dictionary<int, ClientObject> clientObjects = new Dictionary<int, ClientObject>();
        public static Dictionary<int, Player> players = new Dictionary<int, Player>();

        public static void InitializeServer()
        {
            //InitializeMySQLServer();
            InitializeServerSocket();

            Console.WriteLine("\nServer is Running...");
        }

        private static void InitializeMySQLServer()
        {
            MySQL.mySQLSettings.user = "root";
            #region MySQL.mySQLSettings.password
            MySQL.mySQLSettings.password = "1234";
            #endregion MySQL.mySQLSettings.password
            MySQL.mySQLSettings.server = "localhost";
            MySQL.mySQLSettings.database = "DatabaseName";

            MySQL.ConnectToMySQL();
        }

        private static void InitializeServerSocket()
        {
            ServerHandleData.InitializePacketListener();

            serverSocket = new TcpListener(IPAddress.Any, 7171);
            serverSocket.Start();
            serverSocket.BeginAcceptTcpClient(new AsyncCallback(ClientConnectCallback), null);
        }

        public static void RemoveClientObject(int connectionID)
        {
            clientObjects.Remove(connectionID);
            players.Remove(connectionID);
            PACKET_SendRemovePlayer(connectionID);
        }

        private static int GetPositiveHashCode()
        {
            Object obj = new Object();
            int num = obj.GetHashCode();
            num = (num < 0) ? -num : num;
            return num;
        }

        private static void ClientConnectCallback(IAsyncResult result)
        {
            TcpClient tempClient = serverSocket.EndAcceptTcpClient(result);
            serverSocket.BeginAcceptTcpClient(new AsyncCallback(ClientConnectCallback), null);
            //int id = ((IPEndPoint)tempClient.Client.RemoteEndPoint).Port;
            int id = GetPositiveHashCode();
            if (!clientObjects.ContainsKey(id))
            {
                clientObjects.Add(id, new ClientObject(tempClient, id));
                players.Add(id, new Player(id));
            }
            else
            {
                //NOTE: Will this "reconnect" if the port/key already exists, and try untill it find an empty key?
                // Should we close the connection? from ClientObjects class
                Console.WriteLine($"clientObject with id: {id} already exits!!");
            }
        }

        public static async void SendDataTo(int connectionID, byte[] data)
        {
            //Prevent server from sending data to a disconnected or empty client socket
            if (clientObjects[connectionID].socket != null)
            {
                ByteBuffer buffer = new ByteBuffer();
                buffer.WriteInteger((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
                buffer.WriteBytes(data);
                clientObjects[connectionID].myStream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
                buffer.Dispose();
                await Task.Delay(75);

                if (DEBUG_PACKETS)
                {
                    ByteBuffer debugBuffer = new ByteBuffer();
                    debugBuffer.WriteBytes(data);
                    int packageID = debugBuffer.ReadInteger();
                    Console.WriteLine(String.Format("PACKET '{0}' sent to connectionID '{1}'", packageID, connectionID));
                    debugBuffer.Dispose();
                }
            }
        }

        public static void SendDataToAll(byte[] data)
        {
            foreach (var client in clientObjects)
            {
                if (client.Value != null && client.Value.isConnected == true)
                {
                    SendDataTo(client.Key, data);
                }
            }
        }

        public static void SendDataToAllBut(int connectionID, byte[] data)
        {
            foreach (var client in clientObjects)
            {
                if (client.Value != null && client.Value.isConnected == true)
                {
                    if (client.Key != connectionID)
                    {
                        SendDataTo(client.Key, data);
                    }
                }
            }
        }

        public static void PACKET_PingToClient(int connectionID)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SPingClient);

            SendDataTo(connectionID, buffer.ToArray());
            buffer.Dispose();
        }

        public static void PACKET_ChatmessageToClient(int connectionID, string message)
        {
            // Create a new buffer
            ByteBuffer buffer = new ByteBuffer();

            // Write the Package ID
            buffer.WriteInteger((int)ServerPackages.SSendChatMessageClient);
            // Write the connectionID
            buffer.WriteInteger(connectionID);
            // Write the message
            buffer.WriteString(message);

            // Send it to all but connectionID
            SendDataToAllBut(connectionID, buffer.ToArray());
            // Dispose the buffer
            buffer.Dispose();
        }

        public static void PACKET_SendConnectionID(int connectionID)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SSendConnectionID);

            buffer.WriteInteger(connectionID);

            SendDataTo(connectionID, buffer.ToArray());
            buffer.Dispose();
        }

        public static void PACKET_SendNewPlayerToWorld(int connectionID)
        {
            // Send this player to all players on the server, except the player itself
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SSendNewPlayerToWorldPlayers);

            buffer.WriteInteger(connectionID);
            buffer.WriteInteger(ServerTCP.players[connectionID].SpriteID);

            SendDataToAllBut(connectionID, buffer.ToArray());
            buffer.Dispose();
        }

        public static void PACKET_SendWorldPlayersToNewPlayer(int connectionID)
        {
            if (players.Count <= 1) { return; } // no need to send this packet if there is only 1 player online since there is nothing to retrive

            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SSendWorldPlayersToNewPlayer);

            buffer.WriteInteger(players.Count - 1); // Send the amount of players connected to the server, minus the local player
            foreach (var player in players)
            {
                if (player.Key != connectionID)
                {
                    buffer.WriteInteger(player.Value.ConnectionID); // could just use key, but to be more specific, i take the values connection id
                    buffer.WriteFloat(player.Value.PosX);
                    buffer.WriteFloat(player.Value.PosY);
                    buffer.WriteFloat(player.Value.Rotation);
                    buffer.WriteInteger(player.Value.SpriteID);
                }
            }

            SendDataTo(connectionID, buffer.ToArray());
            buffer.Dispose();
        }

        public static void PACKET_SendPlayerMovement(int connectionID, float posX, float posY, float rotation)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SSendPlayerMovement);

            buffer.WriteInteger(connectionID);

            buffer.WriteFloat(posX);
            buffer.WriteFloat(posY);
            buffer.WriteFloat(rotation);

            SendDataToAllBut(connectionID, buffer.ToArray());
            buffer.Dispose();
        }
        public static void PACKET_SendRemovePlayer(int connectionID)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SSendRemovePlayer);

            buffer.WriteInteger(connectionID);

            SendDataToAllBut(connectionID, buffer.ToArray());
            buffer.Dispose();
        }

    }
}
