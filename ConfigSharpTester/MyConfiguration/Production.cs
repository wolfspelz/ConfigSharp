//reference "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.5\netstandard.dll"

namespace ConfigSharpTester.Configuration
{
    class Production : ConfigSharpTester.MyConfig
    {
        public void Load()
        {
            IntFromProductionCs = 44;
        }
    }
}
