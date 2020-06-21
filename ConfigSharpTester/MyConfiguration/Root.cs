using System;

namespace ConfigSharpTester.Configuration
{
    class Root : ConfigSharpTester.MyConfig
    {
        public void Load()
        {
            Include("https://raw.githubusercontent.com/wolfspelz/ConfigSharp/develop/ConfigSharpTester/MyConfiguration/Remote.cs");
            var ub = new UriBuilder("http", "blog.wolfspelz.de");
            UriBuilderResult = ub.ToString();

            Include("Setup.cs");
            IntPropertyFromRootCs = 42;
            StringMemberFromRootCs = "Local value from Root.cs";
            DateTimeProperty = DateTime.Now;

            if (SetupName == "Debug") {
                Include("Debug.cs");
            } else if (SetupName == "Production") {
                Include("Production.cs");
            }

            Include("Finally.cs");
        }
    }
}
