namespace PayRunIO.GettingStarted.Examples.Examples.Base
{
    public interface IExample
    {
        string Title { get; }

        string DocsUrl { get; }

        int Order { get; }

        void Execute();
    }
}
