using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CSBase.Tools;
using CSBase.Commands;
namespace CSBase.Communication
{
    public class Connector
    {

        public enum Modes
        {
            Passiv,
            Activ
        }
        public Modes Mode { get; private set; }
        public event EventHandler Diconnected;
        public int ReceivedTGM { get; private set; }
        public int SendedTGM { get; internal set; }
        public IPEndPoint IP
        {
            get
            {
                try
                {
                    return (IPEndPoint)_socket.RemoteEndPoint;
                }
                catch
                {
                    return null;
                }
            }
        }
        public string DeviceName
        {
            get {
                return String.IsNullOrEmpty(_deviceName) ? "NotSet" : _deviceName;
            }
            set
            {
                if (value != string.Empty && value != _deviceName) ForceNameResend();
                _deviceName = value;
            }
        }
        public bool Connected
        {
            get
            {
                if (_socket == null) 
                    return false;
                return _socket.Connected;
            }
        }
        private string _deviceName;


        private int _connectionWatchdog;
        private Socket _socket;
        private Thread _pollMsg, _sendMsg, _lifeMsg, _reconnect;
        private StreamWriter _netStreamWriter;
        private NetworkStream _netStream;
        private Queue _sendQueue,_rcvQueue;
        private CommandWorker _comWorker;
        private readonly int _port;
        private readonly string _ip;
        private string _namespace;
        private bool _tRun = true;

        public Connector(Socket aConnection, string aNamespace, string aName)
        {
            Logger.Log("Init Connector -> " + this, Logger.LogLevel.DBG);
            _socket = aConnection;
            Mode = Modes.Passiv;
            Init(aNamespace, aName);
            Logger.Log("Init Connector completed -> " + this, Logger.LogLevel.DBG);

        }
        public Connector(string aIP, int aPort, string aNamespace, string aName)
        {
            Logger.Log("Init Connector -> " + this, Logger.LogLevel.DBG);
            _socket = new TcpClient(aIP, aPort).Client;
            _ip = aIP;
            _port = aPort;
            Mode = Modes.Activ;
            Init(aNamespace, aName);
            Logger.Log("Init Connector completed -> " + this, Logger.LogLevel.DBG);
        }
        private void Init(string aNamespace, string aName)
        {
            _tRun = true;
            SendedTGM = 0;
            ReceivedTGM = 0;
            _namespace = aNamespace;
            _netStream = new NetworkStream(_socket);
            _netStreamWriter = new StreamWriter(_netStream);
            _sendQueue = new Queue();
            _rcvQueue = new Queue();
            _pollMsg = new Thread(t_PollMessages) { Name = "MsgPoll(" + IP + ")" };
            _sendMsg = new Thread(t_SendMessages) { Name = "MsgSend(" + IP + ")" };
            _lifeMsg = new Thread(t_LifeMessages) { Name = "MsgLife(" + IP + ")" };
            _sendMsg.Start();
            _pollMsg.Start();
            _lifeMsg.Start();
            _comWorker = new CommandWorker(_rcvQueue, aNamespace, IP.ToString());
            _comWorker.Start();
            DeviceName = aName;
        }
        private void t_PollMessages()
        {
            try
            {
                while (_tRun)
                {
                    if (_netStream.DataAvailable)
                    {
                        Logger.Log("Receive Data -> " + this, Logger.LogLevel.DBG);
                        ReceivedTGM++;
                        TGM rcvTGM = TGM.ParseStream(this, _netStream);
                        if (rcvTGM.IsValid)
                        {
                            _rcvQueue.Enqueue(rcvTGM);
                        }
                    }
                    Thread.Sleep(1);
                }
            }
            catch (ThreadAbortException)
            {
                Logger.Log("Thread aborted!", Logger.LogLevel.WRN);
            }
            catch (Exception ex)
            {
                Logger.Log("Error while polling Msg -> Exception:" + ex, Logger.LogLevel.ERR);
                LostConnection();
            }
        }
        private void t_SendMessages()
        {
            try
            {
                while (_tRun)
                {
                    if (_sendQueue.Count > 0)
                    {
                        TGM tgm = (TGM)_sendQueue.Dequeue();
                        _netStreamWriter.WriteLine(tgm.RawData());
                        _netStreamWriter.Flush();
                        SendedTGM++;
                        Logger.Log("Successfully send TGM -> " + tgm, Logger.LogLevel.DBG);
                    }
                    Thread.Sleep(1);
                }
            }
            catch (ThreadAbortException)
            {
                Logger.Log("Thread aborted!", Logger.LogLevel.WRN);
            }
            catch (Exception ex)
            {
                Logger.Log("Error while sending Msg -> Exception:" + ex, Logger.LogLevel.ERR);
                LostConnection();
            }
        }
        private void t_LifeMessages()
        {
            Thread.Sleep(Global.LifeTime * 1000);
            try
            {
                while (_tRun)
                {
                    TGM lTGM = GenLifeTGM();
                    QueueTGM(lTGM);
                    _connectionWatchdog++;
                    Thread.Sleep(Global.LifeTime * 1000);
                    if (_connectionWatchdog > 0) Logger.Log("Missing Live Response from Client -> " + ToString() + " " + lTGM, Logger.LogLevel.WRN);
                    if (_connectionWatchdog <= 3)
                    {
                        Logger.Log("Rcv Live Response from Client -> " + ToString() + " " + lTGM, Logger.LogLevel.DBG);
                        continue;
                    }
                    Logger.Log("Client TimeOut", Logger.LogLevel.WRN);
                    if(_tRun)LostConnection();
                }
            }
            catch (ThreadAbortException)
            {
                Logger.Log("Thread aborted!", Logger.LogLevel.WRN);
            }
            catch (Exception ex)
            {
                Logger.Log("Error while sending LifeTGM -> Exception:" + ex, Logger.LogLevel.ERR);
                LostConnection();
            }
        }
        private void t_Reconnect()
        {
            while (true)
            {
                Logger.Log("Try to connect -> IP:" + _ip +" Port:" + _port, Logger.LogLevel.DBG);
                try
                {
                    _socket = new TcpClient(_ip, _port).Client;
                    Logger.Log("New connection! -> IP:" + _ip + " Port:" + _port, Logger.LogLevel.DBG);
                    Init(_namespace, DeviceName);
                    ForceNameResend();
                    break;
                }
                catch (Exception)
                {
                    Logger.Log("Connect failed -> IP:" + _ip + " Port:" + _port, Logger.LogLevel.DBG); 
                }
                Thread.Sleep(Global.ReconnectTime * 1000); 
            }    
        }

