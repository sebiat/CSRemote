using System.Reflection;
using CSBase;
using CSBase.Communication;
using CSBase.Tools;

namespace CSServer.Commands
{
    class ServerFunc
    {
        #region Needed_Def
        //public enum Commands
        //{
        //    Life = 0,           //Is handled by CSBase
        //    Disconnect = 1,
        //    SetDeviceName = 2,
        //}
        internal static bool PassCommand(TGM aTGM)
        {
            return Global.InvokeStringMethod(MethodBase.GetCurrentMethod().DeclaringType.FullName,((CSBase.Commands.ServerFunc.Commands)aTGM.ComID).ToString(),aTGM);
        }
        #endregion
        internal static void Disconnect(TGM aTGM)
        {
            Logger.Log("Client is going to disconnect -> " + aTGM.Client.ToString(), Logger.LogLevel.DBG); 
            aTGM.Client.Close();
            Communication.Server.ClientDisconnect(aTGM.Client);
        }
    }

}
