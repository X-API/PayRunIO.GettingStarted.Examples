namespace PayRunIO.CSharp.SDK.GettingStarted.Examples
{
    using PayRunIO.CSharp.SDK.GettingStarted.Properties;
    using PayRunIO.OAuth1;

    public abstract class ExampleBase : IExample
    {
        protected ExampleBase()
        {
            var oauthSigGen = new OAuthSignatureGenerator();

            this.ApiHelper = new RestApiHelper(
                oauthSigGen, 
                Settings.Default.ConsumerKey, 
                Settings.Default.ConsumerSecret, 
                Settings.Default.ApiEndpoint,
                Settings.Default.ContentTypeHeader,
                Settings.Default.AcceptHeader);
        }

        public RestApiHelper ApiHelper { get; private set; }

        public abstract string Title { get; }

        public abstract string DocsUrl { get; }

        public abstract int Order { get; }

        public abstract void Execute();
    }
}