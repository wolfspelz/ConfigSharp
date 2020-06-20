//reference "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.5\netstandard.dll"

namespace ConfigSharpTester.Configuration
{
    class Debug : ConfigSharpTester.MyConfig
    {
        public void Load()
        {
            IntFromDebugCs = 44;
        }
    }
}
