using System;
using System.IO;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Xml.Linq;
using System.Linq;
using Vosk;
using NAudio.Wave;
using System.Collections.Generic;
using mphdict.Models.morph;
using mphdict;
using System.Data.Entity;
using System.Data.Entity.Core;

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
            var db = new mphDb();
            db.getContext().words_list;
            InitializeComponent();
            voskRecognizer = new VoskRecognizer(new Model("E:\\Инет\\vosk-model-uk-v3\\vosk-model-uk-v3"), 44100f);

            WaveInEvent waveIn = new WaveInEvent();
            waveIn.WaveFormat = new WaveFormat(44100,1);
            waveIn.DataAvailable += WaveInOnDataAvailable;
            waveIn.StartRecording();

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
                volume = 5;
                voice = "Anatol";
                SaveSettings();
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
            synthen.Volume = volume*10;
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
                Command(label3.Text);
            }
        }

        void Command(string text) 
        {
            if (synthen.State != SynthesizerState.Speaking)
            {
                switch (text)
                {
                    case "вийти":
                        Application.Exit();
                        break;
                }

                if (text.Split(' ')[0] == "скажи")
                    Speak(text.Remove(0, 6));
                else
                    if(text.Split(' ')[0] == "гучність")
                {
                    volume = WordsToInt.ToInt(text.Remove(0,9));
                    SaveSettings();
                    Speak(volume.ToString() + ", тепер ти мене чуєш?");
                }
                else
                    if (text.Split(' ')[0] == "швидкість" && text.Length>10)
                {
                    rate = WordsToInt.ToInt(text.Remove(0, 10));
                    SaveSettings();
                    Speak(rate.ToString() + ", шла Саша по шосе і сосала сушку");
                }

                text = null;
            }
        }
    }
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
            return isNegative? -result: result;
        }
    }
}
