namespace PayRunIO.GettingStarted.Examples.Examples
{
    using System;
    using System.Linq;
    using System.Threading;

    using PayRunIO.Core.Enums;
    using PayRunIO.GettingStarted.Examples.Examples.Base;
    using PayRunIO.Models;

    public class NHR : ExampleBase
    {
        public override string Title => "NHR PaySchedule Region Update";

        public override string DocsUrl { get; }

        public override int Order => 7;

        public override void Execute()
        {
            // Step 1: Get the employer
            Console.WriteLine("Step 1: Get Employer 2632");
            var employerLink = "/Employer/2632";
            var employer = this.ApiHelper.Get<Employer>(employerLink);

            // Step 2: Create a Pay Schedule
            Link payScheduleLink;
            var paySchedules = this.ApiHelper.Get<LinkCollection>(employerLink + "/PaySchedules");
            if (paySchedules.Links.Count > 0)
            {
                payScheduleLink = paySchedules.Links.First();
                Console.WriteLine($"  FOUND: {payScheduleLink.Title} - {payScheduleLink.Href}");
            }
            else
            {
                Console.WriteLine("Step 2: Create a Pay Schedule");
                var paySchedule = new PaySchedule
                {
                    Name = "Monthly",
                    PayFrequency = PayFrequency.Monthly
                };

                payScheduleLink = this.ApiHelper.Post(employerLink + "/PaySchedules", paySchedule);
                Console.WriteLine($"  CREATED: {payScheduleLink.Title} - {payScheduleLink.Href}");
            }

            // Step 3: Get all employees
            Console.WriteLine("Step 3: Get all employees");
            var employeeLinks = this.ApiHelper.Get<LinkCollection>(employerLink + "/Employees");
            Console.WriteLine($"{employeeLinks.Links.Count} employees found.");

            // Step 4: Assign Employees to the PaySchedule
            Console.WriteLine("Step 4: Assign Employees to the PaySchedule");
            foreach (var employeeLink in employeeLinks.Links)
            {
                var employee = this.ApiHelper.Get<Employee>(employeeLink.Href);
                if (employee.PaySchedule == null)
                {
                    employee.PaySchedule = payScheduleLink;
                    this.ApiHelper.Put(employeeLink.Href, employee);

                    Console.WriteLine($"PUT {employeeLink.Href} into PaySchedule {payScheduleLink.Href}");
                }

                if (employee.Region == CalculatorRegion.NotSet)
                {
                    employee.Region = CalculatorRegion.England;
                    this.ApiHelper.Put(employeeLink.Href, employee);

                    Console.WriteLine($"PUT {employeeLink.Href} into Region: England");
                }
            }

            Console.WriteLine("COMPLETED employee updates");

            //// Step 5: Create a Pay Run Job
            //Console.WriteLine("Step 5: Create a Pay Run Job");
            //var payRunJob = new PayRunJobInstruction
            //{
            //    PaymentDate = new DateTime(2020, 11, 20),
            //    StartDate = new DateTime(2020, 11, 1),
            //    EndDate = new DateTime(2020, 11, 30),
            //    PaySchedule = payScheduleLink
            //};

            //var jobInfoLink = this.ApiHelper.Post("/Jobs/Payruns", payRunJob);
            //Console.WriteLine($"  CREATED: {jobInfoLink.Title} - {jobInfoLink.Href}");

            //// Step 6: Query Pay Run Job Status
            //Console.WriteLine("Step 6: Query Pay Run Job Status");
            //while (true)
            //{
            //    Thread.Sleep(1000);

            //    var payRunJobInfo = this.ApiHelper.Get<JobInfo>(jobInfoLink.Href);
            //    Console.WriteLine($"  Job Status: {payRunJobInfo.JobStatus} - {payRunJobInfo.Progress:P2}");

            //    if (payRunJobInfo.JobStatus == JobStatus.Success)
            //    {
            //        break;
            //    }

            //    if (payRunJobInfo.JobStatus == JobStatus.Failed)
            //    {
            //        throw new Exception("Payrun job failed:" + string.Join(Environment.NewLine, payRunJobInfo.Errors));
            //    }
            //}
        }
    }
}
