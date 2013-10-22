using CSServer.Commands.Windows;
using CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CSBase.Tools;
using CSBase.Communication;
using CSBase;

namespace CSServer.Commands
{
    class WindowsFunc
    {
        internal static bool PassCommand(TGM aTGM)
        {
            return Global.InvokeStringMethod(MethodBase.GetCurrentMethod().DeclaringType.FullName, ((CSBase.Commands.WindowsFunc.Commands)aTGM.ComID).ToString(), aTGM);
        }
        private static MMDevice _device;
        internal static void SetVolume(TGM aTGM)
        {
            int volume = Int32.Parse(aTGM.Data[0]);
            Logger.Log("Set Windowsvolume -> " + volume.ToString(), Logger.LogLevel.DBG);
            try
            {
                MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
                _device = devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
                _device.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)volume / 100.0f);
            }
            catch(Exception ex)
            {
                Logger.Log("Error while set volume -> Exception:" + ex, Logger.LogLevel.DBG);
            }
        }
        internal static void GetVolume(TGM aTGM)
        {
            try
            {
                MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
                _device = devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
                int volume = (int)(_device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                Logger.Log("Get WindowsVolume -> " + volume, Logger.LogLevel.DBG);
                aTGM.Response(volume.ToString());
            }
            catch (Exception ex)
            {
                Logger.Log("Error while reading volume -> Exception:" + ex.ToString(), Logger.LogLevel.ERR);
            }
        }
        internal static void GetFileList(TGM aTGM)
        {
            Logger.Log("Get Dir List -> Dir:" + aTGM.Data[0], Logger.LogLevel.DBG);
            string[] data = {aTGM.Data[0].Replace("%20", " ")};
            int orgDataLen = data.Length;
            string[] tmpdir = System.IO.Directory.GetDirectories(data[0]);
            for (int i = 0; i < tmpdir.Length; i++)
            {
                tmpdir[i] = System.IO.Path.GetFileName(tmpdir[i]);
                tmpdir[i] = Global.DirChar + tmpdir[i];
            }

            Logger.Log("Get File List -> Dir:" + aTGM.Data[0], Logger.LogLevel.DBG);

            string[] tmpfile = System.IO.Directory.GetFiles(data[0]);
            for (int i = 0; i < tmpfile.Length; i++)
            {
                tmpfile[i] = System.IO.Path.GetFileName(tmpfile[i]);
                tmpfile[i] = Global.FileChar + tmpfile[i];
            }

            Array.Resize(ref data, tmpdir.Length + tmpfile.Length + data.Length);
            tmpdir.CopyTo(data, orgDataLen);
            tmpfile.CopyTo(data, tmpdir.Length + orgDataLen);
            aTGM.Response(data);
        }
        internal static void DefaultOpen(TGM aTGM)
        {
            Logger.Log("Open file with default application -> File:" + aTGM.Data[0], Logger.LogLevel.DBG);
            System.Diagnostics.Process.Start(aTGM.Data[0]);    
        }
        internal static void Shutdown(TGM aTGM)
        {
            Logger.Log("Shutdown Windows!",Logger.LogLevel.DBG);
            WinShutdown.Shutdown();
        }
        internal static void Mute(TGM aTGM)
        {
            Logger.Log("Mute Windowsvolume", Logger.LogLevel.DBG);
            try
            {
                MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
                _device = devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
                _device.AudioEndpointVolume.Mute = true;
            }
            catch (Exception ex)
            {
                Logger.Log("Error while Mute -> Exception:" + ex.ToString(), Logger.LogLevel.DBG);
            }
        }
        internal static void UnMute(TGM aTGM)
        {
            Logger.Log("UnMute Windowsvolume", Logger.LogLevel.DBG);
            try
            {
                MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
                _device = devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
                _device.AudioEndpointVolume.Mute = false;
            }
            catch (Exception ex)
            {
                Logger.Log("Error while UnMute -> Exception:" + ex.ToString(), Logger.LogLevel.DBG);
            }
        }
    }
}
