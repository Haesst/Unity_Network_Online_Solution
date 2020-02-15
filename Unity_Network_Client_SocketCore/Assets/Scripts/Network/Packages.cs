// Copy and past to client/server if changes are made to this file!
public enum ServerPackages
{
    Server_PingClient = 1,
    Server_SendGuid,
    Server_SendNewPlayerToWorld,
    Server_SendPlayerRotation,
    Server_SendRemovePlayer,
    Server_SendNewProjectile,
    Server_SendPlayerHealth,
    Server_SendPlayerDied,
    Server_SendHighscore,
    Server_SendPlayerNewMovement,

}

public enum ClientPackages
{
    Client_PingServer = 1,
    Client_SendDisconnect,
    Client_RequestGuid,
    Client_SendPlayerData,
    Client_RequestWorldPlayer,
    Client_SendPlayerRotation,
    Client_SendProjectile,
    Client_SendProjectileHit,
    Client_SendPlayerGotHit,
    Client_SendNewMovement
}