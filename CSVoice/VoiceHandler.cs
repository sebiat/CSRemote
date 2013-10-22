using System.Collections.Generic;
using System.Speech.Recognition;
using CSBase.Communication;
using CSBase.Commands;

namespace CSVoice
{
    class VoiceHandler
    {
        public bool IsListening { get; private set; }
        public string LastCommand { get; private set; } 
        private readonly SpeechRecognitionEngine _sre;
        private readonly List<string> _commands = new List<string>();
        readonly Connector _con;
        public VoiceHandler(Connector aConnector)
        {
            IsListening = true;
            _con = aConnector;
            // create the engine with a custom method (i will describe that later)
            _sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
            _sre.SpeechRecognized += engine_SpeechRecognized;
            InitCommands();
            LoadGrammar();
            _sre.SetInputToDefaultAudioDevice();
            _sre.RecognizeAsync(RecognizeMode.Multiple);
        }
        private void engine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string ReqText = e.Result.Text;
            if (ReqText == "Activate") IsListening = true;
            else if (ReqText == "Deactivate") IsListening = false;
            if(IsListening)HandelCommand(e.Result.Text);
        }
        private void HandelCommand(string aCommand)
        {
            switch (aCommand)
            {
                case "Mute":
                    {
                        _con.QueueTGM(new TGM((int)CSBase.Global.AppID.Windows,(int)WindowsFunc.Commands.Mute));    
                    }
                    break;
                case "Unmute":
                    {
                        _con.QueueTGM(new TGM((int)CSBase.Global.AppID.Windows, (int)WindowsFunc.Commands.UnMute));   
                    }
                    break;
                case "Next Song":
                    {
                        _con.QueueTGM(new TGM((int)CSBase.Global.AppID.Winamp, (int)WinampFunc.Commands.NextTrack));  
                    }
                    break;
                case "Previous Song":
                    {
                        _con.QueueTGM(new TGM((int)CSBase.Global.AppID.Winamp, (int)WinampFunc.Commands.PreviousTrack)); 
                    }
                    break;
                case "Pause Video":
                    {
                        _con.QueueTGM(new TGM((int)CSBase.Global.AppID.VLC, (int)VLCFunc.Commands.Pause));
                    }
                    break;
                case "Pause Music":
                    {
                        _con.QueueTGM(new TGM((int)CSBase.Global.AppID.Winamp, (int)WinampFunc.Commands.Pause));
                    }
                    break;
                case "Play":
                    {
                    }
                    break;
                case "Toggle Fullscreen":
                    {
                        _con.QueueTGM(new TGM((int)CSBase.Global.AppID.VLC, (int)VLCFunc.Commands.ToggleFullsceen)); 
                    }
                    break;
                default:
                    {
                    }
                    break;

            }
            LastCommand = aCommand;
        }
        private void InitCommands()
        {
            _commands.Clear();
            _commands.Add("Mute");
            _commands.Add("Unmute");
            _commands.Add("Next Song");
            _commands.Add("Previous Song");
            _commands.Add("Pause Video");
            _commands.Add("Pause Music");
            _commands.Add("Play");
            _commands.Add("Toggle Fullscreen");
            _commands.Add("Activate");
            _commands.Add("Deactivate");
        }
        private void LoadGrammar()
        {
            _sre.RecognizeAsyncStop();
            _sre.UnloadAllGrammars();
            Choices commandTexts = new Choices();
            foreach (string com in _commands)
            {
                commandTexts.Add(com);
            }
            Grammar wordsList = new Grammar(new GrammarBuilder(commandTexts));
            _sre.LoadGrammar(wordsList);
        }
    }
}
