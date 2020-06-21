using System;
using System.Collections.Generic;
using System.Globalization;

using ConfigSharpTester.Configuration;

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
        public string FromCustomFunction { get; set; } = "unchanged";
        public string FromDontLoadThisCustomFunction { get; set; } = "unchanged";
    }

    class Program
    {
        static void AssertAndPrint(string label, string expected, string actual)
        {
            Console.WriteLine(label + ": " + actual + " " + (expected == actual ? "ok" : "<-- expecetd: " + expected ));
        }

        static void Main(string[] args)
        {
            ConfigSharp.Log.LogHandler = (level, context, message) => Console.WriteLine($"ConfigSharp {level} {context} {message}");
            var Config = new MyConfig();
            Config.Functions.Add(ConfigSharp.Container.AnyPublicMember);
            Config.Functions.Add(ConfigSharp.Container.Not + nameof(Finally.DontLoadThisCustomFunction));

            Config.Include("../../../MyConfiguration/Root.cs");

            Console.WriteLine("");
            AssertAndPrint("SetupName", "Production", Config.SetupName);
            AssertAndPrint("StringMemberFromRootCs", "Local value from Root.cs", Config.StringMemberFromRootCs.ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("IntPropertyFromRootCs", "42", Config.IntPropertyFromRootCs.ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("IntFromDebugCs", "-1", Config.IntFromDebugCs.ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("IntFromProductionCs", "44", Config.IntFromProductionCs.ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("DateTimeProperty", DateTime.Now.ToString(CultureInfo.InvariantCulture), Config.DateTimeProperty.ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("UriBuilderResult", "http://blog.wolfspelz.de/", Config.UriBuilderResult);
            AssertAndPrint("PropertyFromHttpInclude", "Remote value from /wolfspelz/ConfigSharp/develop/ConfigSharpTester/MyConfiguration/Remote.cs", Config.PropertyFromHttpInclude);
            AssertAndPrint("Get(StringMemberFromRootCs)", "Local value from Root.cs", Config.Get("StringMemberFromRootCs", "-default-"));
            AssertAndPrint("Get(IntPropertyFromRootCs)", 42.ToString(CultureInfo.InvariantCulture), Config.Get("IntPropertyFromRootCs", 41).ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("Get(DateTimeProperty.Date.Year)", DateTime.Now.Year.ToString(CultureInfo.InvariantCulture), Config.Get("DateTimeProperty.Date.Year", -1).ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("Get<DateTime>(DateTimeProperty).Date.Year", DateTime.Now.Year.ToString(CultureInfo.InvariantCulture), Config.Get<DateTime>("DateTimeProperty").Date.Year.ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("Get(NotExistingProperty)", "-default-", Config.Get("NotExistingProperty", "-default-"));
            AssertAndPrint("Get(IntPropertyFromRootCs)", "42", Config.Get("IntPropertyFromRootCs", 41).ToString(CultureInfo.InvariantCulture));
            AssertAndPrint("Get(FromCustomFunction)", "FromCustomFunction", Config.Get(nameof(MyConfig.FromCustomFunction), ""));
            AssertAndPrint("Get(FromDontLoadThisCustomFunction)", "unchanged", Config.Get(nameof(MyConfig.FromDontLoadThisCustomFunction), ""));

            Console.WriteLine("");
            Console.WriteLine("<ENTER> to continue");
            Console.ReadLine();
        }
    }
}
