using System.ComponentModel;

namespace Speech2
{
    class StopCommand : ICommand
    {
        bool isStop=false;
        [Description("скажи зупинись або продовжити")]
        public bool Check(string text) => text.Equals("зупинись") || isStop;
        [Description("перестає слухати поки не скажеш продовжити")]
        public void Execute(Assistant assistant, string text)
        {
            if(text == "зупинись")
            {
                isStop = true;
                assistant.Speak("піду чаю поп'ю");
                return;
            }
            if (text == "продовжити")
            {
                isStop=false;
                assistant.Speak("добре, але лише тому що я так захоті" + (assistant.IsWomen? "ла":"в"));
            }
        }
    }
}
