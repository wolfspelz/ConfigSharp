namespace ConfigSharpSample
{
    class SampleConfigInclude : MyConfig
    {
        public void Load()
        {
            TestProperty += " + SampleConfigInclude";
        }
    }
}
