namespace ConfigSharpSample
{
    class SampleConfigFile : ConfigSharpSample.MyConfigObject
    {
        public void Load()
        {
            TestProperty += " + SampleConfigFile";
            Include("SampleConfigInclude.cs");
        }
    }
}
