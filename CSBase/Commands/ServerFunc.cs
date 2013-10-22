using System;
using System.Reflection;
using CSBase.Tools;
using CSBase.Communication;

namespace CSBase.Commands
{
    public class ServerFunc
    {
        #region Needed_Def
        public enum Commands
        {
            Life = 0, 
            Disconnect = 1,
            SetDeviceName = 2,
        }
        internal static bool PassCommand(TGM aTGM)
        {
            return Global.InvokeStringMethod(MethodBase.GetCurrentMethod().DeclaringType.FullName, ((Commands)aTGM.ComID).ToString(), aTGM);
        }
        #endregion
        internal static void Life(TGM aTGM)
        {
            if (aTGM.Data.Length > 0)
            {
                int isResonse = Int32.Parse(aTGM.Data[0]);
                if (isResonse == 1)
                {
                    Logger.Log("Rcv Life TGM Response -> " + aTGM, Logger.LogLevel.DBG);
                    aTGM.Client.ResetWatchDog();
                }
            }
            else
            {
                Logger.Log("Rcv Life TGM -> " + aTGM, Logger.LogLevel.DBG);
                aTGM.Response((int)Global.AppID.Server, (int)Commands.Life, "1");
            }
        }
        internal static void SetDeviceName(TGM aTGM)
        {
            Logger.Log("Set DeviceName -> " + aTGM.Data[0], Logger.LogLevel.DBG);
            aTGM.Client.DeviceName = aTGM.Data[0];
        }
    }

}
