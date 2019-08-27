using System;
using System.Threading;

namespace Unity_Network_Server_SocketCore
{
    class Program
    {
        private static Thread consoleThread;
        static void Main(string[] args)
        {
            InitConsoleThread();
            ServerHandleData.InitializePacketList();
            ServerTCP.SetupServer();
        }
        private static void InitConsoleThread()
        {
            consoleThread = new Thread(ConsoleLoop);
            consoleThread.Name = "ConsoleThread";
            consoleThread.Start();
        }
        private static void ConsoleLoop()
        {
            string line;
            while (true)
            {
                line = Console.ReadLine();
                line.ToLower();
                if (line.Equals("/shutdown"))
                {
                    break;
                }
            }
            
        }
    }
}
