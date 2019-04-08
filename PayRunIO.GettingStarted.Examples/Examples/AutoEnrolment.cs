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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;

    using PayRunIO.Core.Enums;
    using PayRunIO.CSharp.SDK;
    using PayRunIO.GettingStarted.Examples.Examples.Base;
    using PayRunIO.GettingStarted.Examples.Properties;
    using PayRunIO.Models;

    public class AutoEnrolmentExample : ExampleBase
    {
        public override string Title => "Auto Enrolment";

        public override string DocsUrl => "/docs/how-to/auto-enrrolment.html";

        public override int Order => 2;

        public override void Execute()
        {
            Console.WriteLine("Executing Example: " + this.Title);
            Console.WriteLine("See: " + Settings.Default.DeveloperPortalBaseUrl + this.DocsUrl);
            Console.WriteLine("===================================");

            // Step 1: Create an Employer
            Console.WriteLine("Step 1: Create an Employer");

            var employer = new Employer
            {
                EffectiveDate = new DateTime(2019, 4, 1),
                Name = "AE Test Ltd",
                Region = CalculatorRegion.England,
                Territory = CalculatorTerritory.UnitedKingdom,
                RuleExclusions = RuleExclusionFlags.None,
                AutoEnrolment = new EmployerAutoEnrolment
                {
                    StagingDate = new DateTime(2014, 4, 1),
                    PrimaryFirstName = "Terry",
                    PrimaryLastName = "Tester",
                    PrimaryEmail = "Terry.Tester@PayRun.io",
                    PrimaryTelephone = "0123456789",
                    PrimaryJobTitle = "HR Manager"
                }
            };

            var employerLink = this.ApiHelper.Post("/Employers", employer);
            Console.WriteLine($"  CREATED: {employerLink.Title} - {employerLink.Href}");

            // Step 2: Create Auto Enrolment Pension Scheme
            Console.WriteLine("Step 2: Create Auto Enrolment Pension Scheme");

            var autoEnrolmentPension = new Pension
            {
                EffectiveDate = new DateTime(2019, 4, 1),
                SchemeName = "AE Scheme",
                ProviderName = "NEST",
                ProviderEmployerRef = "EMP123456789",
                EmployeeContributionPercent = 0.03m,
                EmployerContributionPercent = 0.02m,
                TaxationMethod = PensionTaxationMethod.ReliefAtSource,
                AECompatible = true,
                UseAEThresholds = true,
                PensionablePayCodes = new Collection<string> { "BASIC" },
                QualifyingPayCodes = new Collection<string> { "BASIC" }
            };

            var aePensionLinkHref = employerLink.Href + "/Pension/AEPENSION";
            var aePensionLink = this.ApiHelper.Put(aePensionLinkHref, autoEnrolmentPension);
            Console.WriteLine($"  CREATED: {aePensionLink.SchemeName} - {aePensionLinkHref}");

            // Step 3: Update Employer Auto Enrolment Pension Scheme
            Console.WriteLine("Step 3: Update Employer Auto Enrolment Pension Scheme");

            employer.AutoEnrolment.Pension = new Link { Href = aePensionLinkHref };
            this.ApiHelper.Patch<Employer>(employerLink.Href, XmlSerialiserHelper.Serialise(employer));
            Console.WriteLine($"  UPDATE: Updated employer with AE pension - {aePensionLinkHref}");

            // Step 4: Create a Pay Schedule
            Console.WriteLine("Step 2: Create a Pay Schedule");
            var paySchedule = new PaySchedule
            {
                Name = "My Monthly",
                PayFrequency = PayFrequency.Monthly
            };

            var payScheduleLink = this.ApiHelper.Post(employerLink.Href + "/PaySchedules", paySchedule);
            Console.WriteLine($"  CREATED: {payScheduleLink.Title} - {payScheduleLink.Href}");

            // Step 5: Create an Employee
            Console.WriteLine("Step 3: Create an Employee");
            var employee = new Employee
            {
                EffectiveDate = new DateTime(2019, 4, 1),
                Code = "EMPAE1",
                FirstName = "Jane",
                LastName = "Johnson",
                DateOfBirth = new DateTime(1990, 12, 10),
                Gender = Gender.Female,
                NicLiability = NicLiability.IsFullyLiable,
                Region = CalculatorRegion.England,
                Territory = CalculatorTerritory.UnitedKingdom,
                WorkingWeek = WorkingWeek.AllWeekDays,
                HoursPerWeek = 37.5m,
                RuleExclusions = RuleExclusionFlags.None,
                PaySchedule = payScheduleLink,
                StartDate = new DateTime(2013, 4, 1),
                AEAssessmentOverride = AEAssessmentOverride.None
            };

            var employeeLink = this.ApiHelper.Post(employerLink.Href + "/Employees", employee);
            Console.WriteLine($"  CREATED: {employeeLink.Title} - {employeeLink.Href}");

            // Step 6: Create a Pay Instruction (Salary)
            Console.WriteLine("Step 6: Pay the Employee");
            var rateInstruction = new RatePayInstruction
            {
                StartDate = new DateTime(2019, 4, 1),
                EndDate = new DateTime(2019, 4, 1),
                Rate = 13.56m,
                RateUoM = UomBasicPay.Hour,
                Units = 160
            };

            var rateInstructionLink = this.ApiHelper.Post(employeeLink.Href + "/PayInstructions", rateInstruction);
            Console.WriteLine($"  CREATED: {rateInstructionLink.Title} - {rateInstructionLink.Href}");

            // Step 7: Create a Pay Run Job
            Console.WriteLine("Step 7: Create a Pay Run Job");
            var payRunJob = new PayRunJobInstruction
            {
                PaymentDate = new DateTime(2019, 4, 30),
                StartDate = new DateTime(2019, 4, 1),
                EndDate = new DateTime(2019, 4, 30),
                PaySchedule = payScheduleLink
            };

            var jobInfoLink = this.ApiHelper.Post("/Jobs/Payruns", payRunJob);
            Console.WriteLine($"  CREATED: {jobInfoLink.Title} - {jobInfoLink.Href}");

            // Step 8: Query Pay Run Job Status
            Console.WriteLine("Step 8: Query Pay Run Job Status");
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

            // Step 9: Examine the AE Assessment Result
            Console.WriteLine("Step 9: Examine the AE Assessment Result");

            var aeAsseeement =
                this.ApiHelper.GetRawXml($"{employeeLink}/AEAssessment/AE001");

            Console.WriteLine(aeAsseeement.InnerXml);

            // Step 10: Review Calculation Commentary
            Console.WriteLine("Step 10: Review Calculation Commentary");
            var commentaryLinks = this.ApiHelper.GetLinks(employeeLink.Href + "/Commentaries");

            var commentary = this.ApiHelper.Get<Commentary>(commentaryLinks.Links.First().Href);
            Console.WriteLine(commentary.Detail);

            // End of examples
            Console.WriteLine(string.Empty);
            Console.WriteLine("-- THE END --");
        }
    }
}
