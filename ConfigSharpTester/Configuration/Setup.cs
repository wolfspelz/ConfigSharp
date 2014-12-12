namespace ConfigSharpTester.Configuration
{
    class Setup
    {
        public static void Run(ConfigSharpTester.MyConfig config)
        {
            config.SetupName = "Production";
        }
    }
}
