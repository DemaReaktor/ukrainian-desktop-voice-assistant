using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speech2
{
    class VoicesCommand : ICommand
    {
        public bool Check(string text) => text.Equals("голоси");
        public void Execute(Assistant assistant, string text)
        {
            foreach (var e in assistant.Voices)
                assistant.Speak(e.VoiceInfo.Name);
        }
    }
}
