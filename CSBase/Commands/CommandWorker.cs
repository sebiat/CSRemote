using System;
using System.Collections;
using System.Threading;
using CSBase.Tools;
using CSBase.Communication;

namespace CSBase.Commands
{
    public class CommandWorker
    {
        private Thread _mThread;
        private bool _doWork;
        private readonly Queue _queue;
        private readonly string _namespace="";
        private readonly string _name;
        public CommandWorker(Queue aTGMQueue, string aNamespace, string aName)
        {
            _doWork = false;
            Logger.Log("Init ComWorker",Logger.LogLevel.DBG);
            _namespace = aNamespace;
            _queue = aTGMQueue;
            _name = aName;
        }
        public void Start()
        {
            if(_doWork) Stop();
            _doWork = true;
            _mThread = new Thread(TGMWorker) {Name = "ComWorker<" + _name + ">"};
            _mThread.Start();
            Logger.Log("ComWorker started", Logger.LogLevel.DBG);
        }
        public void Stop()
        {
            _doWork = false;
            if (_mThread != null)
            {
                _mThread.Abort();
            }
            Logger.Log("ComWorker stopped", Logger.LogLevel.DBG);
        }
        private void TGMWorker()
        {
            while (_doWork)
            {
                if (_queue.Count > 0)
                {
                    TGM tgm = (TGM)_queue.Dequeue();
                    Logger.Log("Dequeu TGM -> " + tgm, Logger.LogLevel.DBG);
                    FuncDistributor(tgm);
                }
                Thread.Sleep(100);
            }
        }

        private void FuncDistributor(TGM aTGM)
        {
            Logger.Log("Distribute TGM -> " + aTGM, Logger.LogLevel.DBG);

            bool result = Global.InvokeStringMethod(_namespace + ".Commands." + (Global.AppID) aTGM.AppID + "Func", "PassCommand", aTGM);

            if (result) return;
            Logger.Log("TGM not handled by Application, using CSBase -> " + aTGM, Logger.LogLevel.DBG);
            result = Global.InvokeStringMethod("CSBase.Commands." + (Global.AppID) aTGM.AppID + "Func", "PassCommand",aTGM);
            if (!result) Logger.Log("TGM not handled -> " + aTGM, Logger.LogLevel.ERR);
            else Logger.Log("TGM handled by CSBase-> " + aTGM, Logger.LogLevel.DBG);
        }
    }
}
