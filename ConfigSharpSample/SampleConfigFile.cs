namespace ConfigSharpSample
{
    class SampleConfigFile
    {
        public static void Run(MyConfigObject config)
        {
            config.TestProperty = "By SampleConfigFile";
            config.Include("SampleConfigInclude.cs");
        }
    }
}
