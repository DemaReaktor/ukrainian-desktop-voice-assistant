namespace Speech2
{
    class SpeakCommand: ICommand
    {
        public bool Check(string text) => text.Split(" ")[0].Equals("скажи");
        public void Execute(Assistant assistant,string text) 
        {
         assistant.Speak(text.Remove(0,6));
        }
    }
}
