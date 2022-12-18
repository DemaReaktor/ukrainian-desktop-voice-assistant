using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speech2
{
    class VoicesCommand : ICommand
    {
        [Description("скажи голоси")]
        public bool Check(string text) => text.Equals("голоси");
        [Description("проговорює всі голоси які є, кожен голос кажеться цим же голосом")]
        public void Execute(Assistant assistant, string text)
        {
            foreach (var e in assistant.Voices)
            {
                string voice = assistant.Voice;
                assistant.Voice = e.VoiceInfo.Name;
                assistant.Speak(e.VoiceInfo.Name);
                assistant.Voice = voice;
            }
        }
    }
}
