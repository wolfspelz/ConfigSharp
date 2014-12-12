namespace ConfigSharpSample
{
    class SampleConfigInclude
    {
        public static void Run(ConfigSharpSample.MyConfigObject config)
        {
            config.TestProperty += "Added by SampleConfigInclude. ";
        }
    }
}
