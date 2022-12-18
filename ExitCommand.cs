using System.Windows.Forms;

namespace Speech2
{
    class ExitCommand : ICommand
    {
        public bool Check(string text) => text.Equals("стоп") || text.Equals("вихід") || text.Equals("вийти") || text.Equals("закрити");
        public void Execute(Assistant assistant, string text)
        {
            Application.Exit();
        }
    }
}
