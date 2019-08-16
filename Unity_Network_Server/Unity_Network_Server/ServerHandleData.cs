using System;
using System.Collections.Generic;

namespace Unity_Network_Server
{
    public class ServerHandleData
    {
        public delegate void Packet_(int connectionID, byte[] data);
        public static Dictionary<int, Packet_> packetListener;
        private static int pLength;

        public static void InitializePacketListener()
        {
            packetListener = new Dictionary<int, Packet_>();
            //Add client packets here
            packetListener.Add((int)ClientPackages.CPingServer, HandlePingFromClient);
            packetListener.Add((int)ClientPackages.CReceiveMessageFromClient, HandleChatMessageFromClient);
        }

        public static void HandleData(int connectionID, byte[] data)
        {
            //Copying our packet information into a temporary array to edit and peek it.
            byte[] buffer = (byte[])data.Clone();

            //Checking if the connected player which did send this package has a instance of the bytebuffer
            //in order to read out the information of the byte[] buffer
            if (ServerTCP.clientObjects[connectionID].buffer == null)
            {
                //if there is no instance, then create a new instance
                ServerTCP.clientObjects[connectionID].buffer = new ByteBuffer();
            }

            //Reading out the package from the player in order to check which package it actually is
            ServerTCP.clientObjects[connectionID].buffer.WriteBytes(buffer);

            //Checking if the received package is empty, if so then do not contiune executing this code!
            if (ServerTCP.clientObjects[connectionID].buffer.Count() == 0)
            {
                ServerTCP.clientObjects[connectionID].buffer.Clear();
                return;
            }

            //Checking if the package actually contains information
            if (ServerTCP.clientObjects[connectionID].buffer.Length() >= 4)
            {
                //if so then read out the full package length
                pLength = ServerTCP.clientObjects[connectionID].buffer.ReadInteger(false);
                if (pLength <= 0)
                {
                    //if there is no package or package is invalid then close this method
                    ServerTCP.clientObjects[connectionID].buffer.Clear();
                    return;
                }
            }

            while (pLength > 0 & pLength <= ServerTCP.clientObjects[connectionID].buffer.Length() - 4)
            {
                if (pLength <= ServerTCP.clientObjects[connectionID].buffer.Length() - 4)
                {
                    ServerTCP.clientObjects[connectionID].buffer.ReadInteger();
                    data = ServerTCP.clientObjects[connectionID].buffer.ReadBytes(pLength);
                    HandleDataPackages(connectionID, data);
                }

                pLength = 0;
                if (ServerTCP.clientObjects[connectionID].buffer.Length() >= 4)
                {
                    pLength = ServerTCP.clientObjects[connectionID].buffer.ReadInteger(false);
                    if (pLength <= 0)
                    {
                        //if there is no package or package is invalid then close this method
                        ServerTCP.clientObjects[connectionID].buffer.Clear();
                        return;
                    }
                }

                if (pLength <= 1)
                {
                    ServerTCP.clientObjects[connectionID].buffer.Clear();
                }
            }
        }

        private static void HandleDataPackages(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packageID = buffer.ReadInteger();
            buffer.Dispose();

            if (packetListener.TryGetValue(packageID, out Packet_ packet))
            {
                packet.Invoke(connectionID, data);
            }
        }

        private static void HandlePingFromClient(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            int packageID = buffer.ReadInteger();

            buffer.Dispose();

            //Console.WriteLine(String.Format("connectionID '{0}' did send a ping, lets send a ping back!", connectionID));
            ServerTCP.PACKET_PingToClient(connectionID);
        }

        private static void HandleChatMessageFromClient(int connectionID, byte[] data)
        {
            Console.WriteLine("HandleChatMessageFromClient");
            // Create a new buffer
            ByteBuffer buffer = new ByteBuffer();
            // Read data from package
            buffer.WriteBytes(data);

            // First slot should always be ID
            int packageID = buffer.ReadInteger();
            // Second slot should be the message
            string message = buffer.ReadString();

            // Dispose the buffer
            buffer.Dispose();

            // Write out a line in the console
            Console.WriteLine($"Recieved message from id: {connectionID}, message: {message}");

            // Call the function that broadcasts the message
            ServerTCP.PACKET_ChatmessageToClient(connectionID, message);
        }
    }
}
