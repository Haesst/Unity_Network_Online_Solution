// Copy and past to client/server if changes are made to this file!
public enum RequestIDs
{
    Server_SendFalseRequest = 0,
    Client_RequestPlayerID,
    Server_SendPlayerID,
    Client_SendMovement,
    Server_SendMovement,
    Server_SendOtherPlayer,
    Server_SendExistingPlayer,
    Server_SendDisconnect,
    Client_RequestPlayersOnline,
}


public enum ClientPackages
{
    CPingServer = 1,
    CBroadcastMessageToServer,
    CRequestConnectionID,
    CSendMovement,
}