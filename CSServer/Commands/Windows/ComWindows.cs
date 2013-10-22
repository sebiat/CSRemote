using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSRemote
{
    class ComWindows
    {
        private static Thread m_Thread;
        private static  bool m_doWork = false;
        public static void Start()
        {
            Stop();
            m_doWork = true;
            m_Thread = new Thread(TGMWorker);
            m_Thread.Name = "WindowsCom";
            m_Thread.Start();
            Logger.Log("WindowsCom started", Logger.LogLevel.Debug);
        }
        public static void Stop()
        {
            m_doWork = false;
            if (m_Thread != null)
            {
                m_Thread.Abort();
            }
            Logger.Log("WindowsCom stopped", Logger.LogLevel.Debug);
        }
        private static void TGMWorker()
        {
            while (m_doWork)
            {
                if (Global.TGMWindowsQueue.Count > 0)
                {
                    TGM TGMtoHandle = (TGM)Global.TGMWindowsQueue.Dequeue();
                    Logger.Log("Dequeu TGM -> " + TGMtoHandle.ToString(), Logger.LogLevel.Debug);
                    switch (TGMtoHandle.ComID)
                    {
                        case (int)Global.Commands.Windows.SetVolume:
                            {
                                WindowsFunc.SetVolume(TGMtoHandle);
                            }
                            break;
                        case (int)Global.Commands.Windows.GetVolume:
                            {
                                WindowsFunc.GetVolume(TGMtoHandle);
                            }
                            break;
                        case (int)Global.Commands.Windows.GetFileList:
                            {
                                WindowsFunc.GetFileList(TGMtoHandle);
                            }
                            break;
                        default:
                            {
                                Logger.Log("Unknown ComID -> " + TGMtoHandle.ToString(), Logger.LogLevel.Warning);
                            }
                            break;
                    }
                }
                Thread.Sleep(100);
            }
        }
    }
}
