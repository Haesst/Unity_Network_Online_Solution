﻿// Copy and past to client/server if changes are made to this file!
public enum ServerPackages
{
    Server_PingClient = 1,
    Server_SendChatMessageClient,
    Server_SendGuid,
    Server_SendNewPlayerToWorld,
    Server_SendWorldPlayersToNewPlayer,
    Server_SendPlayerMovement,
    Server_SendRemovePlayer,
    Server_SendNewProjectile,
    Server_SendPlayerHealth,
    Server_SendPlayerDied,

}

public enum ClientPackages
{
    Client_PingServer = 1,
    Client_ReceiveMessageFromClient,
    Client_RequestGuid,
    Client_RequestWorldPlayers,
    Client_SendMovement,
    Client_SendProjectile,
    Client_SendProjectileHit,
    Client_SendPlayerGotHit,
    Client_SendPlayerData,
}