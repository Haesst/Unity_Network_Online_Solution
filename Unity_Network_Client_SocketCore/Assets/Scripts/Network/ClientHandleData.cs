using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandleData
{
    public static Dictionary<int, Action<ByteBuffer>> packetList;

    public static void InitializePacketList()
    {
        packetList = new Dictionary<int, Action<ByteBuffer>>();
        //Add server packets here
        packetList.Add((int)ServerPackages.Server_PingClient, HandlePingFromServer);
        packetList.Add((int)ServerPackages.Server_SendChatMessageClient, HandleChatMsgFromServer);
        packetList.Add((int)ServerPackages.Server_SendGuid, HandleRequestGuid);
        packetList.Add((int)ServerPackages.Server_SendNewPlayerToWorld, HandleNewPlayerToWorld);
        packetList.Add((int)ServerPackages.Server_SendWorldPlayersToNewPlayer, HandleWorldPlayersToNewPlayer);
        packetList.Add((int)ServerPackages.Server_SendPlayerMovement, HandlePlayerMovement);
        packetList.Add((int)ServerPackages.Server_SendRemovePlayer, HandleRemovePlayer);
        packetList.Add((int)ServerPackages.Server_SendNewProjectile, HandleNewProjectile);
        packetList.Add((int)ServerPackages.Server_SendPlayerHealth, HandlePlayerHealth);
        packetList.Add((int)ServerPackages.Server_SendPlayerDied, HandlePlayerDeath);
    }
    public static void HandleData(ref byte[] data)
    {
        Action<ByteBuffer> packet;
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write(data);
        int packageID = buffer.ReadInteger();

        if (packetList.TryGetValue(packageID, out packet))
        {
            packet.Invoke(buffer);
        }
        buffer.Dispose();
    }
    private static void HandlePingFromServer(ByteBuffer data)
    {

        // get the player amount each ping, should this be a different package?
        //NetPlayer.onlinePlayerCount = data.ReadInteger();

        NetworkManager.elapsedMsTime.Stop();
        //if (NetworkManager.pingMs.color != Color.white) { NetworkManager.pingMs.color = Color.white; }
        if (NetworkManager.elapsedMsTime.ElapsedMilliseconds >= 100) { NetworkManager.pingMs.color = Color.red; }
        else if (NetworkManager.elapsedMsTime.ElapsedMilliseconds > 40) { NetworkManager.pingMs.color = Color.yellow; }
        else if (NetworkManager.elapsedMsTime.ElapsedMilliseconds < 40) { NetworkManager.pingMs.color = Color.green; }
        NetworkManager.pingMs.text = $"Ping: {NetworkManager.elapsedMsTime.ElapsedMilliseconds}ms";
        //ClientTCP.PACKAGE_PingToServer();
        data.Dispose();

    }
    private static void HandleChatMsgFromServer(ByteBuffer data)
    {

        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);

        string message = data.ReadString();

        data.Dispose();

        //ChatText.instance.RecieveChatMessage(message, connectionID);
    }

    private static void HandleRequestGuid(ByteBuffer data)
    {

        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);

        NetPlayer.SetConnectionID(id);

        //Instantiate a new local player to the game
        NetPlayer.InstantiateNewPlayer(id);
        data.Dispose();
    }

    private static void HandleNewPlayerToWorld(ByteBuffer data)
    {

        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);
        int spriteID = data.ReadInteger();
        int health = data.ReadInteger();
        string name = data.ReadString();
        float x = data.ReadFloat();
        float y = data.ReadFloat();
        float rotation = data.ReadFloat();

        NetPlayer.instance.InstantiateNewPlayerAtPosition(id, x, y, rotation, name, spriteID);
        data.Dispose();
    }

    private static void HandleWorldPlayersToNewPlayer(ByteBuffer data)
    {
        int playersOnline = data.ReadInteger();
        for (int i = 0; i < playersOnline; i++)
        {
            int length = data.ReadInteger();
            byte[] guidBytes = data.ReadBytes(length);
            Guid id = new Guid(guidBytes);
            float x = data.ReadFloat();
            float y = data.ReadFloat();
            float rotation = data.ReadFloat();
            int spriteID = data.ReadInteger();
            string name = data.ReadString();
            NetPlayer.instance.InstantiateNewPlayerAtPosition(id, x, y, rotation, name, spriteID);

        }

        data.Dispose();
    }


    private static void HandlePlayerMovement(ByteBuffer data)
    {
        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);
        float posX = data.ReadFloat();
        float posY = data.ReadFloat();
        float rotation = data.ReadFloat();

        NetPlayer.players[id].transform.position = new Vector3(posX, posY, 0);
        NetPlayer.players[id].transform.rotation = Quaternion.Euler(0, 0, rotation);
        data.Dispose();
    }

    private static void HandleRemovePlayer(ByteBuffer data)
    {
        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);

        NetworkManager.Destroy(NetPlayer.players[id]);
        NetPlayer.players.Remove(id);
        data.Dispose();
    }

    private static void HandleNewProjectile(ByteBuffer data)
    {

        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);
        int bulletID = data.ReadInteger();

        NetPlayer.InstantiateNewProjectile(id, bulletID);
        data.Dispose();
    }

    private static void HandlePlayerHealth(ByteBuffer data)
    {

        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);
        int health = data.ReadInteger();


        if (id == NetPlayer.Id)
        {
            Player player = NetPlayer.players[id].GetComponent<Player>();
            player.Health = health;
            NetPlayer.healthText.text = $"Health: {health}";
        }
        data.Dispose();
    }

    private static void HandlePlayerDeath(ByteBuffer data)
    {
        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);
        float posX = data.ReadFloat();
        float posY = data.ReadFloat();
        float rotation = data.ReadFloat();
        int health = data.ReadInteger();


        GameObject player = NetPlayer.players[id];
        player.transform.position = new Vector3(posX, posY, player.transform.position.z);
        player.transform.rotation = new Quaternion(0, 0, rotation, 0);
        player.GetComponent<Player>().Health = health;
        NetPlayer.healthText.text = $"Health: {health}";
        data.Dispose();
    }
}
