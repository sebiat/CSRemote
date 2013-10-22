using System.Reflection;
using CSBase.Communication;

namespace CSBase.Commands
{
    public class BaseFunc
    {
        #region Needed_Def
        public enum Commands
        {
        }
        internal static bool PassCommand(TGM aTGM)
        {
            return Global.InvokeStringMethod(MethodBase.GetCurrentMethod().DeclaringType.FullName, ((Commands)aTGM.ComID).ToString(), aTGM);
        }
        #endregion
    }
}
