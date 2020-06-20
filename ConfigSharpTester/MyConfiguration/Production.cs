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