        private TGM GenLifeTGM()
        {
            TGM lTgm =  new TGM((int)Global.AppID.Server, (int)ServerFunc.Commands.Life);
            Logger.Log("Generate LifeTGM -> " + lTgm, Logger.LogLevel.DBG);
            return lTgm;
        }
        public void ResetWatchDog()
        {
            Logger.Log("Reset connection watchdog -> " + this, Logger.LogLevel.DBG);
            _connectionWatchdog = 0;
        }
        private void LostConnection()
        {
            Close();
            if (Mode != Modes.Activ) return;
            Logger.Log("Going to Reconnect!", Logger.LogLevel.DBG);
            _reconnect = new Thread(t_Reconnect) { Name = "Reconnect" };
            _reconnect.Start();
        }
        public void ForceNameResend()
        {
            QueueTGM(new TGM((int)Global.AppID.Server, (int)ServerFunc.Commands.SetDeviceName,  DeviceName ));    
        }
        public void Close()
        {
            if (Diconnected != null) Diconnected(this, null);
            Logger.Log("Close connection to Server -> " +  this, Logger.LogLevel.DBG);
            try { _socket.Shutdown(SocketShutdown.Both); }
            catch (Exception ex) { Logger.Log("Error while closing connection -> " + ex, Logger.LogLevel.WRN); }
            try { _socket.Close(); }
            catch (Exception ex) { Logger.Log("Error while closing connection -> " + ex, Logger.LogLevel.WRN); }
            _tRun = false;
            if (_netStream != null) _netStream.Dispose();
            if (_netStreamWriter != null) _netStreamWriter.Dispose();
            if (_comWorker != null) _comWorker.Stop();
        }
        public void QueueTGM(TGM aTGM)
        {
            try
            {
                Logger.Log("Queue TGM in Sendqueue-> " + aTGM, Logger.LogLevel.DBG);
                if(_socket.Connected) _sendQueue.Enqueue(aTGM);
                else Logger.Log("Failed to Queue TGM -> No Connection-> " + aTGM, Logger.LogLevel.ERR);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to Queue TGM -> " + aTGM + " Exception: " + ex, Logger.LogLevel.ERR);
            }
        }
        public override string ToString()
        {
            return "Device: " + DeviceName + " IP:" + IP + " Mode:" + Mode;
        }
    }
}
