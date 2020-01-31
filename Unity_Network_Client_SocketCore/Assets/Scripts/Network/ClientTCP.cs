using System;
using System.Net.Sockets;

public class ClientTCP
{
    private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private static byte[] buffer;

    public static void Connect(string host, int port)
    {
        //TODO: Make the client reconnect untill a connection is established.
        NetworkManager.isConnected = false;
        socket.Connect(host, port);
        buffer = new byte[4096 * 2];
        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);

        if (socket.Connected)
        {
            NetworkManager.isConnected = true;
            PACKAGE_PingToServer();
            PACKAGE_RequestGuid();
        }
    }
    private static void ReceiveCallback(IAsyncResult AR)
    {
        Socket socket = (Socket)AR.AsyncState;
        int received = socket.EndReceive(AR);
        byte[] dataBuffer = new byte[received];
        Array.Copy(buffer, dataBuffer, received);
        UnityThread.executeInFixedUpdate(() =>
        {
            ClientHandleData.HandleData(ref dataBuffer);
        });
        ClientTCP.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), ClientTCP.socket);
    }

    public static void SendData(ref ByteBuffer data)
    {
        socket.Send(data.ToArray());
        data.Dispose();
    }
    public static void PACKAGE_PingToServer()
    {
        NetworkManager.elapsedMsTime.Restart();
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_PingServer);

        SendData(ref buffer);
        buffer.Dispose();
    }

    public static void PACKAGE_BroadcastMsg(string msg)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_ReceiveMessageFromClient);

        buffer.Write(msg);

        SendData(ref buffer);
        buffer.Dispose();
    }

    public static void PACKAGE_RequestGuid()
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_RequestGuid);

        SendData(ref buffer);
        buffer.Dispose();
    }

    public static void PACKAGE_RequestWorldPlayers()
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_RequestWorldPlayers);

        SendData(ref buffer);
        buffer.Dispose();
    }

    public static void PACKAGE_SendMovement(float posX, float posY, float rotation)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_SendMovement);

        buffer.Write(posX);
        buffer.Write(posY);
        buffer.Write(rotation);

        SendData(ref buffer);
        buffer.Dispose();
    }

    public static void PACKAGE_SendProjectile(int bulletID)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_SendProjectile);
        buffer.Write(bulletID);
        SendData(ref buffer);
        buffer.Dispose();
    }
    public static void PACKAGE_SendProjectileHit(int bulletID, Guid playerID)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_SendProjectileHit);

        buffer.Write(bulletID);
        buffer.Write(playerID.ToByteArray().Length);
        buffer.Write(playerID.ToByteArray());

        SendData(ref buffer);
        buffer.Dispose();
    }
    public static void PACKAGE_SendPlayerGotHit(Guid id, int bulletID)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_SendPlayerGotHit);

        buffer.Write(id.ToByteArray().Length);
        buffer.Write(id.ToByteArray());
        buffer.Write(bulletID);

        SendData(ref buffer);
        buffer.Dispose();
    }

    public static void PACKAGE_SendPlayerData(Guid id)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_SendPlayerData);

        Player player = NetPlayer.players[id].GetComponent<Player>();
        buffer.Write(player.Name);
        buffer.Write(player.SpriteID);

        SendData(ref buffer);
        buffer.Dispose();
    }
}
