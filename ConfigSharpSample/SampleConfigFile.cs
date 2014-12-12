namespace ConfigSharpSample
{
    class SampleConfigFile
    {
        public static void Run(ConfigSharpSample.MyConfigObject config)
        {
            config.TestProperty = "Initialized by SampleConfigFile. ";
            config.Include("SampleConfigInclude.cs");
        }
    }
}
