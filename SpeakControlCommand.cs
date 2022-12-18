using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Speech2.Assistant;

namespace Speech2
{
    class SpeakControlCommand : ICommand
    {
        bool isSilence = false;
        int volume = 0;
        public bool Check(string text) => text.Equals("коментувати") || text.Equals("не коментувати");
        public void Execute(Assistant assistant, string text)
        {
            if (text == "не коментувати")
            {
                isSilence = true;
                assistant.Speak("все я труп");
                assistant.AddSpeakUpListener(DoSilence);
                assistant.AddSpeakUpListener(StopSilence);
                return;
            }
            if (text == "коментувати")
            {
                isSilence = false;
                assistant.Speak(assistant.IsWomen ? "місіс тарахтолка вернулась" : "містер балабол вернувся");
            }
        }
        void DoSilence(object assistant, TextEventArgs speakEventArgs)
        {
            volume = (assistant as Assistant).Volume;
            (assistant as Assistant).Volume = 0;
        }
        void StopSilence(object assistant, TextEventArgs speakEventArgs)
        {
            (assistant as Assistant).Volume = volume;
        }
    }
}
