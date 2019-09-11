using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Unity_Network_Server
{
    public class ServerHandleData
    {
        /// <summary>
        /// This list is a list containing the ID's we want to print out in the server console for any reason.
        /// </summary>
        private static List<int> messagesToPrint = new List<int>()
        {
            (int)RequestIDs.Client_RequestPlayerID,
            (int)RequestIDs.Client_RequestPlayersOnline,
        };

        /// <summary>
        /// AcceptCallback is the function that accepts client connections on the socket.
        /// </summary>
        /// <param name="AR">AsyncResult</param>
        public static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = ServerTCP.GetSocket.EndAccept(AR); // End accept returns the socket that connected
            ServerTCP.AddSocket(socket); // Call add socket with the socket
            ServerTCP.SetHeader($"Client connected. ID: {socket.RemoteEndPoint}"); // Print out that a client connected. [[This can be removed since this gets overwritten before you can read it]]
            socket.BeginReceive(ServerTCP.GetBuffer, 0, ServerTCP.GetBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket); // Begin to recieve messages from the socket
            ServerTCP.GetSocket.BeginAccept(new AsyncCallback(AcceptCallback), null); // Begin accepting connections again with this function
        }

        /// <summary>
        /// RecieveCallback is the function that listens and recieves messages from a socket.
        /// </summary>
        /// <param name="AR">AsyncResult</param>
        private static void RecieveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState; // Get the socket from the AsyncResult.AsyncState
            if (socket.IsConnected()) // Check if the socket is connected. This is an extension that checks for disconnects as quickly as possible
            {
                try
                {
                    int recieved = socket.EndReceive(AR); // End the recieve. This returns the amount of data sent.

                    byte[] dataBuffer = new byte[recieved]; // Create a databuffer with the recieved amount of bytes so we don't have any null values
                    ByteBuffer byteBuffer = new ByteBuffer(); // Create a new ByteBuffer
                    Array.Copy(ServerTCP.GetBuffer, dataBuffer, recieved); // Copy the array from the serverbuffer to the databuffer with the amount of recieved values
                    byteBuffer.WriteBytes(dataBuffer); // Write the bytes to the databuffer.

                    // Data should start with an integer that's the ID of what we're sending
                    int requestID = byteBuffer.ReadInteger(); // The requestID that the client sent
                    if (messagesToPrint.Contains(requestID)) // Check if our list of id's we want to print to the header contains our id
                    {
                        ServerTCP.SetHeader($"We got the following id: {Enum.GetName(typeof(RequestIDs), requestID)}");
                    }

                    HandleRequest(requestID, ref socket, byteBuffer); // Forward the request, socket and buffer to handle it.

                    socket.BeginReceive(ServerTCP.GetBuffer, 0, ServerTCP.GetBuffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket); // Begin recieving again by calling this method
                }
                catch (SocketException e) // We need to catch socket exceptions
                {
                    ServerTCP.SetHeader($"SocketException: {e}"); // Print the exception to the header
                }
            }
            else
            {
                HandleDisconnect(ref socket); // Call handle disconnect with the socket.
            }
        }

        /// <summary>
        /// Cleanup the disconnected player
        /// </summary>
        /// <param name="socket">Socket of the disconnected player</param>
        private static void HandleDisconnect(ref Socket socket)
        {

            string id = ServerTCP.FindPlayerBySocket(ref socket).ID; // Get the id of the player
            SendDisconnect(ref socket, id); // Send out to every player that the user disconnects

            ServerTCP.RemoveSocket(ref socket); // Remove the socket from our list
            ServerTCP.SetHeader($"Client Disconnected: {socket.RemoteEndPoint}"); // Set the header with a message showing that a user disconnected
            socket.Close(); // Close the socket
        }

        /// <summary>
        /// Handle the request from a client
        /// </summary>
        /// <param name="requestID">ID of the request</param>
        /// <param name="socket">Socket that sent the request</param>
        /// <param name="buffer">The ByteBuffer with the byte[]</param>
        private static void HandleRequest(int requestID, ref Socket socket, ByteBuffer buffer)
        {
            switch (requestID)
            {
                case ((int)RequestIDs.Client_RequestPlayerID):
                    HandleRequestPlayerID(ref socket);
                    buffer.Dispose(); // We don't use the buffer so we need to dispose it.
                    break;
                case ((int)RequestIDs.Client_SendMovement):
                    HandlePlayerMovement(ref socket, buffer);
                    break;
                case ((int)RequestIDs.Client_RequestPlayersOnline):
                    HandlePlayersOnline(ref socket);
                    buffer.Dispose(); // We don't use the buffer so we need to dispose it.
                    break;
                case ((int)RequestIDs.Client_NewBullet):
                    HandleNewBullet(ref socket);
                    buffer.Dispose();
                    break;
                case ((int)RequestIDs.Client_BulletHit):
                    HandleBulletHit(buffer);
                    break;
                case ((int)RequestIDs.Client_PlayerHit):
                    HandlePlayerHit(buffer);
                    break;
                default:
                    HandleFalseRequest(ref socket, requestID);
                    buffer.Dispose(); // We don't use the buffer so we need to dispose it.
                    break;
            }
        }

        /// <summary>
        /// A method that get's called when a client requests a player id
        /// </summary>
        /// <param name="socket">Socket of the client</param>
        private static void HandleRequestPlayerID(ref Socket socket)
        {
            ByteBuffer buffer = new ByteBuffer(); // Create a new ByteBuffer
            buffer.WriteInteger((int)RequestIDs.Server_SendPlayerID); // Write the requestID first
            buffer.WriteString(ServerTCP.FindPlayerBySocket(ref socket).ID); // Write the ID as a string.
            socket.Send(buffer.ToArray()); // Send out the ID to the socket.
            buffer.Dispose(); // Dispose of the ByteBuffer

            ByteBuffer sendToOtherPlayers = new ByteBuffer(); // Create a new ByteBuffer since we want to send this to the existing players as well
            sendToOtherPlayers.WriteInteger((int)RequestIDs.Server_SendOtherPlayer); // Write the requestID
            sendToOtherPlayers.WriteString(ServerTCP.FindPlayerBySocket(ref socket).ID); // Write the players ID
            ServerTCP.SendToAllBut(ref socket, sendToOtherPlayers.ToArray()); // Call send to all but the socket given
            sendToOtherPlayers.Dispose(); // Dispose of the ByteBuffer
        }

        /// <summary>
        /// The client wants every player currently online(except themself)
        /// </summary>
        /// <param name="socket">Socket of the player that wants the list</param>
        private static void HandlePlayersOnline(ref Socket socket)
        {
            List<Player> playerList = new List<Player>(ServerTCP.GetEveryPlayerExceptSocket(ref socket));

            if (playerList.Count <= 0)
                return;

            ByteBuffer buffer = new ByteBuffer(); // Create a new ByteBuffer
            buffer.WriteInteger((int)RequestIDs.Server_SendExistingPlayers); // Write the requestID

            buffer.WriteInteger(playerList.Count); // Write the number of players online
            

            foreach (var player in playerList)
            {

                buffer.WriteString(player.ID); // Write the players ID
                buffer.WriteFloat(player.PosX); // Write the X Position
                buffer.WriteFloat(player.PosY); // Write the Y Position
                buffer.WriteFloat(player.Rotation); // Write the rotation

            }

            socket.Send(buffer.ToArray()); // Send the data to the player
            buffer.Dispose(); // Clean up the buffer
        }

        /// <summary>
        /// Handle player movement. This is an early version of recieving and manage the playermovement
        /// </summary>
        /// <param name="socket">Socket of the player that moved.</param>
        /// <param name="byteBuffer">The ByteBuffer containing the information.</param>
        private static void HandlePlayerMovement(ref Socket socket, ByteBuffer byteBuffer)
        {
            // Move Player
            float posX = byteBuffer.ReadFloat(); // X position comes first
            float posY = byteBuffer.ReadFloat(); // Y position comes second
            float rotation = byteBuffer.ReadFloat(); // Rotatino third
            byteBuffer.Dispose(); // Dispose the data

            ServerTCP.FindPlayerBySocket(ref socket).SetPlayerPosition(posX, posY, rotation); // Update the players position with the new information

            ByteBuffer buffer = new ByteBuffer(); // Create a new ByteBuffer since we want to send the movement update to the other players
            buffer.WriteInteger((int)RequestIDs.Server_SendMovement); // Write the requestID
            buffer.WriteString(ServerTCP.FindPlayerBySocket(ref socket).ID); // Write the ID of the player that moved

            buffer.WriteFloat(posX); // Write the X position first
            buffer.WriteFloat(posY); // Write the Y position second
            buffer.WriteFloat(rotation); // Write the rotation third.

            ServerTCP.SendToAllBut(ref socket, buffer.ToArray()); // Send the playermovement to everyone but the socket
            buffer.Dispose(); // Dispose the data
        }

        /// <summary>
        /// When an ID that's not in use are given from thye client we send the information back to the client
        /// with a message that the ID is not in use.
        /// </summary>
        /// <param name="socket">Socket that sent the request.</param>
        /// <param name="requestID">requestID given.</param>
        private static void HandleFalseRequest(ref Socket socket, int requestID)
        {
            ByteBuffer buffer = new ByteBuffer(); // Create a new buffer
            buffer.WriteInteger((int)RequestIDs.Server_SendFalseRequest); // Write the requestID
            buffer.WriteString($"The request with id [ {requestID} ] doesn't exist."); // Write a message.
            socket.Send(buffer.ToArray()); // Send the buffer
            buffer.Dispose(); // Dispose of the buffer
        }

        /// <summary>
        /// Send out a disconnect from the server that tells the other clients to remove the player
        /// </summary>
        /// <param name="socket">Socket that disconnected.</param>
        /// <param name="id">ID of the disconnected user.</param>
        private static void SendDisconnect(ref Socket socket, string id)
        {
            ByteBuffer buffer = new ByteBuffer(); // Create a new buffer

            buffer.WriteInteger((int)RequestIDs.Server_SendDisconnect); // Write the requestID
            buffer.WriteString(id); // Write the ID of the player that disconnected

            ServerTCP.SendToAllBut(ref socket, buffer.ToArray()); // Send this information to everyone except the socket that disconnected
            buffer.Dispose(); // Dispose of the buffer
        }

        private static void HandleNewBullet(ref Socket socket)
        {
            ByteBuffer buffer = new ByteBuffer();

            buffer.WriteInteger((int)RequestIDs.Server_SendNewBullet);
            buffer.WriteString(ServerTCP.FindPlayerBySocket(ref socket).ID);

            ServerTCP.SendToAllBut(ref socket, buffer.ToArray());
            buffer.Dispose();
        }

        private static void HandleBulletHit(ByteBuffer buffer)
        {
            //
            buffer.Dispose();
        }

        private static void HandlePlayerHit(ByteBuffer buffer)
        {
            //
            buffer.Dispose();
        }
    }
}
