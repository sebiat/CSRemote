using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CSBase.Tools;
using CSBase.Communication;

namespace CSServer.Communication
{
    class Server
    {
        static TcpListener _serverListener;
        static Thread _threadFindClients;
        public static List<Connector> Clients = new List<Connector>();
        private static string _baseNamespace;

        static Server()
        {
            IsListening = false;
        }

        public static bool IsListening { get; private set; }

        public static void ClientDisconnect(Connector aClient)
        {
            RemoveClient(aClient, null);
        }
        public static void StartListening(int aPort)
        {
            _baseNamespace = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            if (IsListening) StopListening();
            _serverListener = new TcpListener(IPAddress.Any, aPort);
            Logger.Log("TCP Server start listen for Clients", Logger.LogLevel.DBG);
            _serverListener.Start();
            _threadFindClients = new Thread(ListenForClients) {Name = "FindClients"};
            _threadFindClients.Start();
            IsListening = true;
        }
        public static void StopListening()
        {
            Logger.Log("TCP Server stop listen for Clients", Logger.LogLevel.DBG);
            _serverListener.Stop();
            _threadFindClients.Abort();
            IsListening = false;
        }        
        private static void ListenForClients()
        {
            try
            {
                while (IsListening)
                {
                    Socket newSocket = _serverListener.AcceptSocket();
                    Logger.Log("TCPServer accept a new Client -> IP:" + newSocket.RemoteEndPoint, Logger.LogLevel.DBG);
                    Connector newCon = new Connector(newSocket, _baseNamespace,"");
                    newCon.Diconnected += RemoveClient;
                    Clients.Add(newCon);
                }
            }
            catch (ThreadAbortException)
            {
                Logger.Log("Thread aborted!", Logger.LogLevel.WRN);
            }
            catch (Exception e)
            {
                Logger.Log("Error while listen for new Clients -> " + e, Logger.LogLevel.ERR);
            }
        }

        static void RemoveClient(object sender, EventArgs e)
        {
            Logger.Log("Remove Client from internal list -> " + sender, Logger.LogLevel.DBG);
            if (Clients.Contains((Connector)sender))
            {
                Clients.Remove((Connector)sender);
            }
            else
            {
                Logger.Log("Cant find Client in internal list -> " + sender, Logger.LogLevel.DBG);
            }

        }
    }
}



