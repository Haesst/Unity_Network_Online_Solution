﻿using System;
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
        packetList.Add((int)ServerPackages.Server_SendRemoveProjectile, HandleRemoveProjectile);
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
        NetworkManager.instance.elapsedMsTime.Stop();
        NetworkManager.instance.pingMs.text = $"Ping: {NetworkManager.instance.elapsedMsTime.ElapsedMilliseconds}ms";
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
        new Projectile(NetPlayer.players[connectionID].transform, bulletID);
    }
    private static void HandleRemoveProjectile(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        int packageID = buffer.ReadInteger();
        int bulletID = buffer.ReadInteger();

        buffer.Dispose();

        NetPlayer.Destroy(NetPlayer.projectiles[bulletID]);
        NetPlayer.projectiles.Remove(bulletID);
    }
}