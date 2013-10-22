using System;
using System.Threading;
using System.IO;
using CSServer.Communication;
using CSBase.Tools;
using CSBase;

namespace CSServer
{
    class Program
    {
        static void Main()
        {
            File.Delete("debug.log");                                                           //to Delete
            Thread.CurrentThread.Name = "Main";
            Logger.Init();
            Logger.Log("Startup Application",Logger.LogLevel.DBG);
            Server.StartListening(Global.CSPort);

            while (true)
            {
                PrintServerStatus();
                Thread.Sleep(1000);
            }
        }
        public static void PrintServerStatus()
        {
            Console.Clear();
            if (Server.IsListening) WriteColorLine("Server Status: Listen for new Clients", ConsoleColor.DarkGreen);
            else WriteColorLine("Server Status: Not Listen for new Clients", ConsoleColor.DarkRed);
            Console.WriteLine("\n");

            string line = String.Format("{0,-20}{1,-21}{2,15}{3,15}", "Client", "IP", "Sended", "Received");
            WriteColorLine(line, ConsoleColor.DarkGreen);
            for (int i = 0; i < Server.Clients.Count; i++)
            {
                try
                {
                    line = String.Format("{0,-20}{1,-21}{2,15}{3,15}", Server.Clients[i].DeviceName,
                                         Server.Clients[i].IP, Server.Clients[i].SendedTGM,
                                         Server.Clients[i].ReceivedTGM);
                    Console.WriteLine(line);

                }
                catch
                {
                    Console.WriteLine(line);  
                }
            }


        }

        private static void WriteColorLine(string aLine, ConsoleColor aColor)
        {
            Console.ForegroundColor = aColor;
            Console.WriteLine(aLine);
            Console.ResetColor();
        }
    }
}
