using System;
using System.Net.Sockets;

public class ClientTCP
{
    private static Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private static byte[] _buffer;

    public static void Connect(string host, int port)
    {
        //TODO: Make the client reconnect untill a connection is established.
        _socket.Connect(host, port);
        _buffer = new byte[4096 * 2];
        _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), _socket);
        PACKAGE_PingToServer();
        /*
        while (!_clientSocket.Connected)
        {
            try
            {
                _clientSocket.Connect(host, port);
                break;
            }
            catch (SocketException)
            {
                UnityEngine.Debug.Log("No response from host, reconnection...");
            }
        }*/

    }
    private static void ReceiveCallback(IAsyncResult AR)
    {
        Socket socket = (Socket)AR.AsyncState;
        int received = socket.EndReceive(AR);
        byte[] dataBuffer = new byte[received];
        Array.Copy(_buffer, dataBuffer, received);
        UnityThread.executeInFixedUpdate(() =>
        {
            ClientHandleData.HandleData(dataBuffer);
        });
        _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), _socket);
    }

    public static void SendData(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        _socket.Send(buffer.ToArray());
        buffer.Dispose();
    }
    public static void PACKAGE_PingToServer()
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)ClientPackages.Client_PingServer);

        SendData(buffer.ToArray());
        buffer.Dispose();
        NetworkManager.elapsedMsTime.Restart();
    }

    public static void PACKAGE_BroadcastMsg(string msg)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)ClientPackages.Client_ReceiveMessageFromClient);

        buffer.WriteString(msg);

        SendData(buffer.ToArray());
        buffer.Dispose();
    }

    public static void PACKAGE_RequestConnectionID()
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)ClientPackages.Client_RequestConnectionID);

        SendData(buffer.ToArray());
        buffer.Dispose();
    }

    public static void PACKAGE_RequestWorldPlayers(int spriteID)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)ClientPackages.Client_RequestWorldPlayers);

        //Sending the sprite to the server to keep track of it, and distribute it to the other players later on.
        buffer.WriteInteger(spriteID);

        SendData(buffer.ToArray());
        buffer.Dispose();
    }

    public static void PACKAGE_SendMovement(float posX, float posY, float rotation)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)ClientPackages.Client_SendMovement);

        buffer.WriteFloat(posX);
        buffer.WriteFloat(posY);
        buffer.WriteFloat(rotation);

        SendData(buffer.ToArray());
        buffer.Dispose();
    }

    public static void PACKAGE_SendProjectile(int bulletID)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)ClientPackages.Client_SendProjectile);
        buffer.WriteInteger(bulletID);
        SendData(buffer.ToArray());
        buffer.Dispose();
    }
    public static void PACKAGE_SendProjectileHit(int bulletID, int playerID)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)ClientPackages.Client_SendProjectileHit);

        buffer.WriteInteger(bulletID);
        buffer.WriteInteger(playerID);

        SendData(buffer.ToArray());
        buffer.Dispose();
    }
    public static void PACKAGE_SendPlayerGotHit(int connectionID, int bulletID)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)ClientPackages.Client_SendPlayerGotHit);

        buffer.WriteInteger(connectionID);
        buffer.WriteInteger(bulletID);

        SendData(buffer.ToArray());
        buffer.Dispose();
    }

    public static void PACKAGE_SendPlayerData(int connectionID)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int)ClientPackages.Client_SendPlayerData);

        Player player = NetPlayer.players[connectionID].GetComponent<Player>();
        buffer.WriteString(player.Name);
        buffer.WriteInteger(player.SpriteID);

        SendData(buffer.ToArray());
        buffer.Dispose();
    }
}
