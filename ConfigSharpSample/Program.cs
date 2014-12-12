using System;

namespace ConfigSharpSample
{
    public class MyConfigObject : ConfigSharp.Container
    {
        public string TestProperty = "Initial value";
    }

    public class Config : ConfigSharp.Global
    {
        public static MyConfigObject Global { get { return (MyConfigObject)ConfigSharp.Global.Instance; } }
    }

    class Program
    {
        static void Main(string[] args)
        {
            { 
                // This is all you need
                ConfigSharp.Global.Instance = new MyConfigObject();
                ConfigSharp.Global.Instance.Include("../../SampleConfigFile.cs"); // Load config
                Console.WriteLine("Config.Global.TestProperty = " + Config.Global.TestProperty); // Use config
                // done

                
                // The same via getter with default
                Console.WriteLine(@"Config.Global.Get(""TestProperty"") = " + Config.Global.Get("TestProperty", "default"));
            }

            {
                // Using just a local variable as config object without the Config.Global option
                var config = new MyConfigObject();
                config.Include("../../SampleConfigFile.cs");
                Console.WriteLine("config.TestProperty = " + config.TestProperty);
            }

            Console.WriteLine("");
            Console.WriteLine("Press <ENTER> to finish");
            Console.ReadLine();
        }
    }
}
