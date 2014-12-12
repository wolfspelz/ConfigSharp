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
