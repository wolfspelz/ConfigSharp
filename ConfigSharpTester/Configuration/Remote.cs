//reference "System.Uri, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"

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
