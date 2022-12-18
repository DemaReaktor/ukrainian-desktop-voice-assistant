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
                assistant.Write(command.GetType().Name);
                assistant.Write("{");
                try
                {
                    assistant.Write("Check: "+(command.GetType().FindMembers(System.Reflection.MemberTypes.Method, System.Reflection.BindingFlags.Public,
                    null, null).First(e => e.Name == "Check").GetCustomAttributes(false).FirstOrDefault(e => e.GetType() ==
                    typeof(DescriptionAttribute)) as DescriptionAttribute).Description);
                }
                catch{
                    assistant.Write("не відомий виклик команди");
                }
                try
                {
                    assistant.Write("Execute: " + (command.GetType().FindMembers(System.Reflection.MemberTypes.Method, System.Reflection.BindingFlags.Public,
                    null, null).First(e => e.Name == "Execute").GetCustomAttributes(false).FirstOrDefault(e => e.GetType() ==
                    typeof(DescriptionAttribute)) as DescriptionAttribute).Description);
                }
                catch
                {
                    assistant.Write("не відоме виконання");
                }
                assistant.Write("}");
            }
        }
    }
}
