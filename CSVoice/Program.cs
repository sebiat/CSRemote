using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSBase.Communication;
using System.Reflection;
using System.IO;
using System.Threading;
using CSBase.Tools;


namespace CSVoice
{
    class Program
    {
        
        static void Main(string[] args)
        {
            File.Delete("debug.log");                                                           //to Delete
            Thread.CurrentThread.Name = "Main";
            Logger.Init();
            Logger.Log("Startup Application", Logger.LogLevel.DBG);
            Connector con = new Connector("localhost", CSBase.Global.CSPort, MethodBase.GetCurrentMethod().DeclaringType.Namespace,"Voice");
            VoiceHandler v = new VoiceHandler(con);

            while (true)
            {
                Console.Clear();
                string line = String.Format("{0,-21}{1,15}{2,15}", "CSServer IP", "Sended", "Received");
                WriteColorLine(line, ConsoleColor.DarkGreen);
                line = String.Format("{0,-21}{1,15}{2,15}", con.IP, con.SendedTGM, con.ReceivedTGM);
                Console.WriteLine(line);
                Console.WriteLine("\n");
                WriteColor("IsListening: ", ConsoleColor.DarkGreen);
                Console.WriteLine(v.IsListening.ToString());
                WriteColor("Last Command: ", ConsoleColor.DarkGreen);
                Console.WriteLine(v.LastCommand);
                System.Threading.Thread.Sleep(1000);
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
