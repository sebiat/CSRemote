using System.Reflection;
using CSBase.Communication;
using CSBase;
using CSServer.Commands.VLC;

namespace CSServer.Commands
{
    class VLCFunc
    {
        internal static bool PassCommand(TGM aTGM)
        {
            return Global.InvokeStringMethod(MethodBase.GetCurrentMethod().DeclaringType.FullName, ((CSBase.Commands.VLCFunc.Commands)aTGM.ComID).ToString(), aTGM);
        }
        internal static void Play(TGM aTGM)
        {
            VLCLib.Play();
        }
        internal static void Stop(TGM aTGM)
        {
            VLCLib.Stop();
        }
        internal static void Pause(TGM aTGM)
        {
            VLCLib.Pause();
        }
        internal static void ToggleFullsceen(TGM aTGM)
        {
            VLCLib.ToggleFullsceen();
        }
        internal static void VolumeDown(TGM aTGM)
        {
            VLCLib.VolDown();
        }
        internal static void VolumeUp(TGM aTGM)
        {
            VLCLib.VolUp();
        }
    }
}
