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
