//reference "C:\Windows\Microsoft.Net\assembly\GAC_MSIL\System\v4.0_4.0.0.0__b77a5c561934e089\System.dll"
//reference "System.Uri, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"

using System;

namespace ConfigSharpTester.Configuration
{
    class Root
    {
        public static void Run(ConfigSharpTester.MyConfig config)
        {
            //config.Include("https://raw.githubusercontent.com/wolfspelz/ConfigSharp/master/ConfigSharpTester/Configuration/Remote.cs");
            var ub = new UriBuilder("http", "blog.wolfspelz.de");
            config.UriBuilderResult = ub.ToString();

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
