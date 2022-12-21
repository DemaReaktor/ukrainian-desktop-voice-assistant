using System;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Speech.Synthesis;
using Vosk;
using NAudio.Wave;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Speech2
{
    public class Assistant
    {
        public class TextEventArgs : EventArgs { public string Text; }
        public delegate void TextMethod(object o, TextEventArgs speakEventArgs);
        private EventHandler<TextEventArgs> OnSpeakUp;
        private EventHandler<TextEventArgs> OnSpeakDown;
        private EventHandler<TextEventArgs> OnConsoleWrite;
        private readonly string settingsPath = "AssistantSettings.xml";
        private Task recognizer;
        /// <summary>
        /// -10 ... 10
        /// </summary>
        public int Rate
        {
            set
            {
                rate = Math.Clamp(value, -10, 10);
                SetData();
            }
            get => rate;
        }
        private int rate;
        /// <summary>
        /// 0 ... 100
        /// </summary>
        public int Volume
        {
            set
            {
                volume = Math.Clamp(value, 0, 100);
                SetData();
            }
            get => volume;
        }
        private int volume;
        public string Voice
        {
            get => voice;
            set
            {
                if (synthen.GetInstalledVoices().Any(e => e.VoiceInfo.Name.ToLower() == value.ToLower()))
                {
                    voice = value.ToLower();
                    SetData();
                }
            }
        }
        private string voice;
        public bool IsWomen { get => synthen.GetInstalledVoices().Any(e => e.VoiceInfo.Name.ToLower() == Voice && e.VoiceInfo.Gender == VoiceGender.Female); }
        public ICommand[] Commands
        {
            get
            {
                ICommand[] names = new ICommand[commands.Count];
                for (int i = 0; i < commands.Count; i++)
                    names[i] = commands.ElementAt(i);
                return names;
            }
        }
        public List<InstalledVoice> Voices { get => synthen.GetInstalledVoices().ToList(); }

        private SpeechSynthesizer synthen;
        private VoskRecognizer voskRecognizer;
        private LinkedList<ICommand> commands;

        public Assistant(string ModelPath)
        {
            synthen = new SpeechSynthesizer();
            synthen.SetOutputToDefaultAudioDevice();

            Task isData = GetData();

            recognizer = Task.Run(() =>
            {
                voskRecognizer = new VoskRecognizer(new Model(ModelPath), 44100f);
                while (!isData.IsCompleted) { }
            });
            try
            {
                WaveInEvent waveIn = new WaveInEvent();
                waveIn.WaveFormat = new WaveFormat(44100, 1);
                waveIn.DataAvailable += WaveInOnDataAvailable;
                waveIn?.StartRecording();
            }
            catch
            {
                Speak("твій мікрофон пішов за російським кораблем");
            }

            commands = new LinkedList<ICommand>();
        }
        public Task Recognizer => recognizer; 

        private Task GetData() => Task.Run(() =>
            {
                lock (settingsPath)
                {
                    if (File.Exists(settingsPath))
                    {
                        var xElement = XElement.Load("Settings.xml");
                        rate = int.Parse(xElement.Element("rate").Value);
                        volume = int.Parse(xElement.Element("volume").Value);
                        voice = xElement.Element("voice").Value.ToLower();
                        return;
                    }
                    rate = 0;
                    volume = 50;
                    voice = synthen.GetInstalledVoices().First().VoiceInfo.Name.ToLower();
                }
            });
        private Task SetData() => Task.Run(() =>
            {
                lock (settingsPath)
                    lock (voice)
                        lock ((object)volume)
                            lock ((object)rate)
                            {
                                XElement xElement = new XElement("settings");
                                xElement.Add(new XElement("rate", rate));
                                xElement.Add(new XElement("volume", volume));
                                xElement.Add(new XElement("voice", voice));
                                xElement.Save(settingsPath);
                            }
            });

        public void Write(string text) => OnConsoleWrite?.Invoke(this, new TextEventArgs() { Text = text });
        public void AddSpeakUpListener(TextMethod method) => OnSpeakUp += new EventHandler<TextEventArgs>(method);
        public void RemoveSpeakUpListener(TextMethod method) => OnSpeakUp -= OnSpeakUp.GetInvocationList().First(e =>
        (e as EventHandler<TextEventArgs>).GetInvocationList().First() as TextMethod == method) as EventHandler<TextEventArgs>;
        public void AddSpeakDownListener(TextMethod method) => OnSpeakDown += new EventHandler<TextEventArgs>(method);
        public void RemoveSpeakDownListener(TextMethod method) => OnSpeakDown -= OnSpeakDown.GetInvocationList().First(e =>
        (e as EventHandler<TextEventArgs>).GetInvocationList().First() as TextMethod == method) as EventHandler<TextEventArgs>;
        public void AddWriteListener(TextMethod method) => OnConsoleWrite += new EventHandler<TextEventArgs>(method);
        public void RemoveWriteListener(TextMethod method) => OnConsoleWrite -= OnConsoleWrite.GetInvocationList().First(e =>
        (e as EventHandler<TextEventArgs>).GetInvocationList().First() as TextMethod == method) as EventHandler<TextEventArgs>;

        public Task Speak(string text) => Task.Run(() =>
        {
            OnSpeakUp?.Invoke(this, new TextEventArgs() { Text = text });

            if (!synthen.GetInstalledVoices().Any(e => e.VoiceInfo.Name.ToLower() == Voice.ToLower()))
                return;

            var builder = new PromptBuilder();
            builder.StartStyle(new PromptStyle());

            synthen.Rate = rate;
            synthen.Volume = volume;

            builder.StartVoice(synthen.GetInstalledVoices().First(e => e.VoiceInfo.Name.ToLower() == voice).VoiceInfo);
            builder.AppendText(text);
            builder.EndVoice();
            builder.EndStyle();
            synthen.SpeakAsync(builder);
            OnSpeakDown?.Invoke(this, new TextEventArgs() { Text = text });
        });

        //записує голос
        private void WaveInOnDataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                //якщо модель українського голосу загрузився
                if (voskRecognizer != null && voskRecognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
                {
                    string text = voskRecognizer.FinalResult().Remove(0, 14);
                    text = text.Remove(text.Length - 3, 3);
                    //якщо щось було сказано
                    if (text.Length > 0)
                        Command(text);
                }
            }
            catch { }
        }
        public void AddCommand(ICommand command) => AddCommand(command, commands.Count);
        public void AddCommand(ICommand command, int prioritet)
        {
            if (prioritet == 0)
                commands.AddFirst(command);
            else
                commands.AddAfter(commands.Find(commands.ElementAt(prioritet - 1)), command);
        }
        public void RemoveCommand(int prioritet) => commands.Remove(commands.Find(commands.ElementAt(prioritet)));

        private void Command(string text)
        {
            foreach (var command in commands)
                if (command.Check(text))
                {
                    command.Execute(this, text);
                    return;
                }
        }
    }
    //перетворює слово у число від -10 до 10
    class WordsToInt
    {
        static readonly Dictionary<string, int> words = new Dictionary<string, int>() {
            { "нуль",0},{ "один",1},{ "два",2},{ "три",3},{ "чотири",4},{ "п'ять",5},{ "шість",6},{ "сім",7},{ "вісім",8},{ "дев'ять",9},{ "десять",10} };
        public static int ToInt(string text)
        {
            int result = 0;
            bool isNegative = false;
            foreach (var e in text.Split(' '))
            {
                if (words.ContainsKey(e))
                    result += words[e];

                if (e == "мінус")
                    isNegative = true;
            }
            return isNegative ? -result : result;
        }
    }
}
