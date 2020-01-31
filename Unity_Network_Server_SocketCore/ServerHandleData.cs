using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Unity_Network_Server_SocketCore
{
    public class ServerHandleData
    {
        public static Dictionary<int, Action<Socket, ByteBuffer>> packetList;

        public static void InitPacketList()
        {
            packetList = new Dictionary<int, Action<Socket, ByteBuffer>>();
            //Add server packets here
            packetList.Add((int)ClientPackages.Client_PingServer, HandlePingFromClient);
            packetList.Add((int)ClientPackages.Client_ReceiveMessageFromClient, HandleChatMessageFromClient);
            packetList.Add((int)ClientPackages.Client_RequestGuid, HandleRequestConnectionID);
            packetList.Add((int)ClientPackages.Client_RequestWorldPlayers, HandleRequestWorldPlayers);
            packetList.Add((int)ClientPackages.Client_SendMovement, HandleClientMovement);
            packetList.Add((int)ClientPackages.Client_SendProjectile, HandleNewProjectile);
            packetList.Add((int)ClientPackages.Client_SendProjectileHit, HandleProjectileHit);
            packetList.Add((int)ClientPackages.Client_SendPlayerGotHit, HandlePlayerGotHit);
            packetList.Add((int)ClientPackages.Client_SendPlayerData, HandlePlayerData);
        }

        public static void HandleData(ref Socket socket, ref byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packageID = buffer.ReadInteger();

            if (packetList.TryGetValue(packageID, out Action<Socket, ByteBuffer> packet))
            {
                packet.Invoke(socket, buffer);
            }
            buffer.Dispose();
        }

        private static void HandlePingFromClient(Socket socket, ByteBuffer data)
        {
            //Console.WriteLine($"connectionID '{ServerTCP._clientSockets[socket].connectionID}' did send a ping, lets send a ping back!");
            ServerTCP.PACKET_PingToClient(ref socket);
            data.Dispose();
        }

        private static void HandleChatMessageFromClient(Socket socket, ByteBuffer data)
        {
            Console.WriteLine("HandleChatMessageFromClient");

            // Second slot should be the message
            string message = data.ReadString();

            // Write out a line in the console
            Console.WriteLine($"Recieved message from id: {ServerTCP.clients[socket].id.ToString()}, message: {message}");

            // Call the function that broadcasts the message
            ServerTCP.PACKET_ChatmessageToClient(ref socket, message);

            // Dispose the buffer
            data.Dispose();
        }

        private static void HandleRequestConnectionID(Socket socket, ByteBuffer data)
        {
            ServerTCP.PACKET_SendGuid(ref socket);
            data.Dispose();
        }

        private static void HandleRequestWorldPlayers(Socket socket, ByteBuffer data)
        {
            ServerTCP.PACKET_SendWorldPlayersToNewPlayer(ref socket);
            ServerTCP.PACKET_SendNewPlayerToWorld(ref socket);
            data.Dispose();
        }

        private static void HandleClientMovement(Socket socket, ByteBuffer data)
        {
            float x = data.ReadFloat();
            float y = data.ReadFloat();
            float rotation = data.ReadFloat();


            // Set player position on server side 
            ServerTCP.clients[socket].player.SetPlayerPosition(x, y, rotation);

            // Send player position to all clients
            ServerTCP.PACKET_SendPlayerMovement(ref socket, x, y, rotation);
            data.Dispose();
        }

        private static void HandleNewProjectile(Socket socket, ByteBuffer data)
        {
            int bulletID = data.ReadInteger();

            ServerTCP.PACKET_ProjectileToClient(ref socket, bulletID);
            data.Dispose();
        }

        private static void HandleProjectileHit(Socket socket, ByteBuffer data)
        {
            int bulletID = data.ReadInteger();
            int length = data.ReadInteger();
            byte[] guidBytes = data.ReadBytes(length);
            Guid playerID = new Guid(guidBytes);    // "connectionID" of player that got hit

            Socket tempSocket = ServerTCP.GetSocketByGuid(playerID);
            ServerTCP.clients[tempSocket].player.BulletHitId = bulletID;
            data.Dispose();
        }
        private static void HandlePlayerGotHit(Socket socket, ByteBuffer data)
        {
            int length = data.ReadInteger();
            byte[] guidBytes = data.ReadBytes(length);
            Guid playerID = new Guid(guidBytes);
            int bulletID = data.ReadInteger();

            Socket tempSocket = ServerTCP.GetSocketByGuid(playerID);
            Player player = ServerTCP.clients[tempSocket].player;

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
                    Console.WriteLine($"playerID: {player.Id} died!");
                    //player = new Player(player.ConnectionID);
                    //TODO: send player is dead
                    player.ResetPlayerData();
                    ServerTCP.PACKET_SendPlayerDied(ref socket, player);
                }
            }
            data.Dispose();
        }
        private static void HandlePlayerData(Socket socket, ByteBuffer data)
        {

            string name = data.ReadString();
            int spriteID = data.ReadInteger();

            Player player = ServerTCP.clients[socket].player;
            player.Name = name;
            player.SpriteID = spriteID;

            ServerTCP.PACKET_SendPlayerHealth(ref socket, player.Health);

            //ServerTCP.PACKET_SendWorldPlayersToNewPlayer(ref socket);
            ServerTCP.PACKET_SendNewPlayerToWorld(ref socket);

            Console.WriteLine($"Player: '{name}' | '{player.Id}' joined the game!");
            data.Dispose();
        }
    }
}
