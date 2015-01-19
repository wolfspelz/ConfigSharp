namespace ConfigSharpTester.Configuration
{
    class Setup : ConfigSharpTester.MyConfig
    {
        public void Load()
        {
            SetupName = "Production";
        }
    }
}
