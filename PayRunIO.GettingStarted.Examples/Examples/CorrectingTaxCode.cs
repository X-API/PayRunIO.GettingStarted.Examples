namespace PayRunIO.GettingStarted.Examples.Examples
{
    using System;
    using System.Linq;
    using System.Threading;

    using PayRunIO.Core.Enums;
    using PayRunIO.GettingStarted.Examples.Examples.Base;
    using PayRunIO.GettingStarted.Examples.Properties;
    using PayRunIO.Models;

    public class CorrectingTaxCodeExample : ExampleBase
    {
        public override string Title => "Correcting a Tax Code";

        public override string DocsUrl => "";

        public override int Order => 3;

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
                EffectiveDate = new DateTime(this.TaxYear, 8, 1),
                Name = "Getting Started Co Ltd",
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
                    TaxOfficeNumber = "451",
                    TaxOfficeReference = "A451",
                    AccountingOfficeRef = "123PA1234567X",
                    Sender = Sender.Employer,
                    SenderId = "ISV451",
                    Password = "testing1",
                    ContactFirstName = "Joe",
                    ContactLastName = "Bloggs",
                    ContactEmail = "Jow.Bloggs@PayRunIO.co.uk",
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

            // Step 2: Create a Pay Schedule
            Console.WriteLine("Step 2: Create a Pay Schedule");
            var paySchedule = new PaySchedule
            {
                Name = "My Weekly",
                PayFrequency = PayFrequency.Weekly
            };

            var payScheduleLink = this.ApiHelper.Post(employerLink.Href + "/PaySchedules", paySchedule);
            Console.WriteLine($"  CREATED: {payScheduleLink.Title} - {payScheduleLink.Href}");

            // Step 3: Create an Employee
            Console.WriteLine("Step 3: Create an Employee");
            var employee = new Employee
            {
                EffectiveDate = new DateTime(this.TaxYear, 8, 1),
                Code = "EMP001",
                Title = "Mr",
                FirstName = "Terry",
                MiddleName = "T",
                LastName = "Tester",
                Initials = "TTT",
                NiNumber = "AA000000A",
                DateOfBirth = new DateTime(1980, 1, 1),
                Gender = Gender.Male,
                NicLiability = NicLiability.IsFullyLiable,
                Region = CalculatorRegion.England,
                Territory = CalculatorTerritory.UnitedKingdom,
                PaySchedule = payScheduleLink,
                StartDate = new DateTime(this.TaxYear, 8, 1),
                StarterDeclaration = StarterDeclaration.A,
                RuleExclusions = RuleExclusionFlags.None,
                WorkingWeek = WorkingWeek.AllWeekDays,
                Address = new Address
                {
                    Address1 = "House",
                    Address2 = "Street",
                    Address3 = "Town",
                    Address4 = "County",
                    Postcode = "TE1 1ST",
                    Country = "United Kingdom"
                },
                HoursPerWeek = 40,
                PassportNumber = "123457890"
            };

            var employeeLink = this.ApiHelper.Post(employerLink.Href + "/Employees", employee);
            Console.WriteLine($"  CREATED: {employeeLink.Title} - {employeeLink.Href}");

            // Step 4: Create a Pay Instruction (Salary)
            Console.WriteLine("Step 4: Create a Pay Instruction (Salary)");
            var salaryInstruction = new SalaryPayInstruction
            {
                StartDate = new DateTime(this.TaxYear, 8, 1),
                AnnualSalary = 25000.00m
            };

            var salaryInstructionLink = this.ApiHelper.Post(employeeLink.Href + "/PayInstructions", salaryInstruction);
            Console.WriteLine($"  CREATED: {salaryInstructionLink.Title} - {salaryInstructionLink.Href}");

            // Step 5: Create a Pay Instruction (Salary)
            Console.WriteLine("Step 5: Create a Tax Instruction (1185L)");
            var taxInstruction = new TaxPayInstruction
            {
                StartDate = new DateTime(this.TaxYear, 8, 1),
                TaxCode = "1185L"
            };

            var taxInstructionLink = this.ApiHelper.Post(employeeLink.Href + "/PayInstructions", taxInstruction);
            Console.WriteLine($"  CREATED: {taxInstructionLink.Title} - {taxInstructionLink.Href}");

            // Step 6: Create a Pay Run Job
            Console.WriteLine("Step 6: Create a Pay Run Job");
            var payRunJob = new PayRunJobInstruction
            {
                PaymentDate = new DateTime(this.TaxYear, 8, 17),
                StartDate = new DateTime(this.TaxYear, 8, 13),
                EndDate = new DateTime(this.TaxYear, 8, 19),
                PaySchedule = payScheduleLink
            };

            var jobInfoLink = this.ApiHelper.Post("/Jobs/Payruns", payRunJob);
            Console.WriteLine($"  CREATED: {jobInfoLink.Title} - {jobInfoLink.Href}");

            // Step 7: Query Pay Run Job Status
            Console.WriteLine("Step 7: Query Pay Run Job Status");
            this.PollPayRunJobStatus(jobInfoLink);

            // Step 8: Create 2nd Pay Run Job
            Console.WriteLine("Step 8: Create a 2nd Pay Run Job");
            var payRunJob2 = new PayRunJobInstruction
            {
                PaymentDate = new DateTime(this.TaxYear, 8, 24),
                StartDate = new DateTime(this.TaxYear, 8, 20),
                EndDate = new DateTime(this.TaxYear, 8, 26),
                PaySchedule = payScheduleLink
            };

            var jobInfoLink2 = this.ApiHelper.Post("/Jobs/Payruns", payRunJob2);
            Console.WriteLine($"  CREATED: {jobInfoLink2.Title} - {jobInfoLink2.Href}");

            // Step 9: Query Pay Run Job Status
            Console.WriteLine("Step 9: Query Pay Run Job Status");
            this.PollPayRunJobStatus(jobInfoLink2);

            // Step 10: Query Pay Run Job Status
            Console.WriteLine("Step 10: Recieve late notification of employee tax code change, requires retrospective correction.");
            var newTaxInstruction = new TaxPayInstruction
            {
                StartDate = new DateTime(this.TaxYear, 8, 20),
                TaxCode = "1285L"
            };

            // Step 11: Get all payruns for employee
            Console.WriteLine("Step 11: Get all PayRuns for Employee");
            var payruns = this.ApiHelper.GetLinks(employeeLink.Href + "/PayRuns");
            foreach (var link in payruns.Links)
            {
                Console.WriteLine($"  -- {link.Title} ({link.Href})");
            }

            // Step 12: Loop Employee PayRuns and DELETE any with payment date after new tax code effective date.
            Console.WriteLine("Step 12: Loop Employee PayRuns and DELETE any with payment date after new tax code effective date.");
            Console.WriteLine($"New tax code effective date: {newTaxInstruction.StartDate:yyyy-MM-dd}");

            foreach (var link in payruns.Links)
            {
                var payrun = this.ApiHelper.Get<PayRun>(link.Href);
                if (payrun.PaymentDate >= newTaxInstruction.StartDate)
                {
                    this.ApiHelper.Delete(link.Href);
                    Console.WriteLine($"  -- DELETE {link.Title} ({link.Href})");
                }
            }

            // Step 13: End Existing Tax Instruction
            Console.WriteLine("Step 13: End Existing Tax Instruction");
            taxInstruction.EndDate = newTaxInstruction.StartDate.AddDays(-1);
            taxInstruction = this.ApiHelper.Put<TaxPayInstruction>(taxInstructionLink.Href, taxInstruction);
            Console.WriteLine($"  UPDATE: {jobInfoLink2.Title} successfully ended {taxInstruction.EndDate.GetValueOrDefault():yyyy-MM-dd}");

            // Step 14: Create new Tax Instruction (1285L)
            Console.WriteLine("Step 14: Create new Tax Instruction (1285L)");
            var newTaxInstructionLink = this.ApiHelper.Post(employeeLink.Href + "/PayInstructions", newTaxInstruction);
            Console.WriteLine($"  CREATED: {newTaxInstructionLink.Title} - {newTaxInstructionLink.Href}");

            // Step 15: Re-queue 2nd Payrun Job
            Console.WriteLine("Step 15: Re-queue 2nd Payrun Job");
            var jobInfoLink3 = this.ApiHelper.Post("/Jobs/Payruns", payRunJob2);
            Console.WriteLine($"  CREATED: {jobInfoLink3.Title} - {jobInfoLink3.Href}");

            // Step 16: Query Pay Run Job Status
            Console.WriteLine("Step 16: Query Pay Run Job Status");
            this.PollPayRunJobStatus(jobInfoLink3);

            // Step 17: Get the Employee Payslip
            Console.WriteLine("Step 17: Get the Employee Payslip");
            var employerKey = employerLink.Href.Split('/').Last();
            var payscheuleKey = payScheduleLink.Href.Split('/').Last();
            var payslipReport =
                this.ApiHelper.GetRawXml($"/Report/PAYSLIP3/run?EmployerKey={employerKey}&PayScheduleKey={payscheuleKey}&TaxYear={this.TaxYear}&PaymentDate={payRunJob2.PaymentDate:yyyy-MM-dd}");
            Console.WriteLine(payslipReport.InnerXml);

            // Step 18: Review Calculation Commentary
            Console.WriteLine("Step 18: Review Calculation Commentary");
            var commentaryLinks = this.ApiHelper.GetLinks(employeeLink.Href + "/Commentaries");
            var commentary = this.ApiHelper.Get<Commentary>(commentaryLinks.Links.Last().Href);
            Console.WriteLine(commentary.Detail);

            // End of example
            Console.WriteLine(string.Empty);
            Console.WriteLine("-- THE END --");
        }

        /// <summary>
        /// Polls the pay run job status.
        /// </summary>
        /// <param name="jobInfoLink">The job information link.</param>
        /// <exception cref="Exception">Payrun job failed:</exception>
        private void PollPayRunJobStatus(Link jobInfoLink)
        {
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
                    throw new Exception("Payrun job failed:" + string.Join(Environment.NewLine, payRunJobInfo.Errors));
                }
            }
        }
    }
}
