// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimplePayrun.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SimplePayrun type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PayRunIO.GettingStarted.Examples.Examples
{
    using System;
    using System.Linq;
    using System.Threading;

    using PayRunIO.Core.Enums;
    using PayRunIO.GettingStarted.Examples.Examples.Base;
    using PayRunIO.GettingStarted.Examples.Properties;
    using PayRunIO.Models;

    public class SimplePayrun : ExampleBase
    {
        public override string Title => "Simple Payrun";

        public override string DocsUrl => "/docs/how-to/simple-payrun.html";

        public override int Order => 1;

        public override void Execute()
        {
            Console.WriteLine("Executing Example: " + this.Title);
            Console.WriteLine("See: " + Settings.Default.DeveloperPortalBaseUrl + this.DocsUrl);
            Console.WriteLine("===================================");

            // Step 1: Create an Employer
            Console.WriteLine("Step 1: Create an Employer");

            var employer = new Employer
            {
                EffectiveDate = new DateTime(2019, 1, 1),
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
                Name = "My Monthly",
                PayFrequency = PayFrequency.Monthly
            };

            var payScheduleLink = this.ApiHelper.Post(employerLink.Href + "/PaySchedules", paySchedule);
            Console.WriteLine($"  CREATED: {payScheduleLink.Title} - {payScheduleLink.Href}");

            // Step 3: Create an Employee
            Console.WriteLine("Step 3: Create an Employee");
            var employee = new Employee
            {
                EffectiveDate = new DateTime(2019, 4, 1),
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
                StartDate = new DateTime(2019, 4, 6),
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
                PassportNumber = "123457890",
                BankAccount = new BankAccount
                {
                    AccountName = "Mr T Tester",
                    AccountNumber = "12345678",
                    SortCode = "012345"
                },
                EmployeePartner = new EmployeePartner
                {
                    FirstName = "Teresa",
                    MiddleName = "T",
                    Initials = "TTT",
                    LastName = "Tester",
                    NiNumber = "AB000000A"
                }
            };

            var employeeLink = this.ApiHelper.Post(employerLink.Href + "/Employees", employee);
            Console.WriteLine($"  CREATED: {employeeLink.Title} - {employeeLink.Href}");

            // Step 4: Create a Pay Instruction (Salary)
            Console.WriteLine("Step 4: Create a Pay Instruction (Salary)");
            var salaryInstruction = new SalaryPayInstruction
            {
                StartDate = new DateTime(2019, 4, 1),
                AnnualSalary = 25000.00m
            };

            var salaryInstructionLink = this.ApiHelper.Post(employeeLink.Href + "/PayInstructions", salaryInstruction);
            Console.WriteLine($"  CREATED: {salaryInstructionLink.Title} - {salaryInstructionLink.Href}");

            // Step 5: Create a Pay Run Job
            Console.WriteLine("Step 5: Create a Pay Run Job");
            var payRunJob = new PayRunJobInstruction
            {
                PaymentDate = new DateTime(2019, 4, 30),
                StartDate = new DateTime(2019, 4, 1),
                EndDate = new DateTime(2019, 4, 30),
                PaySchedule = payScheduleLink
            };

            var jobInfoLink = this.ApiHelper.Post("/Jobs/Payruns", payRunJob);
            Console.WriteLine($"  CREATED: {jobInfoLink.Title} - {jobInfoLink.Href}");

            // Step 6: Query Pay Run Job Status
            Console.WriteLine("Step 6: Query Pay Run Job Status");
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

            // Step 7: Get the Employee Payslip
            Console.WriteLine("Step 7: Get the Employee Payslip");

            var employerId = employerLink.Href.Split('/').Last();

            var payslipReport =
                this.ApiHelper.GetRawXml($"/Report/PAYSLIP/run?EmployerKey={employerId}&TaxYear=2019&TaxPeriod=1");

            Console.WriteLine(payslipReport.InnerXml);

            // Step 8: Review Calculation Commentary
            Console.WriteLine("Step 8: Review Calculation Commentary");
            var commentaryLinks = this.ApiHelper.GetLinks(employeeLink.Href + "/Commentaries");

            var commentary = this.ApiHelper.Get<Commentary>(commentaryLinks.Links.First().Href);

            Console.WriteLine(commentary.Detail);

            // Step 9: Create RTI FPS submission Job
            Console.WriteLine("Step 9: Create RTI FPS submission Job");

            var rtiFpsJobInstruction = new RtiJobInstruction
            {
                RtiType = "FPS",
                Generate = true,
                Transmit = true,
                TaxYear = 2019,
                Employer = employerLink,
                PaySchedule = payScheduleLink,
                PaymentDate = new DateTime(2019, 4, 30),
                Timestamp = DateTime.Now
            };

            jobInfoLink = this.ApiHelper.Post("/Jobs/Rti", rtiFpsJobInstruction);
            Console.WriteLine($"  CREATED: {jobInfoLink.Title} - {jobInfoLink.Href}");

            // Step 10: Query RTI Job Status
            Console.WriteLine("Step 10: Query RTI FPS Job Status");
            while (true)
            {
                Thread.Sleep(1000);

                var rtiJobInfo = this.ApiHelper.Get<JobInfo>(jobInfoLink.Href);
                Console.WriteLine($"  Job Status: {rtiJobInfo.JobStatus}");

                if (rtiJobInfo.JobStatus == JobStatus.Success)
                {
                    break;
                }

                if (rtiJobInfo.JobStatus == JobStatus.Failed)
                {
                    throw new Exception("RTI job failed:" + string.Join(Environment.NewLine, rtiJobInfo.Errors));
                }
            }

            // Step 11: Review FPS transmission results
            Console.WriteLine("Step 11: Review FPS transmission results");

            var rtiTransactionLinks = this.ApiHelper.GetLinks(employerLink.Href + "/RtiTransactions");

            var rtiFpsTransaction = this.ApiHelper.Get<RtiFpsTransaction>(rtiTransactionLinks.Links.First().Href);

            Console.WriteLine(rtiFpsTransaction.Response.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\""));

            // End of examples
            Console.WriteLine(string.Empty);
            Console.WriteLine("-- THE END --");
        }
    }
}
