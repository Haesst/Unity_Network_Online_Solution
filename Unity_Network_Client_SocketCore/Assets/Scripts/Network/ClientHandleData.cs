using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandleData
{
    public static Dictionary<int, Action<byte[]>> packetList;

    public static void InitializePacketList()
    {
        packetList = new Dictionary<int, Action<byte[]>>();
        //Add server packets here
        packetList.Add((int)ServerPackages.Server_PingClient, HandlePingFromServer);
        packetList.Add((int)ServerPackages.Server_SendChatMessageClient, HandleChatMsgFromServer);
        packetList.Add((int)ServerPackages.Server_SendConnectionID, HandleRequestConnectionID);
        packetList.Add((int)ServerPackages.Server_SendNewPlayerToWorldPlayers, HandleNewPlayerToWorldPlayers);
        packetList.Add((int)ServerPackages.Server_SendWorldPlayersToNewPlayer, HandleWorldPlayersToNewPlayer);
        packetList.Add((int)ServerPackages.Server_SendPlayerMovement, HandlePlayerMovement);
        packetList.Add((int)ServerPackages.Server_SendRemovePlayer, HandleRemovePlayer);
        packetList.Add((int)ServerPackages.Server_SendNewProjectile, HandleNewProjectile);
        packetList.Add((int)ServerPackages.Server_SendPlayerHealth, HandlePlayerHealth);
        packetList.Add((int)ServerPackages.Server_SendPlayerDied, HandlePlayerDeath);
    }
    public static void HandleData(byte[] data)
    {
        Action<byte[]> packet;
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        int packageID = buffer.ReadInteger();

        if (packetList.TryGetValue(packageID, out packet))
        {
            packet.Invoke(data);
        }
    }
    private static void HandlePingFromServer(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        int packageID = buffer.ReadInteger();

        // get the player amount each ping, should this be a different package?
        NetPlayer.onlinePlayerCount = buffer.ReadInteger();

        buffer.Dispose();

        if (NetworkManager.instance.isConnected == false)
        {
            NetworkManager.instance.isConnected = true;
        }
        NetworkManager.elapsedMsTime.Stop();
        //if (NetworkManager.pingMs.color != Color.white) { NetworkManager.pingMs.color = Color.white; }
        if (NetworkManager.elapsedMsTime.ElapsedMilliseconds >= 100) { NetworkManager.pingMs.color = Color.red; }
        if (NetworkManager.elapsedMsTime.ElapsedMilliseconds > 40) { NetworkManager.pingMs.color = Color.yellow; }
        if (NetworkManager.elapsedMsTime.ElapsedMilliseconds < 40) { NetworkManager.pingMs.color = Color.green; }
        NetworkManager.pingMs.text = $"Ping: {NetworkManager.elapsedMsTime.ElapsedMilliseconds}ms";
        //Debug.Log("You got a ping from the server, client is sending a ping back to the server.");
        ClientTCP.PACKAGE_PingToServer();

    }
    private static void HandleChatMsgFromServer(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        int packageID = buffer.ReadInteger();

        int connectionID = buffer.ReadInteger();

        string message = buffer.ReadString();

        buffer.Dispose();

        //ChatText.instance.RecieveChatMessage(message, connectionID);
    }

    private static void HandleRequestConnectionID(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        int packageID = buffer.ReadInteger();

        int connectionID = buffer.ReadInteger();
        buffer.Dispose();

        //Instantiate a new local player to the game
        NetPlayer.instance.InstantiateNewPlayer(connectionID, 0);

        //Assign the connectionID to different classes
        NetPlayer.SetConnectionID(connectionID);
        PlayerInput.instance.connectionID = connectionID;
    }

    private static void HandleNewPlayerToWorldPlayers(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        int packageID = buffer.ReadInteger();

        int connectionID = buffer.ReadInteger();
        int spriteID = buffer.ReadInteger();

        buffer.Dispose();

        NetPlayer.instance.InstantiateNewPlayer(connectionID, spriteID);
    }

    private static void HandleWorldPlayersToNewPlayer(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        int packageID = buffer.ReadInteger();

        int playersOnline = buffer.ReadInteger();
        for (int i = 0; i < playersOnline; i++)
        {
            int connectionID = buffer.ReadInteger();
            float x = buffer.ReadFloat();
            float y = buffer.ReadFloat();
            float rotation = buffer.ReadFloat();
            int spriteID = buffer.ReadInteger();
            NetPlayer.instance.InstantiateNewPlayerAtPosition(connectionID, x, y, rotation, spriteID);
        }

        buffer.Dispose();
    }


    private static void HandlePlayerMovement(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        int packageID = buffer.ReadInteger();

        int connectionID = buffer.ReadInteger();
        float posX = buffer.ReadFloat();
        float posY = buffer.ReadFloat();
        float rotation = buffer.ReadFloat();

        buffer.Dispose();

        NetPlayer.players[connectionID].transform.position = new Vector3(posX, posY, 0);
        NetPlayer.players[connectionID].transform.rotation = Quaternion.Euler(0, 0, rotation);
    }

    private static void HandleRemovePlayer(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        int packageID = buffer.ReadInteger();

        int connectionID = buffer.ReadInteger();

        buffer.Dispose();

        NetworkManager.Destroy(GameObject.Find($"Player | {connectionID}"));
        NetPlayer.players.Remove(connectionID);
    }

    private static void HandleNewProjectile(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        int packageID = buffer.ReadInteger();

        int connectionID = buffer.ReadInteger();
        int bulletID = buffer.ReadInteger();

        buffer.Dispose();
        NetPlayer.InstantiateNewProjectile(connectionID, bulletID);
    }

    private static void HandlePlayerHealth(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        int packageID = buffer.ReadInteger();

        int connectionID = buffer.ReadInteger();
        int health = buffer.ReadInteger();

        buffer.Dispose();

        if (connectionID == NetPlayer.connectionID)
        {
            Player player = NetPlayer.players[connectionID].GetComponent<Player>();
            player.Health = health;
            NetPlayer.healthText.text = $"Health: {health}";
        }
    }

    private static void HandlePlayerDeath(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        int packageID = buffer.ReadInteger();

        int connectionID = buffer.ReadInteger();
        float posX = buffer.ReadFloat();
        float posY = buffer.ReadFloat();
        float rotation = buffer.ReadFloat();
        int health = buffer.ReadInteger();

        buffer.Dispose();

        GameObject player = NetPlayer.players[connectionID];
        player.transform.position = new Vector3(posX, posY, player.transform.position.z);
        player.transform.rotation = new Quaternion(0, 0, rotation, 0);
        player.GetComponent<Player>().Health = health;
        NetPlayer.healthText.text = $"Health: {health}";
    }
}
