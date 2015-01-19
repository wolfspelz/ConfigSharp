namespace ConfigSharpSample
{
    class SampleConfig : MyConfigObject
    {
        public void Load()
        {
            TestProperty += " + SampleConfig";
            Include("SampleConfigInclude.cs");
        }
    }
}
