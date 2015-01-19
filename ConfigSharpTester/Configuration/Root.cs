//reference "C:\Windows\Microsoft.Net\assembly\GAC_MSIL\System\v4.0_4.0.0.0__b77a5c561934e089\System.dll"
//reference "System.Uri, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"

using System;

namespace ConfigSharpTester.Configuration
{
    class Root : ConfigSharpTester.MyConfig
    {
        public void Load()
        {
            Include("https://raw.githubusercontent.com/wolfspelz/ConfigSharp/master/ConfigSharpTester/Configuration/Remote.cs");
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

        }
    }
}
