using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
namespace G19WinampController
{
    class INIControll
    {
         public string path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key,string val,string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
                 string key,string def, StringBuilder retVal,
            int size,string filePath);
        public INIControll(string INIPath)
        {
            path = INIPath;
            if (!System.IO.File.Exists(path))
            {
                ErrorLog.NewError("Can not find " + path);
                System.IO.File.CreateText(path);
            }
        }

        public void WriteValue(string Section,string Key,string Value)
        {
            WritePrivateProfileString(Section,Key,Value,this.path);
        }

        public bool ReadBool(string Section, string Key, bool Default)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, null, temp, 255, this.path);
            try
            {
                return Convert.ToBoolean(temp.ToString());
            }
            catch(Exception e)
            {
                ErrorLog.NewError("INI Error in "+ path +" " + Section +" / "+Key,e);
                WriteValue(Section, Key, Default.ToString());
                return Default;
            }
        }
        public int ReadInt(string Section, string Key, int Default)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, null, temp, 255, this.path);
            try
            {
                return Convert.ToInt32(temp.ToString());
            }
            catch (Exception e)
            {
                ErrorLog.NewError("INI Error in "+ path +" " + Section + " / " + Key, e);
                WriteValue(Section, Key, Default.ToString());
                return Default;
            }
        }
        public string ReadString(string Section, string Key, string Default)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, null, temp, 255, this.path);
            if(!String.IsNullOrEmpty(temp.ToString())) 
            {
                return temp.ToString();
            }
            else
            {
                ErrorLog.NewError("INI Error in "+ path +" " + Section + " / " + Key);
                WriteValue(Section, Key, Default);
                return Default;
            }
        }
    }
}
