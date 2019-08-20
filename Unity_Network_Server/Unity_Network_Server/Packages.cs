// Copy and past to client/server if changes are made to this file!
public enum ServerPackages
{
    SPingClient = 1,
    SSendChatMessageClient,
    SSendConnectionID,
    SSendPlayerMovement,
    SSendOnlinePlayer
}

public enum ClientPackages
{
    CPingServer = 1,
    CReceiveMessageFromClient,
    CRequestConnectionID,
    CSendMovement,
}