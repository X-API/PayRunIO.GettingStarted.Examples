namespace PayRunIO.GettingStarted.Examples.Examples
{
    using PayRunIO.Core.Enums;
    using PayRunIO.GettingStarted.Examples.Examples.Base;
    using PayRunIO.GettingStarted.Examples.Properties;
    using PayRunIO.Models;
    using System;
    using System.Linq;
    using System.Threading;

    using Bogus;

    using PayRunIO.CSharp.SDK;

    public class EmployeeUpdateExample : ExampleBase
    {
        public override string Title => "Add bank account where missing for each employee.";

        public override string DocsUrl => "";

        public override int Order => 9;

        public override short TaxYear => 2025;

        public override void Execute()
        {
            Console.WriteLine("Executing Example: " + this.Title);
            Console.WriteLine("See: " + Settings.Default.DeveloperPortalBaseUrl + this.DocsUrl);
            Console.WriteLine("===================================");

            var employees = this.ApiHelper.GetLinks("/Employer/2004/PaySchedule/MONTHLY/PayRun/PR542/Employees");

            var faker = new Faker("en");
            foreach (var link in employees.Links)
            {
                var ee = this.ApiHelper.Get<Employee>(link.Href);
                var key = link.ExtractEmployeeKey();

                if (ee.BankAccount == null)
                {
                    ee.BankAccount = new BankAccount
                    {
                        AccountName = $"{ee.FirstName} {ee.LastName}",
                        AccountNumber = faker.Finance.Account(8),
                        SortCode = $"{faker.Random.Number(10, 99)}-{faker.Random.Number(10, 99)}-{faker.Random.Number(10, 99)}"
                    };

                    ee.EffectiveDate = new DateTime(2024, 7, 1);
                    this.ApiHelper.Put($"/Employer/2004/Employee/{key}", ee);
                    Console.WriteLine($"[{ee.Code}] {ee.FirstName} {ee.LastName}");
                }
            }

            // End of example
            Console.WriteLine(string.Empty);
            Console.WriteLine("-- THE END --");
        }
    }
}
