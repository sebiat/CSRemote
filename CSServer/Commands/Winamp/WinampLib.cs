using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using CSBase.Tools;
using CSBase;

namespace CSServer.Commands.Winamp
{
	public class WinampLib
	{
        private const int MAX_PATH = 260;
        private static IntPtr hwnd;
        public  static Boolean IsRunning = false;

        private static int eqPosition = 0;

        private static Process WinAmpProcess;
        private static IntPtr handle;

		#region DLL Imports
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow([MarshalAs(UnmanagedType.LPTStr)] string lpClassName, 
												[MarshalAs(UnmanagedType.LPTStr)] string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle); 

		[DllImport("user32.dll", CharSet = CharSet.Auto)] 
		public static extern int SendMessageA( 
			IntPtr hwnd, 
			int wMsg, 
			int wParam, 
			uint lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, uint lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(
			IntPtr hwnd, 
			string lpString, 
			int cch);
        [DllImport("Kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UInt32 nSize,
                                                    ref UInt32 lpNumberOfBytesRead);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, bool bInheritHandle, UInt32 dwProcessId);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder lpString, int nMaxCount);
		#endregion

		#region Command Type Constants
		// To tell Winamp that we are sending it a WM_COMMAND it needs the hex code 0x111
		const int WM_COMMAND = 0x111;
        const int WM_COPYDATA = 0x004A;
		// To tell Winamp that we are sending it a WM_USER (WM_WA_IPC) it needs the hex code 0x0400
		const int WM_WA_IPC = 0x0400;
		#endregion

		#region Winamp-specific Constants
		// We have to define the Winamp class name
		private const string m_windowName = "Winamp v1.x"; 
		
		// Useful for GetSongTitle() Method
		private const string strTtlEnd = " - Winamp";
		#endregion

		#region WM_COMMAND Type Constants
		const int WA_NOTHING            = 0; 
		const int WINAMP_OPTIONS_PREFS  = 40012; // pops up the preferences
		const int WINAMP_OPTIONS_AOT    = 40019; // toggles always on top
		const int WINAMP_FILE_PLAY      = 40029; // pops up the load file(s) box
		const int WINAMP_OPTIONS_EQ     = 40036; // toggles the EQ window
		const int WINAMP_OPTIONS_PLEDIT = 40040; // toggles the playlist window
		const int WINAMP_HELP_ABOUT     = 40041; // pops up the about box
		const int WA_PREVTRACK          = 40044; // plays previous track
		const int WA_PLAY               = 40045; // plays selected track
		const int WA_PAUSE              = 40046; // pauses/unpauses currently playing track
		const int WA_STOP               = 40047; // stops currently playing track
		const int WA_NEXTTRACK          = 40048; // plays next track
		const int WA_VOLUMEUP           = 40058; // turns volume up
		const int WA_VOLUMEDOWN         = 40059; // turns volume down
		const int WINAMP_FFWD5S         = 40060; // fast forwards 5 seconds
		const int WINAMP_REW5S          = 40061; // rewinds 5 seconds
		const int WINAMP_BUTTON1_SHIFT  = 40144; // fast-rewind 5 seconds
		const int WINAMP_BUTTON4_SHIFT  = 40147; // stop after current track
		const int WINAMP_BUTTON5_SHIFT  = 40148; // fast-forward 5 seconds
		const int WINAMP_BUTTON1_CTRL   = 40154; // start of playlist
		const int WINAMP_BUTTON2_CTRL   = 40155; // open URL dialog
		const int WINAMP_BUTTON4_CTRL   = 40157; // fadeout and stop
		const int WINAMP_BUTTON5_CTRL   = 40158; // end of playlist
		const int WINAMP_FILE_DIR       = 40187; // pops up the load directory box
		const int ID_MAIN_PLAY_AUDIOCD1 = 40323; // starts playing the audio CD in the first CD reader
		const int ID_MAIN_PLAY_AUDIOCD2 = 40323; // plays the 2nd
		const int ID_MAIN_PLAY_AUDIOCD3 = 40323; // plays the 3rd
		const int ID_MAIN_PLAY_AUDIOCD4 = 40323; // plays the 4th

