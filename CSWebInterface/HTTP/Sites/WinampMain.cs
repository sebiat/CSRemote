using System;
using System.Threading;
using CSBase.Communication;
using CSBase.Tools;
namespace CSWebInterface.HTTP.Sites
{
    static class WinampMain
    {
        public static string BaseUri { get; private set; }
        private static Connector _con;
        private static HTMLSite _site;
        private static bool _valid;
        private static Thread _playlistPoll;
        private static string _currentSong = "NoSet";
        public static string SourceCode 
        {
            get
            {
                if (!_valid)
                {
                    Logger.Log("Site is invalid, wait 500ms", Logger.LogLevel.WRN);
                    Thread.Sleep(500);
                }
                if (!_valid)
                {
                    Logger.Log("Site is invalid", Logger.LogLevel.ERR);
                    return "Site is Invalid";
                }
                return _site.PrintPage();
            }
        }
        public static void Init(Connector aConnector, string aBaseUri)
        {
            BaseUri = aBaseUri;
            _con = aConnector;
            _site = new HTMLSite("Winamp");
            _valid = true;
            _playlistPoll=  new Thread(PlaylistPoll) {Name = "WinampPlaylistPoll"};
            _playlistPoll.Start();
            BuildBaseSite();
        }

        private static void RequestPlaylist()
        {
            TGM tgmReq = new TGM((int) CSBase.Global.AppID.Winamp, (int) CSBase.Commands.WinampFunc.Commands.GetPlaylist);
            _con.QueueTGM(tgmReq);
        }

        private static void PlaylistPoll()
        {
            while (true)
            {
                RequestPlaylist();
                Thread.Sleep(5000);
            }
        }

        public static void HandleCommand(string aCommand)
        {
            string[] param = aCommand.Split('_');
            switch (param[0])
            {
                case "wprev":
                    {
                        _con.QueueTGM(new TGM((int)CSBase.Global.AppID.Winamp,
                                              (int)CSBase.Commands.WinampFunc.Commands.PreviousTrack));     
                    }
                    break;
                case "wplay":
                    {
                        if (param.Length > 1)
                        {
                            _con.QueueTGM(new TGM((int) CSBase.Global.AppID.Winamp,
                                                  (int) CSBase.Commands.WinampFunc.Commands.PlayIndex,
                                                  param[1]));
                        }
                        else
                        {
                            _con.QueueTGM(new TGM((int) CSBase.Global.AppID.Winamp,
                                                  (int) CSBase.Commands.WinampFunc.Commands.Play));
                        }
                    }
                    break;
                case "wpause":
                    {
                        _con.QueueTGM(new TGM((int) CSBase.Global.AppID.Winamp,
                                              (int) CSBase.Commands.WinampFunc.Commands.Pause));
                    }
                    break;
                case "wstop":
                    {
                        _con.QueueTGM(new TGM((int)CSBase.Global.AppID.Winamp,
                                              (int)CSBase.Commands.WinampFunc.Commands.Stop));
                    }
                    break;
                case "wnext":
                    {
                        _con.QueueTGM(new TGM((int) CSBase.Global.AppID.Winamp,
                                              (int) CSBase.Commands.WinampFunc.Commands.NextTrack));
                    }
                    break;
            }
        }

        private static void BuildBaseSite()
        {
            _site.AddText(_currentSong);
            _site.NextLine();
            _site.NextLine();
            _site.AddButton("Prev",BaseUri+ "?wprev");
            _site.AddButton("Play", BaseUri + "?wplay");
            _site.AddButton("Pause", BaseUri + "?wpause");
            _site.AddButton("Stop", BaseUri + "?wstop");
            _site.AddButton("Next", BaseUri + "?wnext");
            _site.NextLine();
            _site.NextLine();
        }

        public static void UpdatePlaylist(string[] aSongs)
        {
            _valid = false;
            int playlistpos = Int32.Parse(aSongs[0]);
            if(aSongs.Length > 1)
                _currentSong = aSongs[playlistpos+1];
            else _currentSong = String.Empty;
            _site.Clear();
            BuildBaseSite();
            
            for (int i = 1; i < aSongs.Length; i++)  // Data[0] contains current playlist pos
            {
                if (i - 1 == playlistpos) _site.AddLink(aSongs[i], BaseUri + "?wplay_" + (i - 1).ToString(),"red");  
                else
                {
                    _site.AddLink(aSongs[i], BaseUri + "?wplay_" + (i - 1));  
                }
                _site.NextLine();
            }
            _valid = true;
        }

    }

    
}
