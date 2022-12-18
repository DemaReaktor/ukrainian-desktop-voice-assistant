using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speech2
{
    class TranslateCommand : ICommand
    {
        [Description("скажи переклади і слово яке хоч перекласти")]
        public bool Check(string text) => text.Split(" ")[0].Equals("переклади");
        [Description("каже перекладене слово")]
        public void Execute(Assistant assistant, string text)
        {
            if(assistant.Voices.Any(e => e.VoiceInfo.Culture.Name == "en-US"))
            {
                string voice = assistant.Voice;
                assistant.Voice = assistant.Voices.FirstOrDefault(e => e.VoiceInfo.Culture.Name == "en-US").VoiceInfo.Name;
                assistant.Speak(Translator.Translate(text.Remove(0, 10)));
                assistant.Voice = voice;
            }
        }
    }
}
