using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Assertions;

public class HandleClientData : MonoBehaviour
{
    private static Dictionary<string, GameObject> playerList = new Dictionary<string, GameObject>();
    
    /// <summary>
    /// This AsyncCallback is the one that's listening for messages on the server.
    /// </summary>
    /// <param name="AR">The IAsyncResult</param>
    public static void RecieveCallback(IAsyncResult AR)
    {
        Socket socket = (Socket)AR.AsyncState; // Take the socket from AsyncState
        try
        {
            int recieved = socket.EndReceive(AR); // End the recive, this returns the amound of data recived

            byte[] dataBuffer = new byte[recieved]; // Create a new byteArray with the size of the amount of data recieved
            ByteBuffer byteBuffer = new ByteBuffer(); // Create a new ByteBuffer
            Array.Copy(NetworkManager.GetBuffer, dataBuffer, recieved); // Copy the array from the main buffer to the databuffer with the number of rows equal to the data recieved
            byteBuffer.WriteBytes(dataBuffer); // Write the bytes to the byteBuffer.

            // Data should start with an integer that's the ID of what we're sending
            int requestID = byteBuffer.ReadInteger();

            UnityThread.executeInUpdate(() => // This is needed in order to let us instantiate GameObjects in Unity
            {
                HandleRequest(requestID, byteBuffer); // Call HandleRequest with the requestID and the byteBuffer
            });
            socket.BeginReceive(NetworkManager.GetBuffer, 0, NetworkManager.GetBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket); // Start listening for messages again
        }
        catch (SocketException e)
        {
            Debug.Log($"Socket Exception: {e}");
        }
    }

