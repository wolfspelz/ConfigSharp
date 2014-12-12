using System;

namespace ConfigSharpSample
{
    public class MyConfigObject : ConfigSharp.Container
    {
        public string TestProperty = "Initial value. ";
    }

    public class Config : ConfigSharp.Global
    {
        public static MyConfigObject Global { get { return (MyConfigObject)ConfigSharp.Global.Instance; } }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ConfigSharp.Global.Instance = new MyConfigObject();
            ConfigSharp.Global.Instance.Include("../../SampleConfigFile.cs");

            Console.WriteLine("TestProperty = " + Config.Global.TestProperty);
            Console.WriteLine("<ENTER> to continue");
            Console.ReadLine();
        }
    }
}
