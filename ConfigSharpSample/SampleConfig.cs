namespace ConfigSharpSample
{
    class SampleConfig : MyConfig
    {
        public void Load()
        {
            TestProperty += " + SampleConfig";
            Include("SampleConfigInclude.cs");
        }
    }
}
