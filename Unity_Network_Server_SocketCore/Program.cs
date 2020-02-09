using System;
using System.Threading;

namespace Unity_Network_Server_SocketCore
{
    class Program
    {
        private static Thread consoleThread;
        private static bool isRunning = false;
        static void Main(string[] args)
        {
            Console.Write("Initializing Server...");
            InitConsoleThread();
            ServerHandleData.InitPacketList();
            ServerTCP.InitTCP();

        }

        private static void InitConsoleThread()
        {
            try
            {
                consoleThread = new Thread(ConsoleLoop);
                isRunning = true;
                consoleThread.Name = "ConsoleThread";
                consoleThread.Start();
                Console.WriteLine("\tDone!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\tFailed\nError: {ex}");
                throw ex;
            }
        }

        private static void ConsoleLoop()
        {
            string line;
            while (isRunning)
            {
                line = Console.ReadLine();
                line.ToLower();
                if (line.Equals("/shutdown"))
                {
                    Console.WriteLine("Server is Shutting down!");
                    isRunning = false;
                    break;
                }
            }

        }
    }
}
