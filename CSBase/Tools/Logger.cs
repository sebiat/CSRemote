using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CSBase.Tools
{
    public static class Logger
    {
        public enum LogLevel
        {
            DBG,
            WRN,
            ERR
        }
        static readonly List<string> LogElements = new List<string>();

        private static bool _doLog = true;
        static Thread _logWriter;
        public static void Init()
        {
            _doLog=true;
            _logWriter = new Thread(Write);
            if(_logWriter.Name != String.Empty) _logWriter.Name = "LogWriter";
            _logWriter.Start();
            Log("Start Logging",LogLevel.DBG);
        }

        public static void Stop()
        {
            Log("Stop logging", LogLevel.DBG);
            _doLog = false;
        }
        static StreamWriter _sr;
        static void Write()
        {
            while(_doLog)
            {
                try
                {
                    _sr = new StreamWriter("debug.log",true);
                    int toRemove = LogElements.Count;
                    for(int i=0;i<toRemove;i++)
                    {
                        _sr.WriteLine(LogElements[i]);
                    }
                    LogElements.RemoveRange(0, toRemove);
                    _sr.Close();
                    _sr.Dispose();
                }
                catch(Exception)
                {
                    _sr.Close();
                    _sr.Dispose();
                }
                Thread.Sleep(1000);
            }
            if (LogElements.Count == 0) return;
            foreach (string logElement in LogElements)
            {
                _sr.WriteLine(logElement);
            }
        }
        public static void Log(string alogData, LogLevel aLogLevel)
        {
            if (!_doLog) return;
            StackTrace stackTra = new StackTrace();
            string threadName = Thread.CurrentThread.Name ?? "NotSet";
            string methodName = "Null";
            var declaringType = stackTra.GetFrame(1).GetMethod().DeclaringType;
            if (declaringType != null)
            {
                methodName = stackTra.GetFrame(1).GetMethod().IsConstructor
                                        ? declaringType.Name
                                        : stackTra.GetFrame(1).GetMethod().Name;
            }
            string tmp = String.Format("{3} : {0:dd/MM/yyyy HH:mm:ss.fff} [{1}] {2} : {4}", DateTime.Now, threadName, stackTra.GetFrame(1).GetMethod().ReflectedType.FullName + "." + methodName, aLogLevel, alogData);
                    
            tmp = Regex.Replace(tmp, @"\n|\r", "");
            LogElements.Add(tmp);
        }
    }
}
