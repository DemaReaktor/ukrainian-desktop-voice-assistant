namespace Speech2
{
    class VoiceCommand : ICommand
    {
        public bool Check(string text) => text.Split(" ")[0].Equals("голос");
        public void Execute(Assistant assistant, string text)
        {
            assistant.Voice = Translator.Translate(text.Remove(0,6));
            if (assistant.Voice != Translator.Translate(text.Remove(0, 6)))
            {
                assistant.Voice = text.Remove(0, 6);
                if (assistant.Voice != text.Remove(0, 6))
                {
                    assistant.Speak("такого голосу не має або твоя українська не така вже і українська");
                    return;
                }
            }
            assistant.Speak(text.Remove(0,6) +" тепер балакає");
        }
    }
    class Rate : ICommand
    {
        public bool Check(string text) => text.Split(" ")[0].Equals("швидкість");
        public void Execute(Assistant assistant, string text)
        {
            assistant.Rate = WordsToInt.ToInt(text.Remove(0, 6));
            assistant.Speak(assistant.Rate.ToString() + ", шла Саша по шосе і сосала сушку");
        }
    }
    class Volume : ICommand
    {
        public bool Check(string text) => text.Split(" ")[0].Equals("гучність");
        public void Execute(Assistant assistant, string text)
        {
            assistant.Volume = WordsToInt.ToInt(text.Remove(0, 6))* 10;
            assistant.Speak(assistant.Volume.ToString() + ", тепер ти мене чуєш?");
        }
    }
}
