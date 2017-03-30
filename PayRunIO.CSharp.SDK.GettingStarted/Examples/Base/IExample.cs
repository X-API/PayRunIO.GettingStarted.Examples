namespace PayRunIO.CSharp.SDK.GettingStarted.Examples
{
    public interface IExample
    {
        string Title { get; }

        string DocsUrl { get; }

        int Order { get; }

        void Execute();
    }
}
