using System.Speech.Synthesis;
using System.ComponentModel;
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
                VoiceInfo voiceInfo = e.VoiceInfo;
                assistant.Voice = voiceInfo.Name;
                string text1 = Translator.Translate(voiceInfo.Name, toLanguage: voiceInfo.Culture.Name);
                Task.WaitAll(assistant.Speak(text1));
                assistant.Voice = voice;
            }
        }
    }
}
