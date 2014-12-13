using System;

namespace ConfigSharpTester
{
    public class MyConfig : ConfigSharp.Container
    {
        public string SetupName = "Production";
        public string StringMemberFromRootCs = "-empty-";
        public int IntPropertyFromRootCs { get; set; }
        public int IntFromDebugCs = -1;
        public int IntFromProductionCs = -1;
        public DateTime DateTimeProperty { get; set; }
        public string PropertyFromHttpInclude { get; set; }
    }

    public class Config : ConfigSharp.Global
    {
        public static MyConfig Global { get { return (MyConfig)ConfigSharp.Global.Instance; } }
    }

    class Program
    {
        static void Main()
        {
            ConfigSharp.Global.Logger((logLevel, logMessage) => Console.WriteLine("ConfigSharp " + logLevel + " " + logMessage));
            ConfigSharp.Global.Instance = new MyConfig().Include("../../Configuration/Root.cs");

            Console.WriteLine("");
            Console.WriteLine("SetupName                  [Production] = " + Config.Global.SetupName);
            Console.WriteLine("StringMemberFromRootCs       [Local...] = " + Config.Global.StringMemberFromRootCs);
            Console.WriteLine("IntPropertyFromRootCs              [42] = " + Config.Global.IntPropertyFromRootCs);
            Console.WriteLine("IntFromDebugCs                     [-1] = " + Config.Global.IntFromDebugCs);
            Console.WriteLine("IntFromProductionCs                [44] = " + Config.Global.IntFromProductionCs);
            Console.WriteLine("DateTimeProperty                        = " + Config.Global.DateTimeProperty);
            Console.WriteLine("PropertyFromHttpInclude     [Remote...] = " + Config.Global.PropertyFromHttpInclude);
            Console.WriteLine("Get(StringMemberFromRootCs)  [Local...] = " + Config.Global.Get("StringMemberFromRootCs", "-default-"));
            Console.WriteLine("Get(IntPropertyFromRootCs)         [42] = " + Config.Global.Get("IntPropertyFromRootCs", 41));
            Console.WriteLine("Get(DateTimeProperty.Date.Year]         = " + Config.Global.Get("DateTimeProperty.Date.Year", -1));
            Console.WriteLine("Get<T>(DateTimeProperty).Date.Year      = " + Config.Global.Get<DateTime>("DateTimeProperty").Date.Year);
            Console.WriteLine("Get(NotExistingProperty)    [-default-] = " + Config.Global.Get("NotExistingProperty", "-default-"));
            Console.WriteLine("Get(IntPropertyFromRootCs)         [42] = " + Config.Global.Get("IntPropertyFromRootCs", 41));

            Console.WriteLine("");
            Console.WriteLine("<ENTER> to continue");
            Console.ReadLine();
        }
    }
}
