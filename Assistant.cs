﻿using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using mphweb.Models;
using mphdict;
using mphdict.Models.morph;
using System.IO;
using Microsoft.EntityFrameworkCore;
using uSofTrod.generalTypes.Models;
using System.Xml.Linq;
using System.Speech.Synthesis;
using Vosk;
using NAudio.Wave;
using System.Collections.Generic;
using System.Threading.Tasks;
using Speech2.Properties;
using SpeechLib;
using System.Diagnostics.Tracing;

namespace Speech2
{
    public class Assistant
    {
        public class SpeakEventArgs : EventArgs { public string Text; }
        public delegate void SpeakMethod(object o, SpeakEventArgs speakEventArgs);
        private EventHandler<SpeakEventArgs> OnSpeakUp;
        private EventHandler<SpeakEventArgs> OnSpeakDown;
        private readonly string settingsPath = "AssistantSettings.xml";
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
                    voice = value;
                    SetData();
                }
            }
        }
        private string voice;
        public bool IsWomen { get => synthen.GetInstalledVoices().Any(e => e.VoiceInfo.Name.ToLower() == Voice && e.VoiceInfo.Gender == VoiceGender.Female); }
        public string[] Commands
        {
            get
            {
                string[] names = new string[commands.Count];
                for (int i = 0; i < commands.Count; i++)
                    names[i] = commands.ElementAt(i).GetType().Name;
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

            Task.Run(() =>
            {
                voskRecognizer = new VoskRecognizer(new Model(ModelPath), 44100f);
                while (!isData.IsCompleted) { }
                Speak("я готовий слухати");
            });

            WaveInEvent waveIn = new WaveInEvent();
            waveIn.WaveFormat = new WaveFormat(44100, 1);
            waveIn.DataAvailable += WaveInOnDataAvailable;
            waveIn?.StartRecording();

            commands = new LinkedList<ICommand>();
        }

        private Task GetData() => Task.Run(() =>
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
                Voice = synthen.GetInstalledVoices().First().VoiceInfo.Name.ToLower();
            });
        private Task SetData() => Task.Run(() =>
            {
                XElement xElement = new XElement("settings");
                xElement.Add(new XElement("rate", rate));
                xElement.Add(new XElement("volume", volume));
                xElement.Add(new XElement("voice", Voice));
                xElement.Save(settingsPath);
            });

        public void AddSpeakUpListener(SpeakMethod method) => OnSpeakUp += new EventHandler<SpeakEventArgs>(method);
        public void RemoveSpeakUpListener(SpeakMethod method) => OnSpeakUp -= OnSpeakUp.GetInvocationList().First(e =>
        (e as EventHandler<SpeakEventArgs>).GetInvocationList().First() as SpeakMethod == method) as EventHandler<SpeakEventArgs>;
        public void AddSpeakDownListener(SpeakMethod method) => OnSpeakDown += new EventHandler<SpeakEventArgs>(method);
        public void RemoveSpeakDownListener(SpeakMethod method) => OnSpeakDown -= OnSpeakUp.GetInvocationList().First(e =>
        (e as EventHandler<SpeakEventArgs>).GetInvocationList().First() as SpeakMethod == method) as EventHandler<SpeakEventArgs>;

        public Task Speak(string text) => Task.Run(() =>
        {
            OnSpeakUp?.Invoke(this, new SpeakEventArgs() { Text = text });

            if (!synthen.GetInstalledVoices().Any(e => e.VoiceInfo.Name.ToLower() == Voice.ToLower()))
                return;

            synthen.Rate = rate;
            synthen.Volume = volume;

            var builder = new PromptBuilder();
            builder.StartStyle(new PromptStyle());
            builder.StartVoice(synthen.GetInstalledVoices().First(e => e.VoiceInfo.Name.ToLower() == Voice.ToLower()).VoiceInfo);
            builder.AppendText(text);
            builder.EndVoice();
            builder.EndStyle();
            synthen.SpeakAsync(builder);
            OnSpeakDown?.Invoke(this, new SpeakEventArgs() { Text = text });
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