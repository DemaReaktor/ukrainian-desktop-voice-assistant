using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Globalization;
using System.Xml.Linq;
using System.Linq;
using AutoIt;

namespace Speech
{

    public partial class Form1 : Form
    {
        int rate;
        int volume;
        string voice;
        SpeechSynthesizer synthen;
        SpeechRecognitionEngine recognizer;

        public Form1()
        {
            InitializeComponent();
            synthen = new SpeechSynthesizer();
            textBox1.Text = "привіт паляниця";
            if (File.Exists("Settings.xml"))
            {
                var xElement = XElement.Load("Settings.xml");
                rate = int.Parse(xElement.Element("rate").Value);
                volume = int.Parse(xElement.Element("volume").Value);
                voice = xElement.Element("voice").Value;
            }
            else
            {
                rate = 1;
                volume = 50;
                voice = "Anatol";
                SaveSettings();
            }
            recognizer = new SpeechRecognitionEngine(new CultureInfo("en-US"));

            Choices unsignedNumber = new Choices(WordsToNumbers.NumberTable.Keys.ToArray());
            GrammarBuilder number = new GrammarBuilder(new Choices("minus", " "));
            number.Append(unsignedNumber, 1, 5);

            GrammarBuilder voiceBuild = new GrammarBuilder();
            voiceBuild.Append("voice");
            voiceBuild.Append(new Choices("rate", "volume"));
            voiceBuild.Append(number);
            voiceBuild.Culture = new CultureInfo("en-US");

            Grammar voiceGrammar = new Grammar(voiceBuild);
            voiceGrammar.Name = "voice";

            GrammarBuilder stopBuild = new GrammarBuilder(new Choices("sudo stop program", "sudo program exit", "sudo delete program"));
            stopBuild.Culture = new CultureInfo("en-US");
            Grammar generalGrammar = new Grammar(stopBuild);
            generalGrammar.Name = "general";

            GrammarBuilder writeBuild = new GrammarBuilder("write");
            GrammarBuilder freeBuild = new GrammarBuilder();
            freeBuild.AppendDictation();
            writeBuild.Append(freeBuild,1,10);
            writeBuild.Culture = new CultureInfo("en-US");
            Grammar writeGrammar = new Grammar(writeBuild);
            writeGrammar.Name = "write";

            GrammarBuilder copyBuild = new GrammarBuilder("control c");
            copyBuild.Culture = new CultureInfo("en-US");
            Grammar copyGrammar = new Grammar(copyBuild);
            copyGrammar.Name = "copy";

            GrammarBuilder pasteBuild = new GrammarBuilder("control v");
            pasteBuild.Culture = new CultureInfo("en-US");
            Grammar pasteGrammar = new Grammar(pasteBuild);
            pasteGrammar.Name = "paste";

            GrammarBuilder showVoicesBuild = new GrammarBuilder("show voices");
            showVoicesBuild.Culture = new CultureInfo("en-US");
            Grammar showVoicesGrammar = new Grammar(showVoicesBuild);
            showVoicesGrammar.Name = "showVoices";

            GrammarBuilder setVoiceBuild = new GrammarBuilder("set voice");
            string[] names = new string[synthen.GetInstalledVoices().Count];
            for (int i = 0; i < names.Length; i++)
                names[i] = synthen.GetInstalledVoices().ElementAt(i).VoiceInfo.Name;
            //setVoiceBuild.Append(new Choices(synthen.GetInstalledVoices().));
            setVoiceBuild.Append(new Choices(names));
            setVoiceBuild.Culture = new CultureInfo("en-US");
            Grammar setVoiceGrammar = new Grammar(setVoiceBuild);
            setVoiceGrammar.Name = "setVoice";

            recognizer.LoadGrammar(voiceGrammar);
            recognizer.LoadGrammar(generalGrammar);
            recognizer.LoadGrammar(writeGrammar);
            recognizer.LoadGrammar(copyGrammar);
            recognizer.LoadGrammar(pasteGrammar);
            recognizer.LoadGrammar(showVoicesGrammar);
            recognizer.LoadGrammar(setVoiceGrammar);
            recognizer.SpeechRecognized +=
              new EventHandler<SpeechRecognizedEventArgs>(OnAudio);

            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        void OnAudio(object sender, SpeechRecognizedEventArgs e)
        {
            LinkedList<string> list;
            int result;

            switch (e.Result.Grammar.Name)
            {
                case "general":
                    Speak("до зустрічі");
                    Application.Exit();
                    break;
                case "voice":
                    list = new LinkedList<string>();
                    foreach (var element in e.Result.Words.ToArray())
                        list.AddLast(element.Text);
                    list.RemoveFirst();
                    bool isVolume = list.First.Value == "volume";
                    list.RemoveFirst();

                    if (isVolume && TryGetValue(list, out result, 0, 100))
                    {
                        volume = result;                                                      
                        SaveSettings();
                        Speak("гучність " + volume);
                    }
                    if (!isVolume && TryGetValue(list, out result, -10, 10))
                    {
                        rate = result;
                        SaveSettings();
                        Speak("швидкість " + rate);
                    }
                    break;
                case "write":
                    long? value = null;
                    label2.Text = "";
                    foreach (var element in e.Result.Words)
                        if (WordsToNumbers.TryConvertToNumbers(element.Text, out long longg))
                        {
                            if (value is null)
                                value = longg;
                            else
                                value += longg;
                        }
                        else
                        {
                            if (value != null)
                                label2.Text += value.ToString() + " ";
                            label2.Text += element.Text + " ";
                            value = null;
                        }
                    if (value != null)
                        label2.Text += value.ToString() + " ";
                    AutoItX.Send(label2.Text.Remove(0,5));
                    break;
                case "copy":
                    //Clipboard.SetText();
                    //Speak("скопійовано");
                    Speak("не скопійовано, команда ще не готова");
                    break;
                case "paste":
                    AutoItX.Send(Clipboard.GetText());
                    Speak("вставлено");
                    break;
                case "showVoices":
                    label2.Text = "";
                    foreach (var element in new SpeechSynthesizer().GetInstalledVoices())
                        label3.Text += element.VoiceInfo.Culture + ", " + element.VoiceInfo.Gender + ", " + element.VoiceInfo.Name + "\n";
                    Speak("ось список");
                    break;
                case "setVoice":
                    voice = e.Result.Text.Remove(0,10);
                    Speak("тепер я "+ voice);
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty)
                Speak(textBox1.Text);
        }
        private Prompt Speak(string text)
        {
            synthen.Rate = rate;
            synthen.Volume = volume;
            synthen.SetOutputToDefaultAudioDevice();

            var builder = new PromptBuilder();
            var settings = new PromptStyle();
            settings.Emphasis = PromptEmphasis.Strong;
            builder.StartStyle(settings);
            builder.StartVoice(synthen.GetInstalledVoices().First(e => e.VoiceInfo.Name == voice).VoiceInfo);
            builder.AppendText(text);
            builder.EndVoice();
            builder.EndStyle();
            return synthen.SpeakAsync(builder);
        }

        void SaveSettings()
        {
            XElement xElement = new XElement("settings");
            xElement.Add(new XElement("rate", rate));
            xElement.Add(new XElement("volume", volume));
            xElement.Add(new XElement("voice", voice));
            xElement.Save("Settings.xml");
        }
        bool TryGetValue(LinkedList<string> value, out int result, int min = 0, int max = 100)
        {
            result = min;

            string s = "";
            foreach (var e in value)
                s += e + " ";

            try
            {
                result = (int)WordsToNumbers.ConvertToNumbers(s);
                return result <= max && result >= min;
            }
            catch { return false; }
        }
    }
    class WordsToNumbers
    {
        public readonly static Dictionary<string, long> NumberTable = new Dictionary<string, long>{
        {"zero",0},{"one",1},{"two",2},{"three",3},{"four",4},{"five",5},{"six",6},
        {"seven",7},{"eight",8},{"nine",9},{"ten",10},{"eleven",11},{"twelve",12},
        {"thirteen",13},{"fourteen",14},{"fifteen",15},{"sixteen",16},{"seventeen",17},
        {"eighteen",18},{"nineteen",19},{"twenty",20},{"thirty",30},{"forty",40},
        {"fifty",50},{"sixty",60},{"seventy",70},{"eighty",80},{"ninety",90},
        {"hundred",100},{"thousand",1000},{"lakh",100000},{"million",1000000},
        {"billion",1000000000},{"trillion",1000000000000},{"quadrillion",1000000000000000},
        {"quintillion",1000000000000000000}
    };

        public static long ConvertToNumbers(string numberString)
        {
            if (Regex.Matches(numberString, @"\w+").Cast<Match>().Select(m => m.Value.ToLowerInvariant()).Any(v => !NumberTable.ContainsKey(v) && v != "minus"))
                throw new Exception("invalid sentence");
            var numbers = Regex.Matches(numberString, @"\w+").Cast<Match>()
                    .Select(m => m.Value.ToLowerInvariant())
                    .Where(v => NumberTable.ContainsKey(v))
                    .Select(v => NumberTable[v]);
            long acc = 0, total = 0L;
            foreach (var n in numbers)
            {
                if (n >= 1000)
                {
                    total += acc * n;
                    acc = 0;
                }
                else if (n >= 100)
                {
                    acc *= n;
                }
                else acc += n;
            }
            return (total + acc) * (numberString.StartsWith("minus",
                    StringComparison.InvariantCultureIgnoreCase) ? -1 : 1);
        }
        public static bool TryConvertToNumbers(string value, out long result)
        {
            try
            {
                result = ConvertToNumbers(value);
            }
            catch
            {
                result = 0;
                return false;
            }
            return true;
        }
    }
}
