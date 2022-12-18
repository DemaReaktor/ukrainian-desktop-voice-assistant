using System.ComponentModel;

namespace Speech2
{
    class VoiceCommand : ICommand
    {
        [Description("скажи голос і назву голоса")]
        public bool Check(string text) => text.Split(" ")[0].Equals("голос");
        [Description("міняє голос якщо такий є")]
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
        [Description("скажи швидкість і число від -10 до 10")]
        public bool Check(string text) => text.Split(" ")[0].Equals("швидкість");
        [Description("змінює швидкість голосу")]
        public void Execute(Assistant assistant, string text)
        {
            assistant.Rate = WordsToInt.ToInt(text.Remove(0, 6));
            assistant.Speak(assistant.Rate.ToString() + ", шла Саша по шосе і сосала сушку");
        }
    }
    class Volume : ICommand
    {
        [Description("скажи гучність і число від 0 до 10")]
        public bool Check(string text) => text.Split(" ")[0].Equals("гучність");
        [Description("змінює гучність голосу")]
        public void Execute(Assistant assistant, string text)
        {
            assistant.Volume = WordsToInt.ToInt(text.Remove(0, 6))* 10;
            assistant.Speak(assistant.Volume.ToString() + ", тепер ти мене чуєш?");
        }
    }
}
