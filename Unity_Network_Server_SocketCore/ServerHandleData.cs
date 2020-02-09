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
            packetList.Add((int)ClientPackages.Client_SendDisconnect, HandleDisconnect);
            packetList.Add((int)ClientPackages.Client_RequestGuid, HandleRequestGuid);
            packetList.Add((int)ClientPackages.Client_SendPlayerData, HandlePlayerData);
            packetList.Add((int)ClientPackages.Client_RequestWorldPlayer, HandleRequestWorldPlayer);
            packetList.Add((int)ClientPackages.Client_SendMovement, HandleClientMovement);
            packetList.Add((int)ClientPackages.Client_SendProjectile, HandleNewProjectile);
            packetList.Add((int)ClientPackages.Client_SendProjectileHit, HandleProjectileHit);
            packetList.Add((int)ClientPackages.Client_SendPlayerGotHit, HandlePlayerGotHit);
        }

        public static void HandleData(Socket socket, byte[] data)
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
            ServerTCP.PACKET_PingToClient(socket);
        }

        private static void HandleDisconnect(Socket socket, ByteBuffer data)
        {
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                ServerTCP.clients[socket].CloseConnection();
            }
        }

        private static void HandleRequestGuid(Socket socket, ByteBuffer data)
        {
            ServerTCP.PACKET_SendGuid(socket);
        }

        private static void HandlePlayerData(Socket socket, ByteBuffer data)
        {
            string name = data.ReadString();
            int spriteID = data.ReadInteger();

            Player player = ServerTCP.clients[socket].player;
            player.Name = name;
            player.SpriteID = spriteID;

            ServerTCP.PACKET_SendNewPlayerToWorld(socket);
            Console.WriteLine($"Player: '{name}' | '{player.Id}' joined the game.");
            ServerTCP.PACKET_SendPlayerHealth(socket, player.Health);
        }

        private static void HandleRequestWorldPlayer(Socket socket, ByteBuffer data)
        {
            int length = data.ReadInteger();
            byte[] guidBytes = data.ReadBytes(length);
            Guid id = new Guid(guidBytes);

            ServerTCP.PACKET_SendNewPlayerToWorld(ServerTCP.GetSocketByGuid(id));
        }

        private static void HandleClientMovement(Socket socket, ByteBuffer data)
        {
            float x = data.ReadFloat();
            float y = data.ReadFloat();
            float rotation = data.ReadFloat();

            // Set player position on server side 
            ServerTCP.clients[socket].player.SetPlayerPosition(x, y, rotation);

            // Send player position to all clients
            ServerTCP.PACKET_SendPlayerMovement(socket, x, y, rotation);
        }

        private static void HandleNewProjectile(Socket socket, ByteBuffer data)
        {
            int length = data.ReadInteger();
            byte[] guidBytes = data.ReadBytes(length);
            Guid bulletID = new Guid(guidBytes);

            ServerTCP.PACKET_ProjectileToClient(socket, bulletID);
        }

        private static void HandleProjectileHit(Socket socket, ByteBuffer data)
        {
            int bulletLength = data.ReadInteger();
            byte[] bulletGuidBytes = data.ReadBytes(bulletLength);
            Guid bulletID = new Guid(bulletGuidBytes);

            int playerLength = data.ReadInteger();
            byte[] playerGuidBytes = data.ReadBytes(playerLength);
            Guid playerID = new Guid(playerGuidBytes);    // playerID of player that got hit

            Socket tempSocket = ServerTCP.GetSocketByGuid(playerID);
            ServerTCP.clients[tempSocket].player.BulletHitId = bulletID;
        }
        private static void HandlePlayerGotHit(Socket socket, ByteBuffer data)
        {
            int length = data.ReadInteger();
            byte[] guidBytes = data.ReadBytes(length);
            Guid playerID = new Guid(guidBytes);
            int bulletLength = data.ReadInteger();
            byte[] bulletGuidBytes = data.ReadBytes(bulletLength);
            Guid bulletID = new Guid(bulletGuidBytes);

            Socket tempSocket = ServerTCP.GetSocketByGuid(playerID);
            Player player = ServerTCP.clients[tempSocket].player;

            if (player.BulletHitId == bulletID)
            {
                if (player.Health > 1)
                {
                    player.Health--;
                    ServerTCP.PACKET_SendPlayerHealth(socket, player.Health);
                }
                else
                {
                    //Console.WriteLine($"playerID: {player.Id} died!");
                    player.ResetPlayerData();
                    ServerTCP.PACKET_SendPlayerDied(socket, player);
                }
            }
        }
    }
}
