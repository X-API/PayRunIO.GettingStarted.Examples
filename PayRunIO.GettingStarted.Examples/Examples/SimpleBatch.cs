// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleBatch.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SimpleBatch type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PayRunIO.GettingStarted.Examples.Examples
{
    using System;
    using System.Threading;

    using PayRunIO.Core.Enums;
    using PayRunIO.CSharp.SDK;
    using PayRunIO.GettingStarted.Examples.Examples.Base;
    using PayRunIO.GettingStarted.Examples.Properties;
    using PayRunIO.Models;

    public class SimpleBatch : ExampleBase
    {
        public override string Title => "Simple Batch";

        public override string DocsUrl => "/docs/how-to/batch-processing.html";

        public override int Order => 5;

        public override short TaxYear => 2022;

        public override void Execute()
        {
            Console.WriteLine("Executing Example: " + this.Title);
            Console.WriteLine("See: " + Settings.Default.DeveloperPortalBaseUrl + this.DocsUrl);
            Console.WriteLine("===================================");

            var employerKey = 
                Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .Replace("=", string.Empty)
                    .Replace("+", string.Empty)
                    .Replace(@"/", string.Empty);

            // Step 1: Create an Employer
            Console.WriteLine("Step 1: Create an Employer");

            var employer = new Employer
            {
                EffectiveDate = new DateTime(this.TaxYear, 4, 6),
                Name = "Batch Employer",
                RuleExclusions = RuleExclusionFlags.None,
                Territory = CalculatorTerritory.UnitedKingdom,
                Region = CalculatorRegion.England,
            };

            // Step 2: Create a Pay Schedule
            Console.WriteLine("Step 2: Create a Pay Schedule to POST");
            var payScheduleA = new PaySchedule
            {
                Name = "Test 1",
                PayFrequency = PayFrequency.Monthly
            };

            // Step 3: Create a Pay Schedule
            Console.WriteLine("Step 2: Create a Pay Schedule to PUT");
            var payScheduleB = new PaySchedule
            {
                Name = "Test 2",
                PayFrequency = PayFrequency.Monthly
            };

            // Step 4: Create an Employee
            Console.WriteLine("Step 4: Create an Employee");
            var employee = new Employee
            {
                EffectiveDate = new DateTime(this.TaxYear, 4, 6),
                Code = "EMP001",
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = new DateTime(1980, 1, 1),
                NicLiability = NicLiability.IsFullyLiable,
                Region = CalculatorRegion.England,
                Territory = CalculatorTerritory.UnitedKingdom,
                PaySchedule = new Link { Href = $"/Employer/{employerKey}/PaySchedule/TEST001" },
                AEAssessmentOverride = AEAssessmentOverride.None
            };

            // Step 5: Create a batch Job
            Console.WriteLine("Step 5: Create a Batch Job");
            var batchJob = new BatchJobInstruction { ValidateOnly = false };

            batchJob.Instructions.Add(new BatchPutItem { Body = employer, Href = $"/Employer/{employerKey}" });
            batchJob.Instructions.Add(new BatchPostItem { Body = payScheduleA, Href = $"/Employer/{employerKey}/PaySchedules" });
            batchJob.Instructions.Add(new BatchPutItem { Body = payScheduleB, Href = $"/Employer/{employerKey}/PaySchedule/TEST001" });
            batchJob.Instructions.Add(new BatchPutItem { Body = employee, Href = $"/Employer/{employerKey}/Employees" });
            batchJob.Instructions.Add(new BatchPatchItem { Href = $"/Employer/{employerKey}/Employee/EE001", Body = $"<Employee><EffectiveDate>{this.TaxYear}-04-06</EffectiveDate><Deactivated>true</Deactivated></Employee>" });

            var doc = XmlSerialiserHelper.SerialiseToXmlDoc(batchJob);

            Console.WriteLine(doc.OuterXml);

            var jobInfoLink = this.ApiHelper.Post("/Jobs/Batch", batchJob);
            Console.WriteLine($"  CREATED: {jobInfoLink.Title} - {jobInfoLink.Href}");

            // Step 6: Query Batch Job Status
            Console.WriteLine("Step 6: Query Batch Job Status");
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
                    throw new Exception("Batch job failed:" + string.Join(Environment.NewLine, payRunJobInfo.Errors));
                }
            }

            // End of examples
            Console.WriteLine(string.Empty);
            Console.WriteLine("-- THE END --");
        }
    }
}