    /// <summary>
    /// HandleRequest checks the ID and calls the appropriate method depending on the request.
    /// </summary>
    /// <param name="requestID">The requestID</param>
    /// <param name="buffer">The buffer.</param>
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
            case ((int)RequestIDs.Server_SendExistingPlayers):
                AddExistingPlayers(buffer);
                break;
            case ((int)RequestIDs.Server_SendMovement):
                HandlePlayerMovement(buffer);
                break;
            case ((int)RequestIDs.Server_SendDisconnect):
                HandlePlayerDisconnect(buffer);
                break;
            case ((int)RequestIDs.Server_SendNewBullet):
                HandleNewBullet(buffer);
                break;
            case ((int)RequestIDs.Server_SendFalseRequest):
                string msg = buffer.ReadString();
                buffer.Dispose();
                Debug.Log(msg);
                break;
        }
    }

    /// <summary>
    /// RequestPlayerID sends a request to the server asking for an ID
    /// </summary>
    public static void RequestPlayerID()
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)RequestIDs.Client_RequestPlayerID);
        NetworkManager.Send(buffer.ToArray());
        buffer.Dispose();
    }

    /// <summary>
    /// Assign a player ID to our own ship
    /// </summary>
    /// <param name="buffer">Buffer containing the information we want.</param>
    public static void SetPlayerID(ByteBuffer buffer)
    {
        string playerID = buffer.ReadString(); // Read the playerID
        Debug.Log($"Assigned playerID: {playerID}");
        NetworkPlayerManager.InstantiateNewPlayer(playerID); // Call InstantiateNewPlayer from NetworkPlayerManager
        buffer.Dispose(); // Dispose of the buffer

        RequestPlayersOnline(); // Request other players online in the game
    }

    /// <summary>
    /// Send a request to the server to send out the players currently online.
    /// </summary>
    public static void RequestPlayersOnline()
    {
        ByteBuffer buffer = new ByteBuffer(); // Create a new buffer
        buffer.WriteInteger((int)RequestIDs.Client_RequestPlayersOnline); // Write the right RequestID

        NetworkManager.Send(buffer.ToArray()); // Send
        buffer.Dispose(); // Dispose of the data
    }

    /// <summary>
    /// Notify the server that we changed our position and or rotation
    /// </summary>
    /// <param name="posX">Our current position x</param>
    /// <param name="posY">Our current position y</param>
    /// <param name="rotation">Our current rotation</param>
    public static void SendPlayerMovement(float posX, float posY, float rotation)
    {
        ByteBuffer buffer = new ByteBuffer(); // Create a new ByteBuffer
        buffer.WriteInteger((int)RequestIDs.Client_SendMovement); // Write the requestID of sending movement

        //Set three floats, x, y and then rotation
        buffer.WriteFloat(posX);
        buffer.WriteFloat(posY);
        buffer.WriteFloat(rotation);

        NetworkManager.Send(buffer.ToArray()); // Send
        buffer.Dispose(); // Dispose of the data
    }

    /// <summary>
    /// This method adds another playercontrolled character that's not controlled by this player.
    /// </summary>
    /// <param name="buffer">The buffer</param>
    public static void AddNewPlayer(ByteBuffer buffer)
    {
        string guid = buffer.ReadString(); // Read the string with the players Guid
        playerList.Add(guid, NetworkPlayerManager.InstantiateNewOtherPlayer(guid)); // Add the player to our list with the right guid and the right GameObject(creating a player returns the gameobject)
        buffer.Dispose(); // Dispose of the data
    }

    /// <summary>
    /// AddExistingPlayer adds a player at a position
    /// </summary>
    /// <param name="buffer">The ByteBuffer</param>
    public static void AddExistingPlayers(ByteBuffer buffer)
    {
        int playerCount = buffer.ReadInteger();

        for(int i = 0; i < playerCount; i++)
        {
            string guid = buffer.ReadString(); // Read out the players guid
            float posX = buffer.ReadFloat(); // Read out the x position
            float posY = buffer.ReadFloat(); // Read out the y position
            float rotation = buffer.ReadFloat(); // Read out the rotation

            playerList.Add(guid, NetworkPlayerManager.InstantiateNewOtherPlayer(guid, posX, posY, rotation)); // Add our player to our list with the right guid and the right GameObject(creating a player returns the gameobject)
        }
        
        buffer.Dispose(); // Dispose of the data
    }

    /// <summary>
    /// Handle playermovement gets called when we get a message from the server that another player moved
    /// </summary>
    /// <param name="buffer">The ByteBuffer</param>
    public static void HandlePlayerMovement(ByteBuffer buffer)
    {
        string id = buffer.ReadString(); // Read out the id of the player that moved

        float posX = buffer.ReadFloat(); // Read out the x position
        float posY = buffer.ReadFloat(); // Read out the y position
        float rotation = buffer.ReadFloat(); // Read out the rotation

        Transform transform = null;

        if (playerList.ContainsKey(id))
        {
            transform = playerList[id]?.transform; // Get the transform of the right player in our list
        }

        if(transform != null) // Check so the transform is not null
        {
            transform.SetPositionAndRotation(new Vector3(posX, posY, transform.position.z), Quaternion.Euler(0, 0, rotation)); // Call set position and rotation with the sent values
        }
        buffer.Dispose(); // Dispose of the data
    }

    /// <summary>
    /// HandlePlayerDisconnect is a method to remove a player from our game.
    /// </summary>
    /// <param name="buffer">The ByteBuffer</param>
    public static void HandlePlayerDisconnect(ByteBuffer buffer)
    {
        string id = buffer.ReadString(); // Read out the id of the player that disconnected
        GameObject go = playerList[id]; // Get the gameObject
        playerList.Remove(id); // Remove the player from the list
        Destroy(go); // Destroy the GameObject
        buffer.Dispose(); // Dispose of the data
    }

    public static void SendNewBullet()
    {
        ByteBuffer buffer = new ByteBuffer(); // Create a new bytebuffer

        buffer.WriteInteger((int)RequestIDs.Client_NewBullet); // Write the requestID of sending movement

        NetworkManager.Send(buffer.ToArray()); // Send
        buffer.Dispose(); // Dispose of the data
    }

    public static void HandleNewBullet(ByteBuffer buffer)
    {
        string id = buffer.ReadString();
        GameObject shooter = playerList[id];

        GameObject go = Instantiate(Resources.Load("Prefabs/Bullet", typeof(GameObject)), shooter.transform.position + shooter.transform.up, shooter.transform.rotation) as GameObject;
        go.GetComponent<Bullet>().SetSource(shooter, shooter.transform.position + shooter.transform.up);

        buffer.Dispose();
    }

    public static void SendBulletHit(string bulletID, string playerID)
    {
        ByteBuffer buffer = new ByteBuffer();

        buffer.WriteInteger((int)RequestIDs.Client_BulletHit);
        buffer.WriteString(bulletID);
        buffer.WriteString(playerID);

        NetworkManager.Send(buffer.ToArray());

        buffer.Dispose();
    }

    public static void SendPlayerHit(string playerID, string bulletID)
    {
        ByteBuffer buffer = new ByteBuffer();

        buffer.WriteInteger((int)RequestIDs.Client_PlayerHit);
        buffer.WriteString(playerID);
        buffer.WriteString(bulletID);

        NetworkManager.Send(buffer.ToArray());

        buffer.Dispose();
    }
}