		#endregion

		#region WM_USER (WM_WA_IPC) Type Constants
        const int IPC_ENQUEUEFILE =  100;
		const int IPC_ISPLAYING      = 104;		 // Returns status of playback. Returns: 1 = playing, 3 = paused, 0 = not playing)
		const int IPC_GETVERSION     = 0;	     // Returns Winamp version (0x20yx for winamp 2.yx,  Versions previous to Winamp 2.0
												 // typically (but not always) use 0x1zyx for 1.zx versions
		const int IPC_DELETE         = 101;		 // Clears Winamp internal playlist;
		const int IPC_GETOUTPUTTIME  = 105;		 // Returns the position in milliseconds of the 
												 // current song (mode = 0), or the song length, in seconds (mode = 1). It 
												 // returns: -1 if not playing or if there is an error.
		const int IPC_JUMPTOTIME     = 106;		 // Sets the position in milliseconds of the current song (approximately). It
												 // returns -1 if not playing, 1 on eof, or 0 if successful. It requires Winamp v1.60+
		const int IPC_WRITEPLAYLIST  = 120;		 // Writes the current playlist to <winampdir>\\Winamp.m3u, and returns the current 
												 // playlist position. It requires Winamp v1.666+
		const int IPC_SETPLAYLISTPOS = 121;		 // Sets the playlist position
		const int IPC_SETVOLUME      = 122;		 // Sets the volume of Winamp (from 0-255)
		const int IPC_SETPANNING     = 123;		 // Sets the panning of Winamp (from 0 (left) to 255 (right))
		const int IPC_GETLISTLENGTH  = 124;		 // Returns the length of the current playlist in tracks
		const int IPC_GETLISTPOS     = 125;      // Returns the playlist position. A lot like IPC_WRITEPLAYLIST only faster since it 
												 // doesn't have to write out the list. It requires Winamp v2.05+
        const int IPC_GETPLAYLISTTITLE = 212;
        const int IPC_GETPLAYLISTPATH = 211;
		const int IPC_GETINFO        = 126;		 // Returns info about the current playing song (about Kb rate). The value it returns 
												 // depends on the value of 'mode'. If mode == 0 then it returns the Samplerate (i.e. 44100), 
												 // if mode == 1 then it returns the Bitrate  (i.e. 128), if mode == 2 then it returns the 
												 // channels (i.e. 2)

		const int IPC_GETEQDATA		 = 127;      // Queries the status of the EQ. The value it returns depends on what 'position' is set to. It
												 // requires Winamp v2.05+
												 // Value      Meaning
												 // ------------------
												 // 0-9        The 10 bands of EQ data. 0-63 (+20db - -20db)
												 // 10         The preamp value. 0-63 (+20db - -20db)
												 // 11         Enabled. zero if disabled, nonzero if enabled.
												 // 12         Autoload. zero if disabled, nonzero if enabled.


		const int IPC_SETEQDATA      = 128;		 // Sets the value of the last position retrieved by IPC_GETEQDATA (integer eqPosition). It
												 // requires Winamp v2.05+
        const int IPC_GETSHUFFLE = 250;
        const int IPC_GETREPEAT = 251;
        const int IPC_SETSHUFFLE = 252;
        const int IPC_SETREPEAT = 253;

