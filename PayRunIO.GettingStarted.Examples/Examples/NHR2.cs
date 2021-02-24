namespace PayRunIO.GettingStarted.Examples.Examples
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using PayRunIO.Core.Enums;
    using PayRunIO.CSharp.SDK;
    using PayRunIO.GettingStarted.Examples.Examples.Base;
    using PayRunIO.Models;

    public class NHR2 : ExampleBase
    {
        public override string Title => "NHR End PayInstructions";

        public override string DocsUrl { get; }

        public override int Order => 8;

        public override void Execute()
        {
            var endDate = new DateTime(2020, 11, 30);
            
            // Step 1: Get the employer
            Console.WriteLine("Step 1: Get Employer 2632");
            var employerLink = "/Employer/2632";

            // Step 2: Get all employees
            Console.WriteLine("Step 2: Get all employees");
            var employeeLinks = this.ApiHelper.Get<LinkCollection>(employerLink + "/Employees");
            Console.WriteLine($"{employeeLinks.Links.Count} employees found.");

            // Step 3: Get Employee PayInstructions
            Console.WriteLine("Step 3: Get Employee PayInstructions");
            foreach (var employeeLink in employeeLinks.Links)
            {
                Console.WriteLine($"  EMPLOYEE: {employeeLink.Href}");

                var payInstructions = this.ApiHelper.GetLinks(employeeLink.Href + "/PayInstructions");
                foreach (var payInstructionLink in payInstructions.Links)
                {
                    Console.Write($"   - {payInstructionLink.Href}");
                    var targetType = Type.GetType("PayRunIO.Models." + payInstructionLink.TargetType + ", PayRunIO.Models");
                    var payInstruction = this.GetGenericPayInstruction(targetType, payInstructionLink.Href);

                    if (!payInstruction.EndDate.HasValue && payInstruction.StartDate < endDate)
                    {
                        payInstruction.EndDate = endDate;

                        switch (payInstructionLink.TargetType)
                        {
                            case "TaxPayInstruction":
                                this.ApiHelper.Put(payInstructionLink.Href, (TaxPayInstruction)payInstruction);
                                break;
                            case "NiPayInstruction":
                                this.ApiHelper.Put(payInstructionLink.Href, (NiPayInstruction)payInstruction);
                                break;
                            case "SalaryPayInstruction":
                                this.ApiHelper.Put(payInstructionLink.Href, (SalaryPayInstruction)payInstruction);
                                break;
                            default:
                                throw new NotSupportedException();
                        }

                        Console.Write($" - ENDED: {endDate:yyyy-mm-dd}");
                    }
                    else if (payInstruction.StartDate >= endDate)
                    {
                        this.ApiHelper.Delete(payInstructionLink.Href);

                        Console.Write(" - DELETED");
                    }

                    Console.Write(Environment.NewLine);
                }
            }

            Console.WriteLine("COMPLETED employee updates");
        }

        private PayInstruction GetGenericPayInstruction(Type targetType, string href)
        {
            MethodInfo method = typeof(RestApiHelper).GetMethod("Get");
            MethodInfo generic = method.MakeGenericMethod(targetType);
            var result = generic.Invoke(this.ApiHelper, new object[] { href });

            return (PayInstruction)result;
        }
    }
}
