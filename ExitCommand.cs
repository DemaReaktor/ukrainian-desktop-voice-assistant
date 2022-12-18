using System.ComponentModel;
using System.Windows.Forms;

namespace Speech2
{
    class ExitCommand : ICommand
    {
        [Description("скажи стоп, вихід, вийти або закрити")]
        public bool Check(string text) => text.Equals("стоп") || text.Equals("вихід") || text.Equals("вийти") || text.Equals("закрити");
        [Description("закриває асистена")]
        public void Execute(Assistant assistant, string text)
        {
            Application.Exit();
        }
    }
}
