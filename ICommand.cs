namespace Speech2
{
    public interface ICommand
    {
        bool Check(string text);
        void Execute(Assistant assistant,string text);
    }
}
