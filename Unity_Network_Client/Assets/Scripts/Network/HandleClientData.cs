using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Assertions;

public class HandleClientData : MonoBehaviour
{
    private static Dictionary<string, GameObject> playerList = new Dictionary<string, GameObject>();
    public static void RecieveCallback(IAsyncResult AR)
    {
        Socket socket = (Socket)AR.AsyncState;
        try
        {
            int recieved = socket.EndReceive(AR);

            byte[] dataBuffer = new byte[recieved];
            ByteBuffer byteBuffer = new ByteBuffer();
            Array.Copy(NetworkManager.GetBuffer, dataBuffer, recieved);
            byteBuffer.WriteBytes(dataBuffer);

            // Data should start with an integer that's the ID of what we're sending
            int requestID = byteBuffer.ReadInteger();
            Debug.Log(requestID);
            UnityThread.executeInUpdate(() =>
            {
                HandleRequest(requestID, byteBuffer);
            });
            socket.BeginReceive(NetworkManager.GetBuffer, 0, NetworkManager.GetBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
        }
        catch (SocketException e)
        {
            Debug.Log($"Socket Exception: {e}");
        }
    }

    public static void HandleRequest(int requestID, ByteBuffer buffer)
    {
        switch (requestID)
        {
            case ((int)RequestIDs.Server_SendPlayerID):
                SetPlayerID(buffer);
                break;
            case ((int)RequestIDs.Server_SendOtherPlayer):
                AddNewPlayer(buffer);
                break;
            case ((int)RequestIDs.Server_SendExistingPlayer):
                Debug.Log("test");
                AddExistingPlayer(buffer);
                break;
            case ((int)RequestIDs.Server_SendMovement):
                HandlePlayerMovement(buffer);
                break;
            case ((int)RequestIDs.Server_SendDisconnect):
                Debug.Log("Gonna run dis");
                HandlePlayerDisconnect(buffer);
                break;
            case ((int)RequestIDs.Server_SendFalseRequest):
                string msg = buffer.ReadString();
                buffer.Dispose();
                Debug.Log(msg);
                break;
        }
    }

    public static void RequestPlayerID()
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)RequestIDs.Client_RequestPlayerID);
        NetworkManager.Send(buffer.ToArray());
        buffer.Dispose();
    }

    public static void SetPlayerID(ByteBuffer buffer)
    {
        string playerID = buffer.ReadString();
        Debug.Log($"Assigned playerID: {playerID}");
        NetworkPlayerManager.InstantiateNewPlayer(playerID);
        buffer.Dispose();

        RequestPlayersOnline();
    }

    public static void RequestPlayersOnline()
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)RequestIDs.Client_RequestPlayersOnline);

        NetworkManager.Send(buffer.ToArray());
        buffer.Dispose();
    }

    public static void SendPlayerMovement(float posX, float posY, float rotation)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)RequestIDs.Client_SendMovement);

        //Set three floats, x, y and then rotation
        buffer.WriteFloat(posX);
        buffer.WriteFloat(posY);
        buffer.WriteFloat(rotation);

        NetworkManager.Send(buffer.ToArray());
        buffer.Dispose();
    }

    public static void AddNewPlayer(ByteBuffer buffer)
    {
        string guid = buffer.ReadString();
        playerList.Add(guid, NetworkPlayerManager.InstantiateNewOtherPlayer(guid));
        buffer.Dispose();
    }

    public static void AddExistingPlayer(ByteBuffer buffer)
    {
        Debug.Log("addexistingoplayer");
        string guid = buffer.ReadString();
        float posX = buffer.ReadFloat();
        float posY = buffer.ReadFloat();
        float rotation = buffer.ReadFloat();

        playerList.Add(guid, NetworkPlayerManager.InstantiateNewOtherPlayer(guid, posX, posY, rotation));
        buffer.Dispose();
    }

    public static void HandlePlayerMovement(ByteBuffer buffer)
    {
        string id = buffer.ReadString();

        float posX = buffer.ReadFloat();
        float posY = buffer.ReadFloat();
        float rotation = buffer.ReadFloat();

        Transform transform = playerList[id].transform;

        transform.SetPositionAndRotation(new Vector3(posX, posY, transform.position.z), Quaternion.Euler(0, 0, rotation));
        buffer.Dispose();
    }

    public static void HandlePlayerDisconnect(ByteBuffer buffer)
    {
        string id = buffer.ReadString();
        GameObject go = playerList[id];
        playerList.Remove(id);
        Destroy(go);
        buffer.Dispose();
    }
}