        #region PE WM_USER (WM_WA_IPC) Type Constants
        const int IPC_PE_DELETEINDEX = 104;
        const int IPC_PE_SWAPINDEX = 105;
        const int IPC_PE_INSERTFILENAME = 106;
        #endregion
        #endregion
        [StructLayout(LayoutKind.Sequential)]
        private struct FileInfo
        {
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PATH)]
            public char[] file; // name of mp3 file (max. 255 chars + '\0')
            [MarshalAs(UnmanagedType.U4)]
            public uint index; // index in playlist
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public uint dwData;
            public uint cbData;
            public uint lpData;
        }
        private static void FindWinmap()
        {
            try
            {
                hwnd = FindWindow(m_windowName, null);
                WinAmpProcess = Process.GetProcessesByName("Winamp")[0];
                handle = OpenProcess(0x0010, false, (uint)WinAmpProcess.Id);
                IsRunning = true;
            }
            catch
            {
                IsRunning = false;
            }
        }
        private static void Check()
        {
                FindWinmap();       //TODO Find better way of doing this
        }
		#region Other useful Winamp Methods
		public  static string GetCurrentSongTitle() 
		{
            Check();
			if (hwnd.Equals(IntPtr.Zero)) 
				return "N/A";
 
			string lpText = new string((char) 0, 100);
			int intLength = GetWindowText(hwnd, lpText, lpText.Length);
            
			if ((intLength <= 0) || (intLength > lpText.Length)) 
				return "N/A";
 
			string strTitle = lpText.Substring(0, intLength);
			int intName = strTitle.IndexOf(strTtlEnd);
			int intLeft = strTitle.IndexOf("[");
			int intRight = strTitle.IndexOf("]");
 
			if ((intName >= 0) && (intLeft >= 0) && (intName < intLeft) && (intRight >= 0) && (intLeft + 1 < intRight))
				return strTitle.Substring(intLeft + 1, intRight - intLeft - 1);
 
			if ((strTitle.EndsWith(strTtlEnd)) && (strTitle.Length > strTtlEnd.Length))
				strTitle = strTitle.Substring(0, strTitle.Length - strTtlEnd.Length);
 
			int intDot = strTitle.IndexOf(".");
			if ((intDot > 0) && IsNumeric(strTitle.Substring(0, intDot)))
				strTitle = strTitle.Remove(0, intDot + 1);
 
			return strTitle.Trim();
		}
        public static string[] GetAllPathsFromPlaylist()
        {
            int len = SendMessage(hwnd, (int)WM_WA_IPC, 0, (uint)IPC_GETLISTLENGTH);
            string[] listPaths = new string[len];
            for (int i = 0; i < len; i++)
            {
                listPaths[i] =readStringFromWinampMemory(SendMessage(hwnd, (int)WM_WA_IPC, i,(uint)IPC_GETPLAYLISTPATH));
            }
            return listPaths; 
        }
        public static string GetPathbyIndex(int a_index)
        {
            return readStringFromWinampMemory(SendMessage(hwnd, (int)WM_WA_IPC, a_index, (uint)IPC_GETPLAYLISTPATH));
        }
        private static bool IsNumeric(string Value)
		{
			try 
			{
				double.Parse(Value);
				return true;
			}
			catch
			{
				return false;
			}
		}
		#endregion

		#region WM_COMMAND Type Methods

        public static int GetCurrentVolume()
        {
            Check();
            return SendMessageA(hwnd, WM_WA_IPC, -666, IPC_SETVOLUME);
        }
        public static void Stop()
		{
            Check();
			SendMessageA(hwnd, WM_COMMAND, WA_STOP, WA_NOTHING); 
		}
        public static void Play() 
		{
            Check();
			SendMessageA(hwnd, WM_COMMAND, WA_PLAY, WA_NOTHING); 
		}
        public static void Pause() 
		{
            Check();
			SendMessageA(hwnd, WM_COMMAND, WA_PAUSE, WA_NOTHING); 
		}
        public static void PrevTrack() 
		{
            Check();
			SendMessageA(hwnd, WM_COMMAND, WA_PREVTRACK, WA_NOTHING); 
		}
        public static void NextTrack() 
		{
            Check();
			SendMessageA(hwnd, WM_COMMAND, WA_NEXTTRACK, WA_NOTHING); 
		}
        public static void VolumeUp() 
		{
            Check();
			SendMessageA(hwnd, WM_COMMAND, WA_VOLUMEUP, WA_NOTHING); 
		}
        public static void VolumeUp(int volinc)
        {
            Check();
            int currentVol = GetCurrentVolume();
            currentVol += Math.Abs(volinc);
            if(currentVol>255) currentVol=255;
            SetVolume(currentVol);
        }
        public static void VolumeDown() 
		{
            Check();
			SendMessageA(hwnd, WM_COMMAND, WA_VOLUMEDOWN, WA_NOTHING); 
		}
        public static void VolumeDown(int volinc)
        {
            Check();
            int currentVol = GetCurrentVolume();
            currentVol -= Math.Abs(volinc);
            if(currentVol<0) currentVol=0;
            SetVolume(currentVol);
        }
        public static void Forward5Sec() 
		{
            Check();
			SendMessageA(hwnd, WM_COMMAND, WINAMP_FFWD5S, WA_NOTHING); 
		}
        public static void Rewind5Sec() 
		{
            Check();
			SendMessageA(hwnd, WM_COMMAND, WINAMP_REW5S, WA_NOTHING); 
		}
		#endregion

		#region WM_USER (WM_WA_IPC) Type Methods
        public static int GetPlaybackStatus() 
		{
            Check();
			return SendMessageA(hwnd, WM_WA_IPC, WA_NOTHING, IPC_ISPLAYING);
		}
        public static int GetWinampVersion()
		{
            Check();
			return SendMessageA(hwnd, WM_WA_IPC, WA_NOTHING, IPC_GETVERSION);
		}
        public static void DeleteCurrentPlaylist()
		{
            Check();
			SendMessageA(hwnd, WM_WA_IPC, WA_NOTHING, IPC_DELETE);
		}
        public static void SavePlaylist()
		{
            Check();
			SendMessageA(hwnd, WM_WA_IPC, WA_NOTHING, IPC_WRITEPLAYLIST);
		}
        public static int GetPlaylistPosition()
		{
            Check();
			return SendMessageA(hwnd, WM_WA_IPC, WA_NOTHING, IPC_GETLISTPOS);
		}
        public static void SetPlaylistPosition(int position)
		{
            Check();
            SendMessageA(hwnd, WM_WA_IPC, position, IPC_SETPLAYLISTPOS);
            Play();
		}
        public static int GetTrackPosition()
		{
            Check();
			return SendMessageA(hwnd, WM_WA_IPC, WA_NOTHING, IPC_GETOUTPUTTIME);
		}
        public static int GetTrackLenght()
		{
            Check();
			return SendMessageA(hwnd, WM_WA_IPC, 1 , IPC_GETOUTPUTTIME);
		}
        public static int GetPlaylistLenght()
		{
            Check();
			return SendMessageA(hwnd, WM_WA_IPC, WA_NOTHING, IPC_GETLISTLENGTH);
		}
        public static void JumpToTrackPosition(int position)
		{
            Check();
			SendMessageA(hwnd, WM_WA_IPC, position, IPC_JUMPTOTIME);
		}
        public static void SetVolume(int position)
		{
            Check();
			SendMessageA(hwnd, WM_WA_IPC, position, IPC_SETVOLUME);
		}
        public static void SetPanning(int position)
		{
            Check();
			SendMessageA(hwnd, WM_WA_IPC, position, IPC_SETPANNING);
		}
        public static void GetTrackInfo(int mode)
		{
            Check();
			SendMessageA(hwnd, WM_WA_IPC, mode, IPC_GETINFO);
		}
        public static void GetEqData(int position)
		{
            Check();
			eqPosition = SendMessageA(hwnd, WM_WA_IPC, position, IPC_GETEQDATA);
		}
        public static int SetEqData()
		{
            Check();
			SendMessageA(hwnd, WM_WA_IPC, eqPosition, IPC_SETEQDATA);
			return eqPosition;
		}
        private static string readStringFromWinampMemory(int winampMemoryAddress)
        {
            try
            {
                Check();
                string str = "";
                byte[] buff = new byte[500];
                uint ret = new UInt32();
                IntPtr pos = new IntPtr(winampMemoryAddress);
                int stringLength = 250; // Max length, if the buffer doesn't contain 0x00
                if (ReadProcessMemory(handle, pos, buff, 500, ref ret))
                {
                    for (int i = 0; i < stringLength; i++)
                    {
                        if (buff[i] != 0x00)
                        {
                            continue;
                        }
                        stringLength = i; // Store length
                        break;
                    }
                    Encoding encoding = Encoding.Default;
                    str = encoding.GetString(buff, 0, stringLength); // Encode from start to 0x00
                }

                return str;
            }
            catch { return null; }
        }
        public static string GetTitlebyIndex(int a_Index)
        {
            Check();
            return readStringFromWinampMemory(SendMessage(hwnd, (int)WM_WA_IPC, a_Index, (uint)IPC_GETPLAYLISTTITLE));
        }
        public static string[] GetPlaylist()
        {
            Check();
            int len = SendMessage(hwnd, (int)WM_WA_IPC, 0, (uint)IPC_GETLISTLENGTH);
            string[] listNames = new string[len];
            for (int i = 0; i < len; i++)
            {
                listNames[i] = readStringFromWinampMemory(SendMessage(hwnd, (int)WM_WA_IPC, i, (uint)IPC_GETPLAYLISTTITLE));
            }
            return listNames;
        }
        public static void DeleteIndex(int a_Index)
        {
            IntPtr hWndWinampPlayList = Global.FindChildWindow(IntPtr.Zero, "Winamp PE", "Winamp Playlist-Editor");
            SendMessageA(hWndWinampPlayList, (int)WM_WA_IPC, IPC_PE_DELETEINDEX, (uint)a_Index);
        }
        public static void Enqueu(string Path, bool a_NextFile)
        {
            Logger.Log("Find Playlist Handle...", Logger.LogLevel.DBG);
            IntPtr hWndWinampPlayList = Global.FindChildWindow(IntPtr.Zero, "Winamp PE", "Winamp Playlist-Editor");
            if (hWndWinampPlayList != IntPtr.Zero)
            {
                Logger.Log("Found Playlist Handle -> " + hWndWinampPlayList.ToString(), Logger.LogLevel.DBG);
                int index;
                if( a_NextFile) index = GetPlaylistPosition() + 1;
                else index = GetPlaylistLenght();
                Enq(hWndWinampPlayList, Path, index);
            }
            else Logger.Log("Could not find Playlist Handle -> " + hWndWinampPlayList.ToString(), Logger.LogLevel.WRN);
        }
        private static void Enq(IntPtr hWnd, string FileName, int Index)
        {
            Logger.Log("Build Structs...", Logger.LogLevel.DBG);
            FileInfo f = new FileInfo();
            f.file = new char[MAX_PATH]; int i;
            char[] dummy = FileName.ToCharArray();
            for (i = 0; (i < FileName.Length) && (i < 255); i++)
                f.file[i] = dummy[i];
            f.file[i] = '\0';

            f.index = (uint)Index;

            IntPtr fmem = Marshal.AllocCoTaskMem(MAX_PATH + sizeof(uint));
            Marshal.StructureToPtr(f, fmem, false);

            COPYDATASTRUCT cds = new COPYDATASTRUCT();
            cds.lpData = (uint)(fmem.ToInt32());
            cds.cbData = (uint)(MAX_PATH + sizeof(uint));
            cds.dwData = (uint)(IPC_PE_INSERTFILENAME);

            IntPtr cdsmem = Marshal.AllocCoTaskMem(3 * sizeof(uint));
            Marshal.StructureToPtr(cds, cdsmem, false);
            Logger.Log("Structs Builded!", Logger.LogLevel.DBG);
            SendMessage(hWnd, WM_COPYDATA, 0, (uint)cdsmem.ToInt32()); // API call
            Logger.Log("Command Sended!", Logger.LogLevel.DBG);
            Marshal.FreeCoTaskMem(cdsmem);
            Marshal.FreeCoTaskMem(fmem);
        }
        public static void MoveSong(int src_Index, int dest_Index)
        {
            //(lParam & 0xFFFF0000) >> 16 = from, (lParam & 0xFFFF) = to
            byte src = byte.Parse(src_Index.ToString());
            byte dest =   byte.Parse(dest_Index.ToString());
            IntPtr hWndWinampPlayList = Global.FindChildWindow(IntPtr.Zero, "Winamp PE", "Winamp Playlist-Editor");
            uint swap = (uint)(src << 16 | dest);
            SendMessageA(hWndWinampPlayList, (int)WM_WA_IPC, IPC_PE_SWAPINDEX, swap);
        }

		#endregion
	}
}
