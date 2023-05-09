using System;
using System.Collections.Generic;
using System.Linq;
using PayRunIO.GettingStarted.Examples.Examples.Base;
using PayRunIO.GettingStarted.Examples.Properties;
using PayRunIO.Models;

namespace PayRunIO.GettingStarted.Examples.Examples
{
    public class MigrateContactGroup : ExampleBase
    {
        public override string Title => "Migrate Contact Group";

        public override string DocsUrl => "";

        public override int Order => 8;

        public override short TaxYear => 2023;

        public override void Execute()
        {
            Console.WriteLine("Executing Example: " + this.Title);
            Console.WriteLine("See: " + Settings.Default.DeveloperPortalBaseUrl + this.DocsUrl);
            Console.WriteLine("===================================");

            var employerGroupings = new Dictionary<string, string>()
            {
                //{ "2004","1" },
            };

            foreach (var employerGroup in employerGroupings)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.White;

                    var employerKey = employerGroup.Key;
                    var targetGroupValue = employerGroup.Value;

                    var employer = this.ApiHelper.Get<Employer>($"/Employer/{employerKey}");
                    Console.WriteLine($"Employer/{employerKey}: {employer.Name}");

                    var currentGroup = employer.MetaData.Items.SingleOrDefault(meta => meta.Name == "Group");
                    if (currentGroup == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(" - No current Group found in meta-data.");
                        continue;
                    }

                    var currentGroupValue = currentGroup.Value;
                    if (currentGroupValue == targetGroupValue)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(" - Current group found matches target group, no change required.");
                        continue;
                    }

                    var nextRevDate = employer.EffectiveDate;
                    var paySchedules = this.ApiHelper.GetLinks($"/Employer/{employerKey}/PaySchedules");
                    foreach (var paySchedule in paySchedules.Links)
                    {
                        var payRuns = this.ApiHelper.GetLinks($"{paySchedule.Href}/PayRuns");
                        if (!payRuns.Links.Any())
                        {
                            continue;
                        }

                        var headPayRun = this.ApiHelper.Get<PayRun>($"{payRuns.Links.Last()}");
                        if (nextRevDate < headPayRun.PaymentDate)
                        {
                            nextRevDate = headPayRun.PaymentDate.AddDays(1);
                        }
                    }

                    // Update employer Group value 
                    employer.EffectiveDate = nextRevDate;
                    currentGroup.Value = targetGroupValue;
                    this.ApiHelper.Put($"/Employer/{employerKey}", employer);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Employer updated.");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine(string.Empty);
                }
            }

            // End of examples
            Console.WriteLine(string.Empty);
            Console.WriteLine("-- THE END --");
        }
    }
}