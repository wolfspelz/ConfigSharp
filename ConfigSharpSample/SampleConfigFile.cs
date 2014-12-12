namespace ConfigSharpSample
{
    class SampleConfigFile
    {
        public static void Run(ConfigSharpSample.MyConfigObject config)
        {
            config.TestProperty = "By SampleConfigFile";
            config.Include("SampleConfigInclude.cs");
        }
    }
}
