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
            PACKAGE_RequestGuid();
        }
    }
    private static void ReceiveCallback(IAsyncResult AR)
    {
        Socket socket = (Socket)AR.AsyncState;
        int received = socket.EndReceive(AR);
        byte[] dataBuffer = new byte[received];
        Array.Copy(buffer, dataBuffer, received);
        UnityThread.executeInUpdate(() =>
        {
            ClientHandleData.HandleData(dataBuffer);
        });
        ClientTCP.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), ClientTCP.socket);
    }

    public static void SendData(ByteBuffer data)
    {
        //NetworkManager.elapsedMsTime.Restart(); // Used to display the ping

        socket.Send(data.ToArray());
        data.Dispose();
    }
    public static void PACKAGE_PingToServer()
    {
        NetworkManager.elapsedMsTime.Restart();
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_PingServer);

        SendData(buffer);
    }

    public static void PACKAGE_Disconnect()
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_SendDisconnect);
        SendData(buffer);
    }

    public static void PACKAGE_RequestGuid()
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_RequestGuid);

        SendData(buffer);
    }

    public static void PACKAGE_SendPlayerData(Guid id)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_SendPlayerData);

        Player player = NetPlayer.players[id].GetComponent<Player>();
        buffer.Write(player.Name);
        buffer.Write(player.SpriteID);

        SendData(buffer);
    }

    public static void PACKAGE_RequestWorldPlayer(Guid id)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_RequestWorldPlayer);

        buffer.Write(id.ToByteArray().Length);
        buffer.Write(id.ToByteArray());

        SendData(buffer);
    }

    public static void PACKAGE_SendPlayerRotation(float rotation)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_SendPlayerRotation);
        buffer.Write(rotation);

        SendData(buffer);
    }

    public static void PACKAGE_SendProjectile(Guid bulletID, float roatation)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_SendProjectile);
        buffer.Write(bulletID.ToByteArray().Length);
        buffer.Write(bulletID.ToByteArray());
        buffer.Write(roatation);
        SendData(buffer);
    }
    public static void PACKAGE_SendProjectileHit(Guid bulletID, Guid ownerID, Guid playerID)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_SendProjectileHit);

        buffer.Write(bulletID.ToByteArray().Length);
        buffer.Write(bulletID.ToByteArray());
        buffer.Write(ownerID.ToByteArray().Length);
        buffer.Write(ownerID.ToByteArray());
        buffer.Write(playerID.ToByteArray().Length);
        buffer.Write(playerID.ToByteArray());

        SendData(buffer);
    }
    public static void PACKAGE_SendPlayerGotHit(Guid id, Guid bulletID)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_SendPlayerGotHit);

        buffer.Write(id.ToByteArray().Length);
        buffer.Write(id.ToByteArray());
        buffer.Write(bulletID.ToByteArray().Length);
        buffer.Write(bulletID.ToByteArray());

        SendData(buffer);
    }

    public static void PACKAGE_SendNewMovement(UnityEngine.Vector2 position, float rotation, float thrust)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write((int)ClientPackages.Client_SendNewMovement);

        buffer.Write(position.x);
        buffer.Write(position.y);
        buffer.Write(rotation);
        buffer.Write(thrust);

        SendData(buffer);
    }
}
