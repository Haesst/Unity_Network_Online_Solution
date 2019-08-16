using System;
using System.Threading;

namespace Unity_Network_Server
{
    class Program
    {
        private static Thread consoleThread;

        static void Main(string[] args)
        {
            InitializeConsoleThread();
            ServerTCP.InitializeServer();
        }

        private static void InitializeConsoleThread()
        {
            consoleThread = new Thread(ConsoleLoop);
            consoleThread.Name = "ConsoleThread";
            consoleThread.Start();
        }

        private static void ConsoleLoop()
        {
            string line;
            //Console Loop
            while (true)
            {
                line = Console.ReadLine();

                if (line.Contains("/shutdown"))
                {
                    //save();
                    break;
                }
                if (line.Contains("/broadcast"))
                {
                    //Split the message at the space
                    string message = line.Split(' ')[1];
                    //Call the function chatmessage to client with connectionID 0 since that's the server
                    ServerTCP.PACKET_ChatmessageToClient(0, "SERVER BROADCAST: " + message);
                }
            }
        }
    }
}
