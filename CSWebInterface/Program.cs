using System;
using CSBase.Tools;
using CSBase.Communication;
using System.Reflection;
using System.IO;
using System.Threading;
using CSWebInterface.HTTP;

namespace CSWebInterface
{
    class Program
    {
        private static void Main(string[] args)
        {
            File.Delete("debug.log"); //to Delete
            Thread.CurrentThread.Name = "Main";
            Logger.Init();
            Logger.Log("Startup Application", Logger.LogLevel.DBG);

            Connector con = new Connector("localhost", CSBase.Global.CSPort,
                                          MethodBase.GetCurrentMethod().DeclaringType.Namespace, "WebInterface");
            HTTPServer.StartServer(10102, con);

            while (true)
            {
                Console.Clear();
                string line = String.Format("{0,-21}{1,15}{2,15}", "CSServer IP", "Sended", "Received");
                WriteColorLine(line, ConsoleColor.DarkGreen);
                line = String.Format("{0,-21}{1,15}{2,15}", con.IP, con.SendedTGM, con.ReceivedTGM);
                Console.WriteLine(line);
                Console.WriteLine("\n");
                WriteColor("HTTP Requests: ", ConsoleColor.DarkGreen);
                Console.WriteLine(HTTPServer.Requests);
                WriteColor("HTTP Commands: ", ConsoleColor.DarkGreen);
                Console.WriteLine(HTTPServer.Commands);
                Thread.Sleep(1000);
            }
        }

        private static void WriteColorLine(string aLine, ConsoleColor aColor)
        {
            Console.ForegroundColor = aColor;
            Console.WriteLine(aLine);
            Console.ResetColor();
        }
        private static void WriteColor(string aLine, ConsoleColor aColor)
        {
            Console.ForegroundColor = aColor;
            Console.Write(aLine);
            Console.ResetColor();
        }
    }
}
