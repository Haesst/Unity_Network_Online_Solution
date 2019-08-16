using System;
using System.Net.Sockets;

namespace Unity_Network_Server
{
    public class ClientObject
    {
        public TcpClient socket;
        public NetworkStream myStream;
        public int connectionID;
        public byte[] receiveBuffer;
        public ByteBuffer buffer;
        public bool isConnected;

        public ClientObject(TcpClient _socket, int _connectionID)
        {
            if (_socket == null) return;

            socket = _socket;
            connectionID = _connectionID;

            socket.NoDelay = true;

            socket.ReceiveBufferSize = 4096;
            socket.SendBufferSize = 4096;

            myStream = socket.GetStream();

            receiveBuffer = new byte[4096];

            myStream.BeginRead(receiveBuffer, 0, socket.ReceiveBufferSize, ReceiveCallback, null);

            Console.WriteLine(String.Format("Incoming connection from {0} | connectionID: {1}", socket.Client.RemoteEndPoint.ToString(), connectionID));
            isConnected = true;
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int readBytes = myStream.EndRead(result);

                if (readBytes <= 0)
                {
                    CloseConnection();
                    return;
                }

                byte[] newBytes = new byte[readBytes];
                Buffer.BlockCopy(receiveBuffer, 0, newBytes, 0, readBytes);
                ServerHandleData.HandleData(connectionID, newBytes);
                myStream.BeginRead(receiveBuffer, 0, socket.ReceiveBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                CloseConnection();
                Console.WriteLine(ex.ToString());
                return;
            }
        }

        private void CloseConnection()
        {
            isConnected = false;
            Console.WriteLine(String.Format("User disconnected : {0} | connectionID: {1}", socket.Client.RemoteEndPoint.ToString(), connectionID));
            socket.Close();
            socket = null;
        }

    }
}
