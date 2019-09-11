using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Unity_Network_Server_SocketCore
{
    public class ServerHandleData
    {
        public static Dictionary<int, Action<Socket, byte[]>> packetList;

        public static void InitializePacketList()
        {
            packetList = new Dictionary<int, Action<Socket, byte[]>>();
            //Add server packets here
            packetList.Add((int)ClientPackages.Client_PingServer, HandlePingFromClient);
            packetList.Add((int)ClientPackages.Client_ReceiveMessageFromClient, HandleChatMessageFromClient);
            packetList.Add((int)ClientPackages.Client_RequestConnectionID, HandleRequestConnectionID);
            packetList.Add((int)ClientPackages.Client_RequestWorldPlayers, HandleRequestWorldPlayers);
            packetList.Add((int)ClientPackages.Client_SendMovement, HandleClientMovement);
            packetList.Add((int)ClientPackages.Client_SendProjectile, HandleNewProjectile);
            packetList.Add((int)ClientPackages.Client_SendProjectileHit, HandleProjectileHit);
            packetList.Add((int)ClientPackages.Client_SendPlayerGotHit, HandlePlayerGotHit);
        }

        public static void HandleData(Socket socket, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packageID = buffer.ReadInteger();
            buffer.Dispose();

            if (packetList.TryGetValue(packageID, out Action<Socket, byte[]> packet))
            {
                packet.Invoke(socket, data);
            }
        }

        private static void HandlePingFromClient(Socket socket, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packageID = buffer.ReadInteger();

            buffer.Dispose();

            //Console.WriteLine($"connectionID '{ServerTCP._clientSockets[socket].connectionID}' did send a ping, lets send a ping back!");
            ServerTCP.PACKET_PingToClient(ref socket);
        }

        private static void HandleChatMessageFromClient(Socket socket, byte[] data)
        {
            Console.WriteLine("HandleChatMessageFromClient");
            // Create a new buffer
            ByteBuffer buffer = new ByteBuffer();
            // Read data from package
            buffer.WriteBytes(data);

            // First slot should always be ID
            int packageID = buffer.ReadInteger();
            // Second slot should be the message
            string message = buffer.ReadString();

            // Dispose the buffer
            buffer.Dispose();

            // Write out a line in the console
            Console.WriteLine($"Recieved message from id: {ServerTCP._clientSockets[socket].connectionID}, message: {message}");

            // Call the function that broadcasts the message
            ServerTCP.PACKET_ChatmessageToClient(ref socket, message);
        }

        private static void HandleRequestConnectionID(Socket socket, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packageID = buffer.ReadInteger();

            buffer.Dispose();

            // Set the players connectionID on serverside then send the connectionID to the client
            ServerTCP._clientSockets[socket].player.ConnectionID = ServerTCP._clientSockets[socket].connectionID;
            ServerTCP.PACKET_SendConnectionID(ref socket);
        }

        private static void HandleRequestWorldPlayers(Socket socket, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packageID = buffer.ReadInteger();

            int spriteID = buffer.ReadInteger();

            buffer.Dispose();

            ServerTCP._clientSockets[socket].player.SpriteID = spriteID;
            ServerTCP.PACKET_SendWorldPlayersToNewPlayer(ref socket);
            ServerTCP.PACKET_SendNewPlayerToWorld(ref socket);
            ServerTCP.PACKET_SendPlayerHealth(ref socket, ServerTCP._clientSockets[socket].player.Health);
        }

        private static void HandleClientMovement(Socket socket, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packageID = buffer.ReadInteger();

            float x = buffer.ReadFloat();
            float y = buffer.ReadFloat();
            float rotation = buffer.ReadFloat();

            buffer.Dispose();

            // Set player position on server side 
            ServerTCP._clientSockets[socket].player.SetPlayerPosition(x, y, rotation);

            // Send player position to all clients
            ServerTCP.PACKET_SendPlayerMovement(ref socket, x, y, rotation);
        }

        private static void HandleNewProjectile(Socket socket, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packageID = buffer.ReadInteger();
            int bulletID = buffer.ReadInteger();

            buffer.Dispose();

            ServerTCP.PACKET_ProjectileToClient(ref socket, bulletID);
        }

        private static void HandleProjectileHit(Socket socket, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packageID = buffer.ReadInteger();
            int bulletID = buffer.ReadInteger();
            int playerID = buffer.ReadInteger();    // "connectionID" of player that got hit
            buffer.Dispose();

            Socket tempSocket = ServerTCP.GetSocketByConnectionID(playerID);
            ServerTCP._clientSockets[tempSocket].player.BulletHitId = bulletID;
        }
        private static void HandlePlayerGotHit(Socket socket, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packageID = buffer.ReadInteger();
            int playerID = buffer.ReadInteger();    // "connectionID" of player that got hit
            int bulletID = buffer.ReadInteger();
            buffer.Dispose();

            Socket tempSocket = ServerTCP.GetSocketByConnectionID(playerID);
            Player player = ServerTCP._clientSockets[tempSocket].player;

            if (player.BulletHitId == bulletID)
            {
                if (player.Health > 1)
                {
                    player.Health--;
                    ServerTCP.PACKET_SendPlayerHealth(ref socket, player.Health);
                    //Console.WriteLine($"playerID: {player.ConnectionID} got hit! HP: {player.Health}");
                }
                else
                {
                    Console.WriteLine($"playerID: {player.ConnectionID} died!");
                    player.ResetPlayerData();
                    
                    //TODO: send player is dead

                    ServerTCP.PACKET_SendPlayerDied(ref socket, player);
                }
            }
        }
    }
}
