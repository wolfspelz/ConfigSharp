using System;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new SampleConfig();
            config.Include("../../../ConfigFile.cs");

            Console.WriteLine(config.Greeting);
            // or:
            Console.WriteLine(config.Get("Greeting", ""));

            Console.WriteLine("Press <ENTER> to finish");
            Console.ReadLine();

        }
    }
}
