using System;
using System.Globalization;

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
        public string UriBuilderResult { get; set; }
    }

    public class Config : ConfigSharp.Global
    {
        public static MyConfig Global { get { return (MyConfig)ConfigSharp.Global.Instance; } }
    }

    class Program
    {
        static void AssertAndPrint(string label, string expected, string actual)
        {
            Console.WriteLine(label + ": " + actual + " " + (expected == actual ? "ok" : "<-- expecetd: " + expected ));
        }

        static void Main()
        {
            ConfigSharp.Global.Logger((logLevel, logMessage) => Console.WriteLine("ConfigSharp " + logLevel + " " + logMessage));
            ConfigSharp.Global.Instance = new MyConfig().Include("../../Configuration/Root.cs");

            Console.WriteLine("");
            AssertAndPrint("SetupName", "Production", Config.Global.SetupName);
            AssertAndPrint("StringMemberFromRootCs", "Local value from Root.cs", Config.Global.StringMemberFromRootCs.ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("IntPropertyFromRootCs", "42", Config.Global.IntPropertyFromRootCs.ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("IntFromDebugCs", "-1", Config.Global.IntFromDebugCs.ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("IntFromProductionCs", "44", Config.Global.IntFromProductionCs.ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("DateTimeProperty", DateTime.Now.ToString(CultureInfo.InvariantCulture), Config.Global.DateTimeProperty.ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("UriBuilderResult", "http://blog.wolfspelz.de/", Config.Global.UriBuilderResult);
            AssertAndPrint("PropertyFromHttpInclude", "Remote value from /wolfspelz/ConfigSharp/master/ConfigSharpTester/Configuration/Remote.cs", Config.Global.PropertyFromHttpInclude);
            AssertAndPrint("Get(StringMemberFromRootCs)", "Local value from Root.cs", Config.Global.Get("StringMemberFromRootCs", "-default-"));
            AssertAndPrint("Get(IntPropertyFromRootCs)", 42.ToString(CultureInfo.InvariantCulture), Config.Global.Get("IntPropertyFromRootCs", 41).ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("Get(DateTimeProperty.Date.Year)", DateTime.Now.Year.ToString(CultureInfo.InvariantCulture), Config.Global.Get("DateTimeProperty.Date.Year", -1).ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("Get<DateTime>(DateTimeProperty).Date.Year", DateTime.Now.Year.ToString(CultureInfo.InvariantCulture), Config.Global.Get<DateTime>("DateTimeProperty").Date.Year.ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("Get(NotExistingProperty)", "-default-", Config.Global.Get("NotExistingProperty", "-default-"));
            AssertAndPrint("Get(IntPropertyFromRootCs)", "42", Config.Global.Get("IntPropertyFromRootCs", 41).ToString(CultureInfo.InvariantCulture));

            Console.WriteLine("");
            Console.WriteLine("<ENTER> to continue");
            Console.ReadLine();
        }
    }
}
