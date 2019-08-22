using System;
using System.Net.Sockets;

public class ClientTCP
{
    private static TcpClient clientSocket;
    private static NetworkStream myStream;
    private static byte[] receiveBuffer;

    public static void InitializeClientSocket(string host, int port)
    {
        clientSocket = new TcpClient();
        clientSocket.ReceiveBufferSize = 4096;
        clientSocket.SendBufferSize = 4096;
        receiveBuffer = new byte[4096 * 2];
        clientSocket.BeginConnect(host, port, new AsyncCallback(ClientConnectCallback), clientSocket);
    }

    private static void ClientConnectCallback(IAsyncResult result)
    {
        clientSocket.EndConnect(result);

        if (clientSocket.Connected == false)
        {
            NetworkManager.instance.isConnected = false;
            UnityEngine.Debug.Log("Connection to the server was lost!");
            return;
        }
        else
        {
            myStream = clientSocket.GetStream();
            myStream.BeginRead(receiveBuffer, 0, 4096 * 2, ReceiveCallback, null);
            PACKAGE_PingToServer();
        }

    }

    private static void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            int readBytes = myStream.EndRead(result);
            if (readBytes <= 0)
            {
                return;
            }

            byte[] newBytes = new byte[readBytes];
            Buffer.BlockCopy(receiveBuffer, 0, newBytes, 0, readBytes);
            UnityThread.executeInUpdate(() =>
            {
                ClientHandleData.HandleData(newBytes);
            });
            myStream.BeginRead(receiveBuffer, 0, 4096 * 2, ReceiveCallback, null);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static void SendData(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
        buffer.WriteBytes(data);
        myStream.Write(buffer.ToArray(), 0, buffer.ToArray().Length);
        buffer.Dispose();
    }

    public static void PACKAGE_PingToServer()
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)ClientPackages.CPingServer);

        SendData(buffer.ToArray());
        buffer.Dispose();
    }

    public static void PACKAGE_BroadcastMsg(string msg)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)ClientPackages.CReceiveMessageFromClient);

        buffer.WriteString(msg);

        SendData(buffer.ToArray());
        buffer.Dispose();
    }

    public static void PACKAGE_RequestConnectionID()
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)ClientPackages.CRequestConnectionID);

        SendData(buffer.ToArray());
        buffer.Dispose();
    }

    public static void PACKAGE_RequestWorldPlayers(int spriteID)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)ClientPackages.CRequestWorldPlayers);

        //Sending the sprite to the server to keep track of it, and distribute it to the other players later on.
        buffer.WriteInteger(spriteID);

        SendData(buffer.ToArray());
        buffer.Dispose();
    }

    public static void PACKAGE_SendMovement(float posX, float posY, float rotation)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)ClientPackages.CSendMovement);

        buffer.WriteFloat(posX);
        buffer.WriteFloat(posY);
        buffer.WriteFloat(rotation);

        SendData(buffer.ToArray());
        buffer.Dispose();
    }

}