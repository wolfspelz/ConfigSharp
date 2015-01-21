using System;

namespace ConfigSharpSample
{
    public class MyConfigObject : ConfigSharp.Container
    {
        public string TestProperty = "Initial";
    }

    // This helper makes the Config.Global.MyProperty syntax available
    // instead of ConfigSharp.Global.Instance.MyProperty 
    public class Config : ConfigSharp.Global
    {
        public static MyConfigObject Global { get { return (MyConfigObject)ConfigSharp.Global.Instance; } }
        public static string Get(string key, string defaultValue) { return ConfigSharp.Global.Instance.Get(key, defaultValue); }
    }

    class Program
    {
        static void Main(string[] args)
        {
            { 
                // This is all you need
                ConfigSharp.Global.Instance = new MyConfigObject();
                ConfigSharp.Global.Instance.Include("../../SampleConfig.cs"); // Load config
                Console.WriteLine("Config.Global.TestProperty= " + Config.Global.TestProperty); // Use config
                // done

                
                // The same via getter with default
                Console.WriteLine(@"Config.Global.Get(TestProperty)= " + Config.Get("TestProperty", "default"));
            }

            {
                // Using just a local variable as config object without the Config.Global option
                var config = new MyConfigObject();
                config.Include("../../SampleConfig.cs");
                Console.WriteLine("config.TestProperty = " + config.TestProperty);
            }

            Console.WriteLine("");
            Console.WriteLine("Press <ENTER> to finish");
            Console.ReadLine();
        }
    }
}
