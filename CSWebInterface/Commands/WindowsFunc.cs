using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CSBase.Tools;
using CSBase.Communication;
using CSBase;

namespace CSWebInterface.Commands
{
    class WindowsFunc
    {
        internal static bool PassCommand(TGM aTGM)
        {
            return Global.InvokeStringMethod(MethodBase.GetCurrentMethod().DeclaringType.FullName, ((CSBase.Commands.WindowsFunc.Commands)aTGM.ComID).ToString(), aTGM);
        }
        internal static void GetFileList(TGM aTGM)
        {
            HTTP.Sites.WindowsExplorer.RcvTgMs.Add(aTGM);
        }
    }
}
