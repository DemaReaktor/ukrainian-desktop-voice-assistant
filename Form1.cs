using System;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Xml.Linq;
using System.Linq;
using Vosk;
using NAudio.Wave;

namespace Speech
{
    public partial class Form1 : Form
    {
        int rate;
        int volume;
        string voice;
        SpeechSynthesizer synthen;
        VoskRecognizer voskRecognizer;
        string text;

        private void WaveInOnDataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                if (voskRecognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))  
                    text = voskRecognizer.FinalResult();
            }
            catch{}
        }

        public Form1()
        {
            InitializeComponent();
            voskRecognizer = new VoskRecognizer(new Model("E:\\Инет\\vosk-model-uk-v3\\vosk-model-uk-v3"), 44100f);

            WaveInEvent waveIn = new WaveInEvent();
            waveIn.WaveFormat = new WaveFormat(44100,1);
            waveIn.DataAvailable += WaveInOnDataAvailable;
            waveIn.StartRecording();

            //synthen = new SpeechSynthesizer();
            //textBox1.Text = "привіт паляниця";
            //if (File.Exists("Settings.xml"))
            //{
            //    var xElement = XElement.Load("Settings.xml");
            //    rate = int.Parse(xElement.Element("rate").Value);
            //    volume = int.Parse(xElement.Element("volume").Value);
            //    voice = xElement.Element("voice").Value;
            //}
            //else
            //{
            //    rate = 1;
            //    volume = 50;
            //    voice = "Anatol";
            //    SaveSettings();
            //}
        }

        //void OnAudio(object sender, SpeechRecognizedEventArgs e)
        //{
        //    LinkedList<string> list;
        //    int result;

        //    switch (e.Result.Grammar.Name)
        //    {
        //        case "general":
        //            Speak("до зустрічі");
        //            Application.Exit();
        //            break;
        //        case "voice":
        //            list = new LinkedList<string>();
        //            foreach (var element in e.Result.Words.ToArray())
        //                list.AddLast(element.Text);
        //            list.RemoveFirst();
        //            bool isVolume = list.First.Value == "volume";
        //            list.RemoveFirst();

        //            if (isVolume && TryGetValue(list, out result, 0, 100))
        //            {
        //                volume = result;                                                      
        //                SaveSettings();
        //                Speak("гучність " + volume);
        //            }
        //            if (!isVolume && TryGetValue(list, out result, -10, 10))
        //            {
        //                rate = result;
        //                SaveSettings();
        //                Speak("швидкість " + rate);
        //            }
        //            break;
        //        case "write":
        //            long? value = null;
        //            label2.Text = "";
        //            foreach (var element in e.Result.Words)
        //                if (WordsToNumbers.TryConvertToNumbers(element.Text, out long longg))
        //                {
        //                    if (value is null)
        //                        value = longg;
        //                    else
        //                        value += longg;
        //                }
        //                else
        //                {
        //                    if (value != null)
        //                        label2.Text += value.ToString() + " ";
        //                    label2.Text += element.Text + " ";
        //                    value = null;
        //                }
        //            if (value != null)
        //                label2.Text += value.ToString() + " ";
        //            AutoItX.Send(label2.Text.Remove(0,5));
        //            break;
        //        case "copy":
        //            //Clipboard.SetText();
        //            //Speak("скопійовано");
        //            Speak("не скопійовано, команда ще не готова");
        //            break;
        //        case "paste":
        //            AutoItX.Send(Clipboard.GetText());
        //            Speak("вставлено");
        //            break;
        //        case "showVoices":
        //            label2.Text = "";
        //            foreach (var element in new SpeechSynthesizer().GetInstalledVoices())
        //                label3.Text += element.VoiceInfo.Culture + ", " + element.VoiceInfo.Gender + ", " + element.VoiceInfo.Name + "\n";
        //            Speak("ось список");
        //            break;
        //        case "setVoice":
        //            voice = e.Result.Text.Remove(0,10);
        //            Speak("тепер я "+ voice);
        //            break;
        //    }
        //}

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

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(text!=null && text.Length>17)
            {
                var text1 = text.Remove(0, 14);
                label3.Text = text1.Remove(text1.Length - 3, 3);
                Command(text);
            }
        }

        void Command(string text) 
        {
            switch (text)
            {
                case "вийти":
                    Application.Exit();
                    break;
            }
        }
    }
}
