using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandleData
{
    public static Dictionary<int, Action<ByteBuffer>> packetList;

    public static void InitializePacketList()
    {
        packetList = new Dictionary<int, Action<ByteBuffer>>();
        //Add server packets here
        packetList.Add((int)ServerPackages.Server_PingClient, HandlePingFromServer);
        packetList.Add((int)ServerPackages.Server_SendGuid, HandleRequestGuid);
        packetList.Add((int)ServerPackages.Server_SendNewPlayerToWorld, HandleNewPlayerToWorld);
        packetList.Add((int)ServerPackages.Server_SendPlayerRotation, HandlePlayerRotation);
        packetList.Add((int)ServerPackages.Server_SendRemovePlayer, HandleRemovePlayer);
        packetList.Add((int)ServerPackages.Server_SendNewProjectile, HandleNewProjectile);
        packetList.Add((int)ServerPackages.Server_SendPlayerHealth, HandlePlayerHealth);
        packetList.Add((int)ServerPackages.Server_SendPlayerDied, HandlePlayerDeath);
        packetList.Add((int)ServerPackages.Server_SendHighscore, HandleHighscore);
        packetList.Add((int)ServerPackages.Server_SendPlayerNewMovement, HandlePlayerNewMovement);
    }
    public static void HandleData(byte[] data)
    {
        Action<ByteBuffer> packet;
        ByteBuffer buffer = new ByteBuffer();
        buffer.Write(data);
        int packageID = buffer.ReadInteger();

        if (packetList.TryGetValue(packageID, out packet))
        {
            packet.Invoke(buffer);
        }
        buffer.Dispose();
        //DisplayPing();
    }

    private static void DisplayPing()
    {
        NetworkManager.elapsedMsTime.Stop();

        long ping = NetworkManager.elapsedMsTime.ElapsedMilliseconds;
        if (ping >= 150) { NetworkManager.pingMs.color = Color.red; }
        else if (ping >= 75) { NetworkManager.pingMs.color = new Color32(255, 100, 0, 255); }   // Orange Color
        else if (ping > 20) { NetworkManager.pingMs.color = Color.yellow; }
        else { NetworkManager.pingMs.color = Color.green; }

        NetworkManager.pingMs.text = $"Ping: {ping}ms";
    }
    private static void HandlePingFromServer(ByteBuffer data)
    {
        DisplayPing();
        //ClientTCP.PACKAGE_PingToServer();
    }

    private static void HandleRequestGuid(ByteBuffer data)
    {
        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);

        NetPlayer.SetGuid(id);
    }

    private static void HandleNewPlayerToWorld(ByteBuffer data)
    {
        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);
        int spriteID = data.ReadInteger();
        int health = data.ReadInteger();
        string name = data.ReadString();
        float x = data.ReadFloat();
        float y = data.ReadFloat();
        float rotation = data.ReadFloat();

        NetPlayer.InstantiateNewPlayer(id, spriteID, name, x, y, rotation);
    }

    private static void HandlePlayerRotation(ByteBuffer data)
    {
        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);
        if (!NetPlayer.players.ContainsKey(id))
        {
            ClientTCP.PACKAGE_RequestWorldPlayer(id);
            return;
        }
        float rotation = data.ReadFloat();

        GameObject player = NetPlayer.players[id];
        player.transform.rotation = Quaternion.Euler(0, 0, rotation);
    }

    private static void HandleRemovePlayer(ByteBuffer data)
    {
        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);

        NetworkManager.Destroy(NetPlayer.players[id]);
        NetPlayer.players.Remove(id);
    }

    private static void HandleNewProjectile(ByteBuffer data)
    {
        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);

        int bulletLength = data.ReadInteger();
        byte[] bulletGuidBytes = data.ReadBytes(bulletLength);
        Guid bulletID = new Guid(bulletGuidBytes);

        float rotation = data.ReadFloat();
        NetPlayer.players[id].transform.rotation = Quaternion.Euler(0, 0, rotation);

        NetPlayer.InstantiateNewProjectile(id, bulletID);
    }

    private static void HandlePlayerHealth(ByteBuffer data)
    {
        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);
        int health = data.ReadInteger();


        if (NetPlayer.Id == id)
        {
            Player player = NetPlayer.players[id].GetComponent<Player>();
            player.Health = health;
            //NetPlayer.healthText.text = $"Health: {health}";
            NetPlayer.healthBar.GetComponent<HealthBar>().SetSize(health * 0.1f);
        }
    }

    private static void HandlePlayerDeath(ByteBuffer data)
    {
        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);
        float posX = data.ReadFloat();
        float posY = data.ReadFloat();
        float rotation = data.ReadFloat();
        int health = data.ReadInteger();


        GameObject player = NetPlayer.players[id];
        player.transform.position = new Vector3(posX, posY, player.transform.position.z);
        player.transform.rotation = new Quaternion(0, 0, rotation, 0);
        player.GetComponent<Player>().Health = health;
        //NetPlayer.healthText.text = $"Health: {health}";
        NetPlayer.healthBar.GetComponent<HealthBar>().SetSize(health * 0.1f);

    }
    private static void HandleHighscore(ByteBuffer data)
    {
        int playerAmount = data.ReadInteger();
        for (int i = 0; i < playerAmount; i++)
        {
            string name = data.ReadString();
            int kills = data.ReadInteger();
            NetPlayer.instance.highscore[i].text = $"{i + 1}. {name} | {kills}";
        }

        if (playerAmount < NetPlayer.instance.highscore.Length)
        {
            for (int i = playerAmount + 1; i < NetPlayer.instance.highscore.Length; i++)
            {
                NetPlayer.instance.highscore[i].text = String.Empty;
            }
        }
    }

    private static void HandlePlayerNewMovement(ByteBuffer data)
    {
        int length = data.ReadInteger();
        byte[] guidBytes = data.ReadBytes(length);
        Guid id = new Guid(guidBytes);
        if (!NetPlayer.players.ContainsKey(id))
        {
            ClientTCP.PACKAGE_RequestWorldPlayer(id);
            return;
        }
        float posX = data.ReadFloat();
        float posY = data.ReadFloat();
        float rotation = data.ReadFloat();
        float thrust = data.ReadFloat();

        GameObject player = NetPlayer.players[id];
        Player playerData = player.GetComponent<Player>();
        player.transform.position = new Vector3(posX, posY, 0);
        player.transform.rotation = Quaternion.Euler(0, 0, rotation);
        playerData.rb.velocity = Vector3.ClampMagnitude(playerData.rb.velocity, 5f);
        playerData.rb.AddForce(player.transform.up * thrust);
    }
}
