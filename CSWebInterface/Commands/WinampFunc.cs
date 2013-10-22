using System.Reflection;
using CSBase;
using CSBase.Communication;
using CSBase.Tools;

namespace CSWebInterface.Commands
{
    public class WinampFunc
    {
        internal static bool PassCommand(TGM aTGM)
        {
            return Global.InvokeStringMethod(MethodBase.GetCurrentMethod().DeclaringType.FullName, ((CSBase.Commands.WinampFunc.Commands)aTGM.ComID).ToString(), aTGM);
        }
        internal static void GetPlaylist(TGM aTGM)
        {
            Logger.Log("Rcv Playlist -> " + aTGM, Logger.LogLevel.DBG);
            CSWebInterface.HTTP.Sites.WinampMain.UpdatePlaylist(aTGM.Data);
        }
    }

}
