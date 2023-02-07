using System.ComponentModel;
using System.Linq;

namespace Speech2
{
     class CommandsDescriptionCommand : ICommand
    {
        [Description("скажи 'команди'")]
        public bool Check(string text) => text.Equals("команди");
        [Description("показує всі команди + як кожну викликати + що кожна виконує")]
        public void Execute(Assistant assistant, string text)
        {
            foreach (var command in assistant.Commands)
            {
                assistant.Write(new string('-', 5));
                assistant.Write(command.GetType().Name);
                assistant.Write("{");
                try
                {
                    assistant.Write("Check: "+ (command.GetType().GetMember("Check")[0].GetCustomAttributes(false).FirstOrDefault(e => e.GetType() ==
                    typeof(DescriptionAttribute)) as DescriptionAttribute).Description);
                }
                catch{
                    assistant.Write("не відомий виклик команди");
                }
                try
                {
                    assistant.Write("Execute: " + (command.GetType().GetMember("Execute")[0].GetCustomAttributes(false).FirstOrDefault(e => e.GetType() ==
                    typeof(DescriptionAttribute)) as DescriptionAttribute).Description);
                }
                catch
                {
                    assistant.Write("не відоме виконання");
                }
                assistant.Write("}");
                assistant.Write(new string('-', 5));
            }
        }
    }
}
