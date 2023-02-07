using System.ComponentModel;

namespace Speech2
{
    class SpeakCommand: ICommand
    {
        [Description("скажи 'скажи' і речення яке хоч сказати")]
        public bool Check(string text) => text.Split(" ")[0].Equals("скажи");
        [Description("повторяє за тобою окрім першого слова сказати")]
        public void Execute(Assistant assistant,string text) 
        {
         assistant.Speak(text.Remove(0,6));
        }
    }
}
