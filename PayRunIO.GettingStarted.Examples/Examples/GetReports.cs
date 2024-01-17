namespace PayRunIO.GettingStarted.Examples.Examples
{
    using System;
    using System.IO;
    using System.Text;
    using PayRunIO.CSharp.SDK;
    using PayRunIO.GettingStarted.Examples.Examples.Base;
    using PayRunIO.GettingStarted.Examples.Properties;
    using PayRunIO.Models;
    using PayRunIO.Utilities;

    public class GetReports : ExampleBase
    {
        public override string Title => "Get Reports";

        public override string DocsUrl => "/docs/";

        public override int Order => 7;

        public override short TaxYear => 2023;

        public const string OutputFolder = @"C:\Development\Payescape.PayRunIO.ReportTransforms\Payescape.PayRunIO.ReportTransforms\Payescape.PayRunIO.ReportTransforms\ReportDefinitions\Custom";

        public override void Execute()
        {
            Console.WriteLine("Executing: " + this.Title);
            Console.WriteLine("See: " + Settings.Default.DeveloperPortalBaseUrl + this.DocsUrl);
            Console.WriteLine("===================================");

            var allReports = this.ApiHelper.GetLinks("/Reports").Links;

            foreach (var reportLink in allReports)
            {
                var reportDef = this.ApiHelper.Get<ReportDefinition>(reportLink.Href);
                if (reportDef.Readonly)
                {
                    continue;
                }

                Console.WriteLine(reportLink.Href);

                var reportKey = reportLink.ExtractKey("Report");
                var reportXml = this.ApiHelper.GetRawXml(reportLink.Href);

                System.IO.File.WriteAllText(Path.Combine(OutputFolder, reportKey + ".xml"), reportXml.Beautify().Replace(" encoding=\"utf-16\"", string.Empty), Encoding.UTF8);
            }

            // End of examples
            Console.WriteLine(string.Empty);
            Console.WriteLine("-- THE END --");
        }
    }
}