namespace Speech2
{
    class StopCommand : ICommand
    {
        bool isStop=false;
        public bool Check(string text) => text.Equals("зупинись") || isStop;
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
