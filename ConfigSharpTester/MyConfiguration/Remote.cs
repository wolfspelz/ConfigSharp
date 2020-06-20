using System;

namespace ConfigSharpTester.Configuration
{
    class Remote : ConfigSharpTester.MyConfig
    {
        public void Load()
        {
            PropertyFromHttpInclude = "Remote value from " + new Uri(CurrentFile).PathAndQuery;
        }
    }
}
