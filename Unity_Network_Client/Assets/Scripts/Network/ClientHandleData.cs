using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ClientHandleData
{

    private static ByteBuffer playerBuffer;
    public delegate void Packet_(byte[] data);
    public static Dictionary<int, Packet_> packetListener;
    private static int pLength;

    public static void InitializePacketListener()
    {
        packetListener = new Dictionary<int, Packet_>();
        //Add server packets here
        packetListener.Add((int)ServerPackages.SPingClient, HandlePingFromServer);
        packetListener.Add((int)ServerPackages.SSendChatMessageClient, HandleChatMsgFromServer);
        packetListener.Add((int)ServerPackages.SSendConnectionID, HandleRequestConnectionID);
        packetListener.Add((int)ServerPackages.SSendNewPlayerToWorldPlayers, HandleNewPlayerToWorldPlayers);
        packetListener.Add((int)ServerPackages.SSendWorldPlayersToNewPlayer, HandleWorldPlayersToNewPlayer);
        packetListener.Add((int)ServerPackages.SSendPlayerMovement, HandlePlayerMovement);
        packetListener.Add((int)ServerPackages.SSendRemovePlayer, HandleRemovePlayer);

        
    }

    public static void HandleData(byte[] data)
    {
        //Copying out packet information into a temporary array to edit and peek it.
        byte[] buffer = (byte[])data.Clone();

        //Checking if the connected player which did send this package has a instance of the bytebuffer
        //in order to read out the information of the byte[] buffer
        if (playerBuffer == null)
        {
            //if there is no instance, then create a new instance
            playerBuffer = new ByteBuffer();
        }

        //Reading out the package from the player in order to check which package it actually is
        playerBuffer.WriteBytes(buffer);

        //Checking if the received package is empty, if so then do not contiune executing this code!
        if (playerBuffer.Count() == 0)
        {
            playerBuffer.Clear();
            return;
        }

        //Checking if the package actually contains information
        if (playerBuffer.Length() >= 4)
        {
            //if so then read out the full package length
            pLength = playerBuffer.ReadInteger(false);
            if (pLength <= 0)
            {
                //if there is no package or package is invalid then close this method
                playerBuffer.Clear();
                return;
            }
        }

        while (pLength > 0 & pLength <= playerBuffer.Length() - 4)
        {
            if (pLength <= playerBuffer.Length() - 4)
            {
                playerBuffer.ReadInteger();
                data = playerBuffer.ReadBytes(pLength);
                HandleDataPackages(data);
            }

            pLength = 0;
            if (playerBuffer.Length() >= 4)
            {
                pLength = playerBuffer.ReadInteger(false);
                if (pLength <= 0)
                {
                    //if there is no package or package is invalid then close this method
                    playerBuffer.Clear();
                    return;
                }
            }

            if (pLength <= 1)
            {
                playerBuffer.Clear();
            }
        }
    }

    private static void HandleDataPackages(byte[] data)
    {
        Packet_ packet;
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        int packageID = buffer.ReadInteger();

        if (packetListener.TryGetValue(packageID, out packet))
        {
            packet.Invoke(data);
        }
    }

    private static void HandlePingFromServer(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        int packageID = buffer.ReadInteger();

        buffer.Dispose();

        if (NetworkManager.instance.isConnected != true)
        {
            NetworkManager.instance.isConnected = true;
        }
        
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

        ChatText.instance.RecieveChatMessage(message, connectionID);
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

        //Note: change z value!!!
        NetPlayer.players[connectionID].transform.position = new Vector3(posX, posY, 0);
        NetPlayer.players[connectionID].transform.rotation = Quaternion.Euler(0, 0, rotation);
    }

    private static void HandleOnlinePlayer(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        int packageID = buffer.ReadInteger();

        int playerID = buffer.ReadInteger();
        float playerPosX = buffer.ReadFloat();
        float playerPosY = buffer.ReadFloat();
        float playerRotation = buffer.ReadFloat();

        buffer.Dispose();

        NetPlayer.instance.InstantiateNewPlayerAtPosition(playerID, playerPosX, playerPosY, playerRotation);
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
}
