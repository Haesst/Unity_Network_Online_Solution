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
        private static TcpListener serverSocket;
        public static ClientObject[] clientObjects;
        public static Player[] players;

        public static void InitializeServer()
        {
            //InitializeMySQLServer();
            InitializeClientObject();
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

        private static void InitializeClientObject()
        {
            clientObjects = new ClientObject[Constants.MAX_PLAYERS];
            players = new Player[Constants.MAX_PLAYERS];
            for (int i = 1; i < Constants.MAX_PLAYERS; i++)
            {
                clientObjects[i] = new ClientObject(null, 0);
                players[i] = new Player(0);
            }
        }

        private static void ClientConnectCallback(IAsyncResult result)
        {
            TcpClient tempClient = serverSocket.EndAcceptTcpClient(result);
            serverSocket.BeginAcceptTcpClient(new AsyncCallback(ClientConnectCallback), null);


            for (int i = 1; i < Constants.MAX_PLAYERS; i++)
            {
                if (clientObjects[i].socket == null)
                {
                    clientObjects[i] = new ClientObject(tempClient, i);
                    return;
                }
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

                if (Constants.DEBUG_PACKETS)
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
            for (int i = 1; i < Constants.MAX_PLAYERS; i++)
            {
                if (clientObjects[i] != null)
                {
                    if (clientObjects[i].isConnected)
                    {
                        SendDataTo(i, data);
                        //await Task.Delay(75);
                    }
                }
            }
        }

        public static void SendDataToAllBut(int connectionID, byte[] data)
        {
            for (int i = 1; i < Constants.MAX_PLAYERS; i++)
            {
                if (clientObjects[i] != null)
                {
                    if (clientObjects[i].isConnected)
                    {
                        if (i != connectionID)
                        {
                            SendDataTo(i, data);
                            //await Task.Delay(75);
                        }
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
            //SendDataToAllBut(connectionID, buffer.ToArray());
            //SendDataToAll(buffer.ToArray());
            buffer.Dispose();
            for (int i = 1; i < ServerTCP.players.Length; i++)
            {
                if (ServerTCP.players[i].ConnectionID > 0 && i != connectionID)
                {
                    PACKET_SendOnlinePlayer(connectionID, i);
                    PACKET_SendOnlinePlayer(i, connectionID);
                }
            }
            
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
        public static void PACKET_SendOnlinePlayer(int connectionID, int playerID)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInteger((int)ServerPackages.SSendOnlinePlayer);

            buffer.WriteInteger(playerID);
            buffer.WriteFloat(players[playerID].PosX);
            buffer.WriteFloat(players[playerID].PosY);
            buffer.WriteFloat(players[playerID].Rotation);
            SendDataTo(connectionID, buffer.ToArray());
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
