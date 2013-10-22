using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSBase.Communication;
using CSBase.Tools;
namespace CSWebInterface.HTTP.Sites
{
    class WindowsExplorer
    {
        public static List<TGM> RcvTgMs = new List<TGM>();
        public string BaseUri { get; private set; }
        private readonly Connector _con;
        private readonly HTMLSite _site;
        private TGM _responseTGM;
        private int _timeout = 0;
        public static string BaseSite = "";
        public WindowsExplorer(Connector aConnector, string aBaseUri)
        {
            BaseUri = aBaseUri;
            _con = aConnector;
            _site = new HTMLSite("File Browser");
        }

        private string RequestList(string aDir)
        {
            _timeout = 0;
            TGM tgmReq = new TGM((int) CSBase.Global.AppID.Windows,
                                 (int) CSBase.Commands.WindowsFunc.Commands.GetFileList, aDir);
            Logger.Log("Request list -> " + tgmReq, Logger.LogLevel.DBG);
            _con.QueueTGM(tgmReq);

            while (_timeout < 5000)
            {
                System.Threading.Thread.Sleep(10);
                _responseTGM = CheckList(tgmReq.ID);
                if (_responseTGM != null)
                {
                    Logger.Log("Rcv List -> Dir: " + aDir + " Timeout: " + _timeout, Logger.LogLevel.DBG);
                    break;
                }
                _timeout += 10;
            }
            if (_responseTGM != null)
            {
                BuildSite(_responseTGM.Data);
                RcvTgMs.Remove(_responseTGM);
                return _site.PrintPage();
            }
            else
            {
                Logger.Log("No response -> " + tgmReq + " Timeout: " + _timeout, Logger.LogLevel.WRN);
                return "Error";
            }

        }

        public string HandleCommand(string aCommand)
        {
            string com = aCommand.Substring(0, aCommand.IndexOf("_"));
            string param=  aCommand.Substring(aCommand.IndexOf("_")+1);
            Logger.Log("Handle Command -> Command:" +com + " Param: " + param,Logger.LogLevel.DBG);
            switch (com)
            {
                case "dir":
                    {
                        return RequestList(param.Replace("%20", " "));
                    }
                case "file":
                    {
                        OpenFile(param.Replace("%20", " "));
                        return String.Empty;
                    }
                default:
                    return String.Empty;
            }
        }
        private void OpenFile(string aFile)
        {
            TGM tgm = new TGM((int)CSBase.Global.AppID.Windows,(int)CSBase.Commands.WindowsFunc.Commands.DefaultOpen,aFile.Replace("%20"," "));
            _con.QueueTGM(tgm);
        }
        private void BuildSite(string[] aList)
        {
            Logger.Log("Build Site", Logger.LogLevel.DBG); 
            _site.Clear();
            string rootDir = aList[0].Replace(" ", "%20");
            if (rootDir[rootDir.Length - 1] != '/') rootDir+= "/";
            for (int i = 1; i < aList.Length; i++)
            {
                string file = aList[i].Replace(" ", "%20");
                string name = aList[i];
                if (file[0] == '1') _site.AddLink(name.Substring(1), BaseUri + "?dir_" + rootDir + file.Substring(1));
                else _site.AddLink(name.Substring(1), BaseUri + "?file_" + rootDir + file.Substring(1));
                _site.NextLine();
            }
            Logger.Log("Build completed -> Elements: " + (aList.Length-1), Logger.LogLevel.DBG); 
        }
        private TGM CheckList(int aID)
        {
            return RcvTgMs.FirstOrDefault(rcvTgM => rcvTgM.ID == aID);
        }
    }
}
