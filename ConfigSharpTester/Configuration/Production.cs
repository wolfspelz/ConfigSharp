namespace ConfigSharpTester.Configuration
{
    class Production
    {
        public static void Run(ConfigSharpTester.MyConfig config)
        {
            config.IntFromProductionCs = 44;
        }
    }
}
