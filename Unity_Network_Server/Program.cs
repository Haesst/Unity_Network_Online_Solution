using System;
using System.Threading;

namespace Unity_Network_Server
{
    class Program
    {
        private static Thread consoleThread;

        static void Main(string[] args)
        {
            ServerTCP.InitializeServer(); // Initialize the server
        }

        /// <summary>
        /// Initialize the console thread.
        /// </summary>
        public static void InitializeConsoleThread()
        {
            consoleThread = new Thread(ServerLoop); // Create a new thread with ServerLoop
            consoleThread.Name = "ConsoleThread";
            consoleThread.Start(); // Start the thread
        }

        private static void ServerLoop()
        {
            while (true)
            {
                string line = Console.ReadLine();

                if (line.Contains("/shutdown"))
                {
                    //save();
                    break;
                }
                //if (line.Contains("/broadcast"))
                //{
                //    //Split the message at the space
                //    string message = line.Split(' ')[1];
                //    //Call the function chatmessage to client with connectionID 0 since that's the server
                //    ServerTCP.PACKET_ChatmessageToClient(0, "SERVER BROADCAST: " + message);
                //}
            }
        }
    }
}
