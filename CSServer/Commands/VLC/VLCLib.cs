using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSServer.Commands.VLC
{
    class VLCLib
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, bool bInheritHandle, UInt32 dwProcessId);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);


        private const int WM_KEYUP = 0x0101;
        private const int WM_KEYDOWN = 0x0100;

        private const int Hotkey_PlayPause = (int)' ';
        private const int Hotkey_Next = (int)'N';
        private const int Hotkey_Prev = (int)'P';
        private const int Hotkey_Stop = (int)'S';
        private const int Hotkey_FullScreen = (int)'F';

        private static IntPtr handle;

        private static void Check()
        {
            handle = Process.GetProcessesByName("vlc")[0].MainWindowHandle;
        }
        private static void SendKey(int aKey)
        {
            SendMessage(handle, WM_KEYDOWN, aKey, 0);
            SendMessage(handle, WM_KEYUP, aKey, 0);
        }

        public static void Pause()
        {
            Check();
            SendKey(Hotkey_PlayPause);
        }
        public static void ToggleFullsceen()
        {
            Check();
            SendKey(Hotkey_FullScreen);
            SetForegroundWindow(handle);
        }
        public static void Play()
        {
            Check();
            SendKey(Hotkey_PlayPause);
        }
        public static void Stop()
        {
            Check();
            SendKey(Hotkey_Stop);
        }
        public static void VolDown()
        {
            Check();
            SendKey((int)System.Windows.Forms.Keys.VolumeUp);
        }
        public static void VolUp()
        {
            Check();
            SendKey((int)0x800);
        }

    }
}
