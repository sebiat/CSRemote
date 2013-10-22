using System;
using System.Reflection;
using CSBase.Tools;
using CSBase.Communication;
using CSBase;
using CSServer.Commands.Winamp;

namespace CSServer.Commands
{
    class WinampFunc
    {
        internal static bool PassCommand(TGM aTGM)
        {
            return Global.InvokeStringMethod(MethodBase.GetCurrentMethod().DeclaringType.FullName, ((CSBase.Commands.WinampFunc.Commands)aTGM.ComID).ToString(), aTGM);
        }
        internal static void Play(TGM aTGM)
        {
            Logger.Log("Start Playing -> " + aTGM, Logger.LogLevel.DBG);
            WinampLib.Play();
        }
        internal static void Stop(TGM aTGM)
        {
            Logger.Log("Stop Playing -> " + aTGM, Logger.LogLevel.DBG);
            WinampLib.Stop();
        }
        internal static void Pause(TGM aTGM)
        {
            Logger.Log("Pause Playing -> " + aTGM, Logger.LogLevel.DBG);
            WinampLib.Pause();
        }
        internal static void NextTrack(TGM aTGM)
        {
            Logger.Log("Play next Song -> " + aTGM, Logger.LogLevel.DBG);
            WinampLib.NextTrack();
        }
        internal static void PreviousTrack(TGM aTGM)
        {
            Logger.Log("Play Prev Song -> " + aTGM, Logger.LogLevel.DBG);
            WinampLib.PrevTrack();
        }
        internal static void GetPlaylist(TGM aTGM)
        {
            Logger.Log("Getting Playlist -> " + aTGM, Logger.LogLevel.DBG);
            string[] playlist = WinampLib.GetPlaylist();
            string[] fullPlaylist = {WinampLib.GetPlaylistPosition().ToString()};
            Array.Resize(ref fullPlaylist, playlist.Length + 1);
            playlist.CopyTo(fullPlaylist, 1);
            Logger.Log("Found " + playlist.Length +" Tracks in Playlist", Logger.LogLevel.DBG);
            aTGM.Response(fullPlaylist);
        }
        internal static void PlayIndex(TGM aTGM)
        {
            Logger.Log("Play Track by Index -> Index:" + aTGM.Data[0] + " -> " + aTGM, Logger.LogLevel.DBG);
            WinampLib.SetPlaylistPosition(Int32.Parse(aTGM.Data[0]));
        }
        internal static void RemoveIndex(TGM aTGM)
        {
            Logger.Log("PlayIndex -> Index:" + aTGM.Data[0] +" -> "+ aTGM, Logger.LogLevel.DBG);
            WinampLib.DeleteIndex(Int32.Parse(aTGM.Data[0]));
        }
        internal static void EnqueueTrack(TGM aTGM)
        {
            bool nextTrack = aTGM.Data[1] != "0";
            WinampLib.Enqueu(aTGM.Data[0], nextTrack);
        }

        internal static void EnqueueDir(TGM aTGM)
        {
            bool nextTrack = aTGM.Data[1] != "0";
            WinampLib.Enqueu(aTGM.Data[0], nextTrack);
        }

        internal static void ClearPlaylist(TGM aTGM)
        {
            Logger.Log("Going to Clear Playlist ->" + aTGM, Logger.LogLevel.DBG);
            WinampLib.DeleteCurrentPlaylist();
        }
        internal static void SetVolume(TGM aTGM)
        {
            Logger.Log("Set WinampVolume -> Vol:" + aTGM.Data[0] + " -> " + aTGM.ToString(), Logger.LogLevel.DBG);
            WinampLib.SetVolume(Int32.Parse(aTGM.Data[0]));
        }
        internal static void GetVolume(TGM aTGM)
        {
            int volume = WinampLib.GetCurrentVolume();
            Logger.Log("Get WinampVolume -> " + volume, Logger.LogLevel.DBG);
            aTGM.Response(volume.ToString());
            
        }
        internal static void GetTitle(TGM aTGM)
        {
            string title = WinampLib.GetCurrentSongTitle();
            Logger.Log("Get WinampTitle -> " + title +" -> " + aTGM.ToString(), Logger.LogLevel.DBG);
            aTGM.Response(title);
        }
        internal static void GetIndex(TGM aTGM)
        {
            int index = WinampLib.GetPlaylistPosition();
            Logger.Log("Get WinampIndex-> " + index + " -> " + aTGM.ToString(), Logger.LogLevel.DBG);
            aTGM.Response(index.ToString());
        }
    }

}
