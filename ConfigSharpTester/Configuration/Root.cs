using System;

namespace ConfigSharpTester.Configuration
{
    class Root
    {
        public static void Run(ConfigSharpTester.MyConfig config)
        {
            config.Include("http://www.wolfspelz.de/tmp/Remote.cs");
            config.Include("Remote.cs");

            config.Include("Setup.cs");
            config.IntPropertyFromRootCs = 42;
            config.StringMemberFromRootCs = "Local value from Root.cs";
            config.DateTimeProperty = DateTime.Now;
            
            if (config.SetupName == "Debug") { 
                config.Include("Debug.cs");
            } else if (config.SetupName == "Production") {
                config.Include("Production.cs");
            }

        }
    }
}
