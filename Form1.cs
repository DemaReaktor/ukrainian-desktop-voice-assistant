using System;
using System.Windows.Forms;
using static Speech2.Assistant;
using static Speech2.SearchWordCommand;

namespace Speech2
{
    public partial class Form1 : Form
    {
        Assistant assistant;

        public Form1()
        {
            assistant = new Assistant("E:\\Инет\\vosk-model-uk-v3\\vosk-model-uk-v3");
            assistant.AddCommand(new StopCommand());
            assistant.AddCommand(new CommentCommand());
            assistant.AddCommand(new SpeakCommand());
            assistant.AddCommand(new ExitCommand());
            assistant.AddCommand(new Volume());
            assistant.AddCommand(new VoiceCommand());
            assistant.AddCommand(new Rate());
            assistant.AddCommand(new TranslateCommand());
            assistant.AddCommand(new VoicesCommand());
            SearchWordCommand searchWord = new SearchWordCommand();
            searchWord.OnGetWord += new EventHandler((object o, EventArgs eventArgs)=>WebBrowser.Url = (eventArgs as WordEventArgs).Uri);
            assistant.AddCommand(searchWord);
            CommandsDescriptionCommand commandsDescriptionCommand = new CommandsDescriptionCommand();
            assistant.AddSpeakUpListener(AddListBoxText);
            assistant.AddWriteListener(AddListBoxText);
            assistant.AddCommand(commandsDescriptionCommand);

            InitializeComponent();
            textBox1.Text = "привіт паляниця";
        }
        public void AddListBoxText(object o, TextEventArgs textEventArgs)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<object,TextEventArgs>(AddListBoxText), new object[] { o,textEventArgs });
                return;
            }
            listBox1.Items.Add(textEventArgs.Text);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty)
                assistant.Speak(textBox1.Text);
        }
    }
}
