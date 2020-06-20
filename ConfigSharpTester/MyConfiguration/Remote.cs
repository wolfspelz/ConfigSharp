//reference "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.5\netstandard.dll"

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
