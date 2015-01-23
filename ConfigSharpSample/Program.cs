using System;

namespace ConfigSharpSample
{
    public class MyConfig : ConfigSharp.Container
    {
        public string TestProperty = "Initial";
    }

    // This helper makes the Global.Config.MyProperty syntax available instead of ConfigSharp.Global.Instance.MyProperty 
    public class Global : ConfigSharp.Global
    {
        public static MyConfig Config { get { return (MyConfig)ConfigSharp.Global.Instance; } }
        public static string Get(string key, string defaultValue) { return ConfigSharp.Global.Instance.Get(key, defaultValue); }
    }

    class Program
    {
        static void Main(string[] args)
        {
            { 
                // This is all you need
                ConfigSharp.Global.Instance = new MyConfig().Include("../../SampleConfig.cs"); // Load config
                Console.WriteLine("Global.Config.TestProperty = " + Global.Config.TestProperty); // Use config

                // The same via getter with default
                var testProperty = Global.Get("TestProperty", "default");
            }

            Console.WriteLine("");
            Console.WriteLine("Press <ENTER> to finish");
            Console.ReadLine();
        }
    }
}
