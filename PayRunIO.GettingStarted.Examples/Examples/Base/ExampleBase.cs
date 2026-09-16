using PayRunIO.v2.OAuth1;

namespace PayRunIO.GettingStarted.Examples.Examples.Base
{
    using PayRunIO.v2.CSharp.SDK;
    using PayRunIO.GettingStarted.Examples.Properties;
    using PayRunIO.v2.OAuth1;

    public abstract class ExampleBase : IExample
    {
        protected ExampleBase()
        {
            var oauthSigGen = new OAuthSignatureGenerator();

            this.ApiHelper = new HttpRestApiHelper(
                oauthSigGen, 
                Settings.Default.ConsumerKey, 
                Settings.Default.ConsumerSecret, 
                Settings.Default.ApiEndpoint,
                Settings.Default.ContentTypeHeader,
                Settings.Default.AcceptHeader);

            //this.ApiHelper.ApiVersionHeader = "v2";
        }

        public HttpRestApiHelper ApiHelper { get; private set; }

        public abstract string Title { get; }

        public abstract string DocsUrl { get; }

        public abstract int Order { get; }

        public abstract short TaxYear { get; } 

        public abstract void Execute();
    }
}