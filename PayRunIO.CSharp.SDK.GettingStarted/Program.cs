namespace PayRunIO.CSharp.SDK.GettingStarted
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using PayRunIO.CSharp.SDK.GettingStarted.Examples;
    using PayRunIO.CSharp.SDK.GettingStarted.Properties;

    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            var examples = LoadExamples();
            if (args.Length > 0 && args[0].ToLower().EndsWith("run"))
            {
                RunAll(examples);
            }
            else
            {
                DisplayMenu(examples);
            }
        }

        private static void RunAll(Dictionary<int, IExample> examples)
        {
            foreach (var example in examples.OrderBy(e => e.Key))
            {
                example.Value.Execute();
            }
        }

        private static Dictionary<int, IExample> LoadExamples()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var exampleTypes = assembly.GetTypes().Where(c => typeof(IExample).IsAssignableFrom(c) && c.IsClass && !c.IsAbstract);

            var examples = new Dictionary<int, IExample>();

            foreach (var type in exampleTypes)
            {
                var example = (IExample)Activator.CreateInstance(type);
                examples.Add(example.Order, example);
            }

            return examples;
        }

        private static void DisplayMenu(Dictionary<int, IExample> examples)
        {
            BuildMenu(examples);

            ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey(false);
                if (cki.KeyChar == 'm')
                {
                    Console.Clear();
                    BuildMenu(examples);
                }
                else
                {
                    var key = int.Parse(cki.KeyChar.ToString());
                    Console.Clear();
                    examples[key].Execute();
                    Console.WriteLine("Press M to return to menu");
                }
                
            } while (cki.Key != ConsoleKey.Escape);
        }

        private static void BuildMenu(Dictionary<int, IExample> examples)
        {
            Console.WriteLine("PayRun.IO - How-to Examples");
            Console.WriteLine($"See: {Settings.Default.DeveloperPortalBaseUrl}/docs/how-to/examples.html");
            Console.WriteLine("===================================");
            Console.WriteLine(string.Empty);

            Console.WriteLine("Examples Menu");
            foreach (var example in examples.OrderBy(e => e.Key))
            {
                Console.WriteLine($"[{example.Key}] {example.Value.Title}");
            }
        }
    }
}
