using System;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using CSBase.Tools;

namespace CSBase.Communication
{
    public sealed class TGM
    {
        public const string TGMDelimter = "<>";
        public const int MaxTgmDataLog = 5;
        private const int TGMHeaderLen = 3;

        public Connector Client { get; private set; }
        public bool IsValid { get; private set; }
        

        #region TGM_Data

        private int _id;
        public int ID { 
            get { return _id; }
            private set
            {
                if (_id == 0) _id = value;
                else Logger.Log("Can't override TGM ID -> " + ToString(),Logger.LogLevel.ERR);
            }
        }
        public int AppID { get; private set; }
        public int ComID { get; private set; }
        public string[] Data { get; private set; }
        #endregion

        public static TGM ParseStream(Connector aClient, NetworkStream aStream)
        {
            Logger.Log("Start analysis NetStream", Logger.LogLevel.DBG);
            StreamReader sr = new StreamReader(aStream);
            string rcvData = sr.ReadLine();
            Logger.Log("Netstream Data -> " + rcvData, Logger.LogLevel.DBG);
            try
            {
                string[] splitedRaw = Regex.Split(rcvData.Substring(0, rcvData.Length), TGMDelimter);
                var tmpID = Int32.Parse(splitedRaw[0]);
                var tmpAppID = Int32.Parse(splitedRaw[1]);
                var tmpComID = Int32.Parse(splitedRaw[2]);
                var tmpAppIDData = new string[splitedRaw.Length - TGMHeaderLen];
                Array.Copy(splitedRaw, TGMHeaderLen, tmpAppIDData, 0, splitedRaw.Length - TGMHeaderLen);


                TGM retTGM = new TGM(aClient,tmpAppID,tmpComID,tmpAppIDData) {ID = tmpID};
                Logger.Log("Analysis finished -> " + retTGM, Logger.LogLevel.DBG);
                return retTGM;
            }
            catch (Exception ex)
            {
                Logger.Log("Error while analysis TGM -> rcvData:" + rcvData + " Excetion:" + ex, Logger.LogLevel.ERR);
                return null;
            }
        }
        public TGM(Connector aClient, int aAppID, int aComID, string[] aData)
        {
            Client = aClient;
            AppID = aAppID;
            ComID = aComID;
            Data = aData;
            IsValid = true;
        }
        public TGM(Connector aClient, int aAppID, int aComID)
            : this(aClient, aAppID, aComID, new string[0])
        {
            
        }

        public TGM(int aAppID, int aComID)
            : this(aAppID, aComID, new string[0])
        {
        }
        public TGM(int aAppID, int aComID, string aData)
            : this(aAppID, aComID, new[]{aData})
        {
        }

        public TGM(int aAppID, int aComID, string[] aData)
        {
            AppID = aAppID;
            ComID = aComID;
            Data = aData;
            ID = GenerateID();
            IsValid = true;
        }

        public string RawData()
        {
            string data = "";
            for (int i = 0; i < Data.Length; i++)
            {
                data += Data[i];
                if (i < Data.Length - 1) data += TGMDelimter;   //Avoid Delimiter at the and of TGM
            }
            if (data != string.Empty)
                return ID + TGMDelimter + AppID + TGMDelimter + ComID + TGMDelimter + data;
            return ID + TGMDelimter + AppID + TGMDelimter + ComID;
        }

        private static int GenerateID()
        {
            DateTime dNow = DateTime.Now;
            return Int32.Parse(String.Format("{0:mmssffff}", dNow));
        }

        public void Response(string aData)
        {
            Response(AppID, ComID, aData);
        }
        public void Response(string[] aData)
        {
            Response(AppID, ComID, aData);
        }
        public void Response(int aAppID, int aComID)
        {
           Response(aAppID, aComID,new string[0]);
        }
        public void Response(int aAppID, int aComID, string aData)
        {
            Response(aAppID, aComID, new[] { aData });
        }
        public void Response(int aAppID, int aComID, string[] aData)
        {
            TGM tgmResponse = new TGM(Client, aAppID, aComID, aData) {ID = this.ID};
            Client.QueueTGM(tgmResponse);
        }


        public override string ToString()
        {
            return "ID:" + ID + " AppID:" + AppID + " ComID:" + ComID + " Data:" + ParamsArray2String();
        }
        private string ParamsArray2String()
        {
            string tmp = "";
            int count = Data.Length;
            if (Data.Length > MaxTgmDataLog)
            {
                int i;
                for (i = 0; i < MaxTgmDataLog; i++)
                    tmp += "Data[" + i + "]=" + Data[i] + " ";
                tmp += "Data[" + i + "]=...";
            }
            else
            {
                for (int i = 0; i < count; i++)
                    tmp += "Data[" + i + "]=" + Data[i] + " ";
            }
            return tmp;
        }
    }
}
