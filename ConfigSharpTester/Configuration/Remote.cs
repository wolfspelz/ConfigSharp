#pragma reference "C:\Windows\Microsoft.Net\assembly\GAC_MSIL\System\v4.0_4.0.0.0__b77a5c561934e089\System.dll"

using System;

namespace ConfigSharpTester.Configuration
{
    class Remote
    {
        public static void Run(ConfigSharpTester.MyConfig config)
        {
            config.PropertyFromHttpInclude = "Remote value from " + new Uri(config.CurrentFile).PathAndQuery;
        }
    }
}
