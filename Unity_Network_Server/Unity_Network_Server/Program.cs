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
            }
        }
    }
}
