using System;
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

namespace Speech2
{
    public partial class Form1 : Form
    {
        DataB context;
        int rate;
        int volume;
        string voice;
        SpeechSynthesizer synthen;
        VoskRecognizer voskRecognizer;
        KeyValuePair<bool, string> Said;

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
                    if(text.Length>0)
                    Said = new KeyValuePair<bool, string>(true,text);
                }
            }
            catch { }
        }

        public Form1()
        {
            Task.Run(async () => {
            voskRecognizer = new VoskRecognizer(new Model("E:\\Инет\\vosk-model-uk-v3\\vosk-model-uk-v3"), 44100f);
               Speak("я готовий слухати");
                });

            context = new DataB(new DbContextOptions<DataB>());

            synthen = new SpeechSynthesizer();

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

            WaveInEvent waveIn = null;
            waveIn = new WaveInEvent();
            waveIn.WaveFormat = new WaveFormat(44100, 1);
            waveIn.DataAvailable += WaveInOnDataAvailable;
            waveIn?.StartRecording();

            InitializeComponent();
            textBox1.Text = "привіт паляниця";

            Speak("я загрузився");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty)
                Speak(textBox1.Text);
        }
        //каже те що у тексті 
        private void Speak(string text)
        {
            synthen.Rate = rate;
            synthen.Volume = volume;
            synthen.SetOutputToDefaultAudioDevice();

            var builder = new PromptBuilder();
            var settings = new PromptStyle();
            settings.Emphasis = PromptEmphasis.Moderate;
            builder.StartStyle(settings);
            builder.StartVoice(synthen.GetInstalledVoices().First(e => e.VoiceInfo.Name == voice).VoiceInfo);
            builder.AppendText(text);
            builder.EndVoice();
            builder.EndStyle();
            synthen.SpeakAsync(builder);
        }

        //зберігає налаштування голосу
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
            //якщо було щось сказано але ще не виконано
            if (Said.Key)
            {
                Command(Said.Value);
                Said= new KeyValuePair<bool, string>(false,null);
            }
        }

        //виконує команду
        void Command(string text)
        {
            label1.Text = text;
            if (synthen.State != SynthesizerState.Speaking)
            {
                switch (text)
                {
                    case "вихід":
                    case "вийти":
                    case "закрити":
                    case "стоп":
                        Application.Exit();
                        break;
                    case "команди":
                        if(!File.Exists("E:\\C#\\Speech2\\Speech2\\Speech2\\bin\\Debug\\net6.0-windows\\commands.html"))
                            using (var stream = new StreamWriter("commands.html"))
                        {
                            stream.WriteLine("<!DOCTYPE html>");
                            stream.WriteLine("<html>");
                            stream.WriteLine("<meta charset=\"UTF-8\">");
                            stream.WriteLine("<body>");
                            stream.WriteLine("<p>вийти | вихід | закрити | стоп - програма закривається</p>");
                            stream.WriteLine("<p>команди - показується список команд</p>");
                            stream.WriteLine("<p>скажи \"text\" - проговорюється text</p>");
                            stream.WriteLine("<p>гучність \"value(0...10)\" - змінюється голос</p>");
                            stream.WriteLine("<p>швидкість \"value(-10...10)\" - змінюється швидкість</p>");
                            stream.WriteLine("<p>знайти \"word\" - знаходить дані про слово</p>");
                            stream.WriteLine("</body>");
                            stream.WriteLine("</html>");
                        }
                        WebBrowser.Url = new Uri("E:\\C#\\Speech2\\Speech2\\Speech2\\bin\\Debug\\net6.0-windows\\commands.html");
                        return;
                }

                if (text.Split(' ')[0] == "скажи")
                    Speak(text.Remove(0, 6));
                else
                    if (text.Split(' ')[0] == "гучність")
                {
                    volume = WordsToInt.ToInt(text.Remove(0, 9))*10;
                    SaveSettings();
                    Speak(volume.ToString() + ", тепер ти мене чуєш?");
                }
                else
                    if (text.Split(' ')[0] == "швидкість" && text.Length > 10)
                {
                    rate = WordsToInt.ToInt(text.Remove(0, 10));
                    SaveSettings();
                    Speak(rate.ToString() + ", шла Саша по шосе і сосала сушку");
                }
                if (text.Split(' ')[0] == "знайти" && text.Length > 7)
                {
                    string word = text.Remove(0, 7);
                    GetWord(word);
                    Speak("слово" + word);
                }
            }
        }

        //показує розбір слова
        void GetWord(string word)
        {
            string templ = "", str = "";
            alphadigit[] al = (from c in context.alphadigits orderby c.digit, c.ls select c).ToArray();
            var q = (from c in context.words_list select c);
            var w = sharedTypes.atod(word, al);
            q = q.Where(c => EF.Functions.Like(c.digit, w));
            q = q.OrderBy(c => c.digit).ThenBy(c => c.field2);
            q = q.Skip((from c in q where w.CompareTo(c.digit) > 0 select c).Count() - 1).Take(1);
            word_param item = q.First();
            item.parts = (from c in context.parts.AsNoTracking() where c.id == item.part select c).First();
            item.indents = (from c in context.indents.AsNoTracking() where c.type == item.type select c).First();
            item.indents.flexes = (from c in context.flexes.AsNoTracking() where (c.type == item.type && (c.field2 > 0)) orderby c.field2, c.id select c).ToList();
            item.accents_class = (from c in context.accents_class.AsNoTracking() select c).First();
            item.accents_class.accents = (from c in context.accent.AsNoTracking() where c.accent_type == item.accent select c).ToArray();

            if (item.field7 != null)
                str = item.field7.Replace("<", "&#60;").Replace(">", "&#62;");
            str += "<br>";
            if (item.field6 != null)
                str += item.field6.Replace("<", "&#60;").Replace(">", "&#62;");
            str += "<br>";
            if (item.indents.comment != null)
                str += item.indents.comment.Replace("<", "&#3C;").Replace(">", "&#3E;");
            string rdv = string.Empty;
            templ += mphEntry.generateTempl(item, out rdv);

            templ = templ.Replace("[WORD]", item.reestr.Replace("\"", ""));
            templ = templ.Replace("[gram]", item.parts.com);
            str = str.Replace("$", rdv);
            templ = templ.Replace("*[text]", str);
            if (item.field5 != null)
                templ = templ.Replace("[(sem comment)]", item.field5.Replace("<", "&#60;").Replace(">", "&#62;"));
            else templ = templ.Replace("[(sem comment)]", "");
            using (var stream = new StreamWriter("word.html"))
            {
                stream.WriteLine("<!DOCTYPE html>");
                stream.WriteLine("<html>");
                stream.WriteLine("<meta charset=\"UTF-8\">");
                stream.WriteLine("<body>");
                stream.WriteLine(templ);
                stream.WriteLine("</body>");
                stream.WriteLine("</html>");
            }
            WebBrowser.Url = new Uri("E:\\C#\\Speech2\\Speech2\\Speech2\\bin\\Debug\\net6.0-windows\\word.html");
        }

        //база даних українського словника
        class DataB : DbContext
        {
            public DbSet<word_param> words_list { get; set; }
            public DbSet<indents> indents { get; set; }
            public DbSet<flexes> flexes { get; set; }
            public DbSet<alphadigit> alphadigits { get; set; }
            public DbSet<parts> parts { get; set; }
            public DbSet<accents_class> accents_class { get; set; }
            public DbSet<accent> accent { get; set; }

            public DataB(DbContextOptions<DataB> options) : base(options) { }
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite("datasource=E:\\C#\\Lemitologia\\mphdict\\src\\data/mph_ua.db");
            }
            protected override void OnModelCreating(ModelBuilder /*DbModelBuilder*/ modelBuilder)
            {
                modelBuilder.Entity<word_param>().ToTable("nom");

                modelBuilder.Entity<alphadigit>().ToTable("alphadigit", "dbo");
                modelBuilder.Entity<alphadigit>()
               .HasKey(c => new { c.lang, c.alpha, c.ls });
                modelBuilder.Entity<word_param>().HasIndex(b => b.accent);
                modelBuilder.Entity<word_param>().HasIndex(b => b.digit);
                modelBuilder.Entity<word_param>().HasIndex(b => b.reverse);
                modelBuilder.Entity<word_param>().HasIndex(b => b.isdel);
                modelBuilder.Entity<word_param>().HasIndex(b => b.part);
                modelBuilder.Entity<word_param>().HasIndex(b => b.reestr);
                modelBuilder.Entity<word_param>().HasIndex(b => b.type);
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
}
