// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimplePayrun.cs" company="">
//   PayRun.IO 2017
// </copyright>
// <summary>
//   Defines the SimplePayrun type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PayRunIO.GettingStarted.Examples.Examples
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;

    using PayRunIO.Core.Enums;
    using PayRunIO.GettingStarted.Examples.Examples.Base;
    using PayRunIO.GettingStarted.Examples.Properties;
    using PayRunIO.Models;
    using PayRunIO.Utilities;

    public class GetDpsMessages : ExampleBase
    {
        public override string Title => "Get DPS Messages";

        public override string DocsUrl => "/docs/key-concepts/data-provisioning-service.html";

        public override int Order { get; } = 6;

        public override short TaxYear { get; } = 2023;

        public override void Execute()
        {
            Console.WriteLine("Executing Example: " + this.Title);
            Console.WriteLine("See: " + Settings.Default.DeveloperPortalBaseUrl + this.DocsUrl);
            Console.WriteLine("===================================");

            // Step 1: Create an Employer
            Console.WriteLine("Step 1: Create an Employer");

            var employer = new Employer
            {
                EffectiveDate = new DateTime(2020, 1, 1),
                Name = "DPS Example Ltd",
                BacsServiceUserNumber = "123456",
                RuleExclusions = RuleExclusionFlags.None,
                Territory = CalculatorTerritory.UnitedKingdom,
                Region = CalculatorRegion.England,
                Address = new Address
                {
                    Address1 = "House",
                    Address2 = "Street",
                    Address3 = "Town",
                    Address4 = "County",
                    Postcode = "TE1 1ST",
                    Country = "United Kingdom"
                },
                HmrcSettings = new HmrcSettings
                {
                    TaxOfficeNumber = "123",
                    TaxOfficeReference = "AN64312",
                    AccountingOfficeRef = "123PA1234567X",
                    Sender = Sender.Employer,
                    SenderId = "ISV451",
                    Password = "testing1",
                    ContactFirstName = "Joe",
                    ContactLastName = "Bloggs",
                    ContactEmail = "Joe.Bloggs@PayRunIO.co.uk",
                    ContactTelephone = "01234567890",
                    ContactFax = "01234567890"
                },
                BankAccount = new BankAccount
                {
                    AccountName = "Getting St",
                    AccountNumber = "12345678",
                    SortCode = "012345"
                }
            };

            var employerLink = this.ApiHelper.Post("/Employers", employer);
            Console.WriteLine($"  CREATED: {employerLink.Title} - {employerLink.Href}");

            // Step 2: Create DPS message retrieval job.
            Console.WriteLine("Step 5: Create a DPS message retrieval Job");
            var payRunJob = new DpsJobInstruction
            {
                Retrieve = true,
                Apply = true,
                Employer = employerLink,
                MessageTypes = new Collection<DpsMessageType>(
                    new[]
                        {
                            DpsMessageType.P6, 
                            DpsMessageType.P9, 
                            DpsMessageType.SL1, 
                            DpsMessageType.SL2, 
                            DpsMessageType.AR, 
                            DpsMessageType.NOT, 
                            DpsMessageType.RTI, 
                            DpsMessageType.PGL1, 
                            DpsMessageType.PGL2
                        })
            };

            var jobInfoLink = this.ApiHelper.Post("/Jobs/dps", payRunJob);
            Console.WriteLine($"  CREATED: {jobInfoLink.Title} - {jobInfoLink.Href}");

            // Step 3: Query DPS Job Status
            Console.WriteLine("Step 3: Query DPS Job Status");
            while (true)
            {
                Thread.Sleep(1000);

                var payRunJobInfo = this.ApiHelper.Get<JobInfo>(jobInfoLink.Href);
                Console.WriteLine($"  Job Status: {payRunJobInfo.JobStatus} - {payRunJobInfo.Progress:P2}");

                if (payRunJobInfo.JobStatus == JobStatus.Success)
                {
                    break;
                }

                if (payRunJobInfo.JobStatus == JobStatus.Failed)
                {
                    throw new Exception("Dps job failed:" + string.Join(Environment.NewLine, payRunJobInfo.Errors));
                }
            }

            // Step 4: List retrieved DPS messages
            Console.WriteLine("Step 4: List retrieved DPS messages");

            var employerId = employerLink.Href.Split('/').Last();

            var dpsReport =
                this.ApiHelper.GetRawXml($"/Report/DPSMSG/run?EmployerKey={employerId}&FromDate={employer.EffectiveDate:yyyy-MM-dd}&ToDate={employer.EffectiveDate.AddYears(1):yyyy-MM-dd}");

            Console.WriteLine(dpsReport.Beautify());

            // End of examples
            Console.WriteLine(string.Empty);
            Console.WriteLine("-- THE END --");
        }
    }
}
