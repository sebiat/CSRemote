using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections;
using CSBase.Communication;
using CSBase.Tools;
using CSWebInterface.HTTP.Sites;

namespace CSWebInterface.HTTP
{
    static class HTTPServer
    {
        public static int Requests { get; private set; }
        public static int Commands { get; private set; }
        private static HttpListener _listener;
        private static Thread _listenerThread;
        private static CSBase.Communication.Connector _con;
        public static void StartServer(int aPort , CSBase.Communication.Connector aCon)
        {
            Logger.Log("Init Webserver -> Port:" + aPort, Logger.LogLevel.DBG);
            _listener = new HttpListener();
            _listener.Prefixes.Add(@"http://*:" + aPort + @"/");
            _listener.Start();
            _listenerThread = new Thread(HandleRequests) {Name = "HTTPListener"};
            _listenerThread.Start();
            _con = aCon;
            WinampMain.Init(aCon,"winamp");
            Logger.Log("Webserver started", Logger.LogLevel.DBG);
        }
        private static void HandleRequests()
        {
            while (_listener.IsListening)
            {
                ThreadPool.QueueUserWorkItem(ClientRequest, _listener.GetContext());
                Thread.Sleep(1);
            }
        }

        private static void ClientRequest(object o)
        {
            Requests++;
            var context = o as HttpListenerContext;
            if (Thread.CurrentThread.Name == null) Thread.CurrentThread.Name = "HTTPResponse<" + context.Request.RemoteEndPoint + ">";
            Logger.Log("New HTTP Request -> Uri:" + context.Request.RawUrl,Logger.LogLevel.DBG);
            string site="", command="",  responsString="";
            PharseUri(context.Request.RawUrl.ToLower(), out site, out command);
            Logger.Log("HTTP Request -> Site:" + site + " Command:" + command, Logger.LogLevel.DBG);

            if (String.IsNullOrEmpty(command))
            {
                switch (site)
                {
                    case "winamp":
                        {
                            responsString = Sites.WinampMain.SourceCode;
                        }
                        break;
                    case "win":
                        {
                            responsString = Sites.WindowsExplorer.BaseSite;
                        }
                        break;
                    case "tgm":
                        {
                            responsString = "TGM http pass through";
                        }
                        break;
                    default:
                        {
                            responsString = "Unknow Site";
                            Logger.Log("Unknow Site Request -> Site: " + site,Logger.LogLevel.DBG);
                        }
                        break;
                }
                Respond(context, responsString);
            }
            else
            {
                Commands++;
                switch (site)
                {
                    case "winamp":
                        {
                            WinampMain.HandleCommand(command);
                            Navigate(context, site);
                        }
                        break;
                    case "win":
                        {
                            WindowsExplorer expl = new WindowsExplorer(_con, "win");
                            responsString = expl.HandleCommand(command);
                            if (responsString == String.Empty) NavigateBack(context);
                            else Respond(context, responsString);

                        }
                        break;
                    case "tgm":
                        {
                            Respond(context, HandePassThrough(command) ? "true" : "false");
                        }
                        break;
                    default:
                        {
                            Logger.Log("Unknow Site Request -> Site: " + site, Logger.LogLevel.DBG);
                        }
                        break;
                }
            }
        }

        private static bool HandePassThrough(string acommand)
        {
            const int headerlen = 2;
            acommand = acommand.Replace("%3c", "<");
            acommand = acommand.Replace("%3c", ">");
            acommand = acommand.Replace("%3E", "<");
            acommand = acommand.Replace("%3e", ">");
            try
            {
                string[] splitedRaw = Regex.Split(acommand.Substring(0, acommand.Length), TGM.TGMDelimter);
                var tmpAppID = Int32.Parse(splitedRaw[0]);
                var tmpComID = Int32.Parse(splitedRaw[1]);
                var tmpAppIDData = new string[splitedRaw.Length - headerlen];
                Array.Copy(splitedRaw, headerlen, tmpAppIDData, 0, splitedRaw.Length - headerlen);
                TGM TGMPT = new TGM(tmpAppID, tmpComID, tmpAppIDData);
                _con.QueueTGM(TGMPT);
                Logger.Log("Pass command to Server:" + acommand , Logger.LogLevel.DBG);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Cant pass command to Server:" + acommand + " Excetion:" + ex.ToString(), Logger.LogLevel.ERR);
            }
            return false;

        }
        private static void Navigate(HttpListenerContext aClient, string aUri)
        {
            string responsString = "<script language=\"javascript\"> window.location.href = \"" + aUri + "\"</script>";
            Respond(aClient, responsString);
        }
        private static void NavigateBack(HttpListenerContext aClient)
        {
            const string responsString = "<script language=\"javascript\"> history.back(); </script>";
            Respond(aClient, responsString);
        }

        private static void PharseUri(string aUri, out string aSite, out string aCom)
        {
            string tmpSite="", tmpCom="";
            aUri = aUri.Substring(1); // Remove / at begin of Uri
            if (aUri.Contains("?"))
            {
                try
                {
                    tmpSite = aUri.Substring(0, aUri.IndexOf('?'));
                }
                catch (Exception ex)
                {
                    Logger.Log("Can't pharse Site -> Uri:" + aUri + " Excetion:" + ex.ToString(), Logger.LogLevel.ERR);
                }
                try
                {
                    tmpCom = aUri.Substring(aUri.IndexOf('?')+1);
                }
                catch (Exception ex)
                {
                    Logger.Log("Can't pharse Command -> Uri:" + aUri + " Exception:" +ex, Logger.LogLevel.WRN);
                }  
            }
            else
            {
                tmpSite = aUri;
                tmpCom = "";
            }
            
            aSite = tmpSite;
            aCom = tmpCom;
        }
        private static void Respond(HttpListenerContext aClient, string aSource)
        {
            Logger.Log("Send data to Client -> Datalen:" + aSource.Length,Logger.LogLevel.DBG);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(aSource);
            aClient.Response.ContentLength64 = buffer.Length;
            System.IO.Stream output = aClient.Response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Flush();
            output.Dispose();
            Logger.Log("Send data to Client successful ", Logger.LogLevel.DBG);
        }
    }
}
