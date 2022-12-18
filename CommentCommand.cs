using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Speech2.Assistant;

namespace Speech2
{
    class CommentCommand : ICommand
    {
        bool isSilence = false;
        int volume = 0;
        [Description("скажи 'коментувати' або 'не коментувати'")]
        public bool Check(string text) => text.Equals("коментувати") || text.Equals("не коментувати");
        [Description("перестає або починає коментувати дії які виконав")]
        public void Execute(Assistant assistant, string text)
        {
            if (text == "не коментувати" && !isSilence)
            {
                isSilence = true;
                assistant.Speak("все я труп");
                assistant.AddSpeakUpListener(DoSilence);
                assistant.AddSpeakUpListener(StopSilence);
                return;
            }
            if (text == "коментувати")
            {
                if (!isSilence)
                {
                    assistant.Speak(assistant.IsWomen ? "наче до цього я мовчала" : "ех, а раніше то мовчав");
                    return;
                }
                    isSilence = false;
                assistant.AddSpeakUpListener(DoSilence);
                assistant.AddSpeakUpListener(StopSilence);
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
