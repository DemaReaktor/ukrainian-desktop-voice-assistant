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
using System.Threading;
using System.Diagnostics;
using static Speech2.Assistant;
using static Speech2.SearchWord;

namespace Speech2
{
    public partial class Form1 : Form
    {
        Assistant assistant;

        public Form1()
        {
            assistant = new Assistant("E:\\Инет\\vosk-model-uk-v3\\vosk-model-uk-v3");
            assistant.AddCommand(new Stop());
            assistant.AddCommand(new SpeakControl());
            assistant.AddCommand(new Speak());
            assistant.AddCommand(new Exit());
            assistant.AddCommand(new Volume());
            assistant.AddCommand(new Voice());
            assistant.AddCommand(new Rate());
            assistant.AddCommand(new Translate());
            assistant.AddCommand(new Voices());
            SearchWord searchWord = new SearchWord();
            searchWord.OnGetWord += new EventHandler((object o, EventArgs eventArgs)=>WebBrowser.Url = (eventArgs as WordEventArgs).Uri);
            assistant.AddCommand(searchWord);
            assistant.AddSpeakUpListener((object o, SpeakEventArgs speakEventArgs) => Debug.WriteLine(speakEventArgs.Text));

            InitializeComponent();
            textBox1.Text = "привіт паляниця";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty)
                assistant.Speak(textBox1.Text);
        }

        //виконує команду
        //void Command(string text)
        //{
        //    label1.Text = text;
        //    if (synthen.State != SynthesizerState.Speaking)
        //    {
        //        switch (text)
        //        {
        //            case "команди":
        //                if(!File.Exists("E:\\C#\\Speech2\\Speech2\\Speech2\\bin\\Debug\\net6.0-windows\\commands.html"))
        //                    using (var stream = new StreamWriter("commands.html"))
        //                {
        //                    stream.WriteLine("<!DOCTYPE html>");
        //                    stream.WriteLine("<html>");
        //                    stream.WriteLine("<meta charset=\"UTF-8\">");
        //                    stream.WriteLine("<body>");
        //                    stream.WriteLine("<p>вийти | вихід | закрити | стоп - програма закривається</p>");
        //                    stream.WriteLine("<p>команди - показується список команд</p>");
        //                    stream.WriteLine("<p>скажи \"text\" - проговорюється text</p>");
        //                    stream.WriteLine("<p>гучність \"value(0...10)\" - змінюється голос</p>");
        //                    stream.WriteLine("<p>швидкість \"value(-10...10)\" - змінюється швидкість</p>");
        //                    stream.WriteLine("<p>знайти \"word\" - знаходить дані про слово</p>");
        //                    stream.WriteLine("</body>");
        //                    stream.WriteLine("</html>");
        //                }
        //                WebBrowser.Url = new Uri("E:\\C#\\Speech2\\Speech2\\Speech2\\bin\\Debug\\net6.0-windows\\commands.html");
        //                return;
        //        }
        //    }
        //}
    }
}
