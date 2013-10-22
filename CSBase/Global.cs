using System;
using System.Reflection;
using CSBase.Tools;
using CSBase.Communication;
using System.Runtime.InteropServices;
namespace CSBase
{
    public static class Global
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        public const string DirChar = "1";
        public const string FileChar = "0";

        public const int LifeTime = 15;
        public const int ReconnectTime = 5;
        public const int CSPort = 10101;

        public enum AppID
        {
            Server = 0,
            Windows = 1,
            Winamp = 2,
            VLC = 3
        }

        public static bool InvokeStringMethod(string typeName, string methodName, TGM aTGM)
        {
            Logger.Log("Try to Invoke Methode by Name -> Class:" + typeName + " Method:" + methodName,
                       Logger.LogLevel.DBG);
            Type calledType = (Assembly.GetEntryAssembly().GetType(typeName) ??
                               Assembly.GetCallingAssembly().GetType(typeName)) ??
                              Assembly.GetExecutingAssembly().GetType(typeName);
            if (calledType == null)
            {
                Logger.Log("Can't find Class -> Class:" + typeName + " Method:" + methodName, Logger.LogLevel.DBG);
                return false;
            }
            bool ret;
            try
            {
                object result = calledType.InvokeMember(methodName,
                                                        BindingFlags.InvokeMethod | BindingFlags.Public |
                                                        BindingFlags.Static | BindingFlags.NonPublic,
                                                        null,
                                                        null,
                                                        new Object[] {aTGM});
                if (result == null) ret = true;
                else ret = (bool) result;
            }
            catch (Exception)
            {
                ret = false;
            }
            Logger.Log("Invoke Methode -> Class:" + typeName + " Method:" + methodName + " Result:" + ret, Logger.LogLevel.DBG);
            return ret;
        }

        public static IntPtr FindChildWindow(IntPtr hwndParent, string lpszClass, string lpszTitle)
        {
            return FindChildWindow(hwndParent, IntPtr.Zero, lpszClass, lpszTitle);
        }
        public static IntPtr FindChildWindow(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszTitle)
        {
            IntPtr hwnd = FindWindowEx(hwndParent, IntPtr.Zero, lpszClass, lpszTitle);
            if (hwnd == IntPtr.Zero)
            {
                IntPtr hwndChild = FindWindowEx(hwndParent, IntPtr.Zero, null, null);
                while (hwndChild != IntPtr.Zero && hwnd == IntPtr.Zero)
                {
                    hwnd = FindChildWindow(hwndChild, IntPtr.Zero, lpszClass, lpszTitle);
                    if (hwnd == IntPtr.Zero)
                    {
                        hwndChild = FindWindowEx(hwndParent, hwndChild, null, null);
                    }
                }
            }
            return hwnd;
        }

    }

}
