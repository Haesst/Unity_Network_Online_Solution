using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Unity_Network_Server
{
    static class SocketExtensions
    {
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }
    }
    class ServerTCP
    {
        private static byte[] _buffer = new byte[1024 * 4]; // Create a buffer with max size
        private static Dictionary<Socket, Player> _clientSockets = new Dictionary<Socket, Player>(); // Create a dictionary with the sockets as key and a Player as value
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Create a server socket
        private static int port = 7171; // Port to listen to
        private static float version = 0.2f; // Current server version

        public static Socket GetSocket { get => _serverSocket; }
        public static void AddSocket(Socket socket) { _clientSockets.Add(socket, new Player(Guid.NewGuid())); }
        public static byte[] GetBuffer { get => _buffer; }
        public static List<Player> PlayerList { get => new List<Player>(_clientSockets.Values); } // Get a copy of the playerlist

        /// <summary>
        /// InitializeServer gets called when we start the server.
        /// It begins with changing the title of the console before binding the socket to accept
        /// all ip adresses on port 7171. It uses a backlog of 10.
        /// 
        /// It proceeds with beginning to accept new connections using the AsynCallback AcceptCallback in the file ServerHandleData
        /// before setting the header with a message displaying that the server is now running.
        /// 
        /// Lastly it calls InitializeConsoleThread() in Program that is listening to input and keeping the server online
        /// 
        /// </summary>
        public static void InitializeServer()
        {
            Console.Title = "Server"; // Set console title
            Console.WriteLine("Setting up server...");
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, port)); // Bind to accept all ip addresses on port 7171
            _serverSocket.Listen(10); // Begin listening. Backlog (the integer argument) is how many pending connections the server can have in a queue before denying requests
            _serverSocket.BeginAccept(new AsyncCallback(ServerHandleData.AcceptCallback), null); // Begin accepting connections using the AcceptCallback method in ServerHandleData
            SetHeader("Server is now running."); // Set header
            Program.InitializeConsoleThread(); // Run the console thread
        }

        /// <summary>
        /// Set the header without any message.
        /// </summary>
        public static void SetHeader()
        {
            Console.Clear();
            Console.WriteLine($"UNO SocketCore v.{version}");
            Console.WriteLine($"Currently {_clientSockets.Count} users connected.");
            Console.WriteLine("Connected players:");
            if(_clientSockets.Count <= 0)
            {
                Console.WriteLine("-");
            } else
            {
                foreach (var item in _clientSockets)
                {
                    Console.WriteLine($"{item.Value.ID} from {item.Key.RemoteEndPoint}");
                }
            }
        }

        /// <summary>
        /// Set the header with a message to display
        /// </summary>
        /// <param name="message">Message to show under the server header.</param>
        public static void SetHeader(string message)
        {
            SetHeader();
            Console.WriteLine($"Message: {message}");
        }

        /// <summary>
        /// Remove a socket from the list of connected players
        /// </summary>
        /// <param name="socket">Socket to remove</param>
        public static void RemoveSocket(ref Socket socket)
        {

            _clientSockets.Remove(socket);
        }

        /// <summary>
        /// Find a player by ID.
        /// </summary>
        /// <param name="guid">ID to search for.</param>
        /// <returns>Player with the ID given or null.</returns>
        public static Player FindPlayerById (string guid)
        {
            foreach (var item in _clientSockets)
            {
                if (item.Value.ID == guid)
                    return item.Value;
            }
            return null;
        }

        /// <summary>
        /// Find a player by socket.
        /// </summary>
        /// <param name="socket">Socket to search for.</param>
        /// <returns>Player with the socket given or null.</returns>
        public static Player FindPlayerBySocket(ref Socket socket)
        {
            return _clientSockets[socket];
        }

        /// <summary>
        /// Get a list of every player except the one that uses the socket given.
        /// </summary>
        /// <param name="socket">Socket of the player that we dont want in the list.</param>
        /// <returns>A list of players (List<Players>) where the player with the socket have been removed.</returns>
        public static List<Player> GetEveryPlayerExceptSocket(ref Socket socket)
        {
            List<Player> playerList = new List<Player>(_clientSockets.Values); // Copy the dictionary but use only the values since we want a list
            playerList.Remove(_clientSockets[socket]); // Remove the player with the socket

            return playerList;
        }

        /// <summary>
        /// Used when we want to broadcast something to every connected player.
        /// </summary>
        /// <param name="buffer">The byte[] that we want to send.</param>
        public static void SendToAll(byte[] buffer)
        {
            foreach (var item in _clientSockets)
            {
                item.Key.Send(buffer);
            }
        }

        /// <summary>
        /// Used when we want to send something to everyone except one player.
        /// </summary>
        /// <param name="socket">The socket that we don't want to send to.</param>
        /// <param name="buffer">The byte[] that we want to change.</param>
        public static void SendToAllBut(ref Socket socket, byte[] buffer)
        {
            List<Socket> playerList = new List<Socket>(_clientSockets.Keys); // Copy a list of sockets from the Dictionary keys

            playerList.Remove(socket); // Remove the socket that we don't want to send to

            foreach (var playerSocket in playerList)
            {
                playerSocket.Send(buffer);
            }
        }
    }
}
