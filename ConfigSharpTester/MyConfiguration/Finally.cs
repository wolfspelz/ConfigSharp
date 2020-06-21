namespace ConfigSharpTester.Configuration
{
    class Finally : ConfigSharpTester.MyConfig
    {
        public void CustomFunction()
        {
            FromCustomFunction = "FromCustomFunction";
        }

        public void DontLoadThisCustomFunction()
        {
            FromDontLoadThisCustomFunction = "FromDontLoadThisCustomFunction";
        }
    }
}
