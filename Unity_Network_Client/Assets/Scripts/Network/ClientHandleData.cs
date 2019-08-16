using System.Collections;
using System.Collections.Generic;
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

        // assign the connectionID to the PlayerInput class
        PlayerInput.instance.connectionID = connectionID;
        NetPlayer.SetConnectionID(connectionID);
    }
}
