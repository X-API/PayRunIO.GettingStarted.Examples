namespace PayRunIO.GettingStarted.Examples.Examples
{
    using System;
    using System.Threading;

    using PayRunIO.Core.Enums;
    using PayRunIO.GettingStarted.Examples.Examples.Base;
    using PayRunIO.GettingStarted.Examples.Properties;
    using PayRunIO.Models;

    public class SimpleCis : ExampleBase
    {
        public override string Title => "Simple CIS";

        public override string DocsUrl => "/docs/how-to/simple-cis.html";

        public override int Order => 4;

        public override short TaxYear => 2022;

        public override void Execute()
        {
            Console.WriteLine("Executing Example: " + this.Title);
            Console.WriteLine("See: " + Settings.Default.DeveloperPortalBaseUrl + this.DocsUrl);
            Console.WriteLine("===================================");

            // Step 1: Create an Employer
            Console.WriteLine("Step 1: Create an Employer");

            var employer = new Employer
            {
                EffectiveDate = new DateTime(2020, 4, 6),
                Name = "CIS Employer",
                RuleExclusions = RuleExclusionFlags.None,
                Territory = CalculatorTerritory.UnitedKingdom,
                Region = CalculatorRegion.England,
                HmrcSettings = new HmrcSettings
                {
                    TaxOfficeNumber = "123",
                    TaxOfficeReference = "R395",
                    AccountingOfficeRef = "123PP87654321",
                    Sender = Sender.Employer,
                    SenderId = "CISRUSER1194",
                    Password = "testing1",
                    SAUTR = "7325648155"
                }
            };

            var employerLink = this.ApiHelper.Post("/Employers", employer);
            Console.WriteLine($"  CREATED: {employerLink.Title} - {employerLink.Href}");

            // Step 2: Create Subcontractor
            Console.WriteLine("Step 2: Create a Subcontractor");
            var subContractor = new SubContractor
            {
                EffectiveDate = new DateTime(2020, 4, 6),
                TradingName = "Building Dreams",
                UniqueTaxReference = "1234567882",
                WorksNumber = "SUB001",
                BusinessType = SubContractorType.SoleTrader,
                FirstName = "Sidney",
                LastName = "James",
                NiNumber = "YW000009A",
                Region = CalculatorRegion.England,
                Territory = CalculatorTerritory.UnitedKingdom
            };

            var subcontractorLink = this.ApiHelper.Post(employerLink.Href + "/SubContractors", subContractor);
            Console.WriteLine($"  CREATED: {subcontractorLink.Title} - {subcontractorLink.Href}");

            // Step 3: Verify the Subcontractor
            Console.WriteLine("Step 3: Verify the Subcontractor");
            var verificationJob = new CisVerifyJobInstruction
            {
                Employer = employerLink,
                Timestamp = DateTime.Now,
                Generate = true,
                Transmit = true,
                Declaration = true
            };

            var verifyJobInfoLink = this.ApiHelper.Post("/Jobs/cis", verificationJob);
            Console.WriteLine($"  CREATED: {verifyJobInfoLink.Title} - {verifyJobInfoLink.Href}");

            Console.WriteLine("Polling CIS Verification Job Status");
            while (true)
            {
                Thread.Sleep(1000);

                var verifyJobInfo = this.ApiHelper.Get<JobInfo>(verifyJobInfoLink.Href);
                Console.WriteLine($"  Job Status: {verifyJobInfo.JobStatus} - {verifyJobInfo.Progress:P2}");

                if (verifyJobInfo.JobStatus == JobStatus.Success)
                {
                    break;
                }

                if (verifyJobInfo.JobStatus == JobStatus.Failed)
                {
                    throw new Exception("CIS verification job failed:" + string.Join(Environment.NewLine, verifyJobInfo.Errors));
                }
            }

            subContractor = this.ApiHelper.Get<SubContractor>(subcontractorLink.Href);
            Console.WriteLine($"Verification Number: {subContractor.VerificationNumber}");
            Console.WriteLine($"Taxation Status: {subContractor.TaxationStatus}");

            // Step 4: Create CIS Instructions
            Console.WriteLine("Step 4: Create CIS Instructions");

            var wages = new CisInstruction
            {
                CisLineType = "CISBASIC",
                PayFrequency = SubContractorPayFrequency.Monthly,
                TaxYearStart = this.TaxYear,
                PeriodStart = 1,
                TaxYearEnd = this.TaxYear,
                PeriodEnd = 1,
                UOM = UomBasicPay.Hour,
                Units = 30,
                Value = 50.00m,
                Description = "Wages"
            };

            this.ApiHelper.Post(subcontractorLink.Href + "/CisInstructions", wages);

            var materials = new CisInstruction
            {
                CisLineType = "CISMAT",
                PayFrequency = SubContractorPayFrequency.Monthly,
                TaxYearStart = this.TaxYear,
                PeriodStart = 1,
                TaxYearEnd = this.TaxYear,
                PeriodEnd = 1,
                Value = 500.00m,
                Description = "Materials"
            };

            this.ApiHelper.Post(subcontractorLink.Href + "/CisInstructions", materials);

            // Step 5 - Run the CIS Calculation Job
            Console.WriteLine("Step 5 - Run the CIS Calculation Job");
            var calculationJob = new CisCalculateJobInstruction
            {
                Employer = employerLink,
                PayFrequency = SubContractorPayFrequency.Monthly,
                TaxYear = this.TaxYear,
                TaxPeriod = 1
            };

            var calculationJobInfoLink = this.ApiHelper.Post("/Jobs/cis", calculationJob);
            Console.WriteLine($"  CREATED: {calculationJobInfoLink.Title} - {calculationJobInfoLink.Href}");

            Console.WriteLine("Polling CIS Calculation Job Status");
            while (true)
            {
                Thread.Sleep(1000);

                var calcJobInfo = this.ApiHelper.Get<JobInfo>(calculationJobInfoLink.Href);
                Console.WriteLine($"  Job Status: {calcJobInfo.JobStatus} - {calcJobInfo.Progress:P2}");

                if (calcJobInfo.JobStatus == JobStatus.Success)
                {
                    break;
                }

                if (calcJobInfo.JobStatus == JobStatus.Failed)
                {
                    throw new Exception("CIS calculation job failed:" + string.Join(Environment.NewLine, calcJobInfo.Errors));
                }
            }

            // Step 6 - Perform the Monthly Return (CIS300)
            Console.WriteLine("Step 6 - Perform the Monthly Return (CIS300)");
            var returnJob = new CisReturnJobInstruction
            {
                Employer = employerLink,
                Timestamp = DateTime.Now,
                TaxYear = this.TaxYear,
                TaxMonth = 1,
                Generate = true,
                Transmit = true,
                InformationCorrect = true
            };

            var returnJobInfoLink = this.ApiHelper.Post("/Jobs/cis", returnJob);
            Console.WriteLine($"  CREATED: {returnJobInfoLink.Title} - {returnJobInfoLink.Href}");

            Console.WriteLine("Polling CIS Return Job Status");
            while (true)
            {
                Thread.Sleep(1000);

                var returnJobInfo = this.ApiHelper.Get<JobInfo>(calculationJobInfoLink.Href);
                Console.WriteLine($"  Job Status: {returnJobInfo.JobStatus} - {returnJobInfo.Progress:P2}");

                if (returnJobInfo.JobStatus == JobStatus.Success)
                {
                    break;
                }

                if (returnJobInfo.JobStatus == JobStatus.Failed)
                {
                    throw new Exception("CIS Return job failed:" + string.Join(Environment.NewLine, returnJobInfo.Errors));
                }
            }

            // End of examples
            Console.WriteLine(string.Empty);
            Console.WriteLine("-- THE END --");
        }
    }
}
