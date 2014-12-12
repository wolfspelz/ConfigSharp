using System;

namespace ConfigSharpTester.Configuration
{
    class Root
    {
        public static void Run(ConfigSharpTester.MyConfig config)
        {
            config.Include("https://raw.githubusercontent.com/wolfspelz/ConfigSharp/master/ConfigSharpTester/Configuration/Remote.cs");
            //config.Include("Remote.cs");

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
