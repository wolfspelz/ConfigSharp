namespace ConfigSharpSample
{
    class SampleConfigInclude : MyConfigObject
    {
        public void Load()
        {
            TestProperty += " + SampleConfigInclude";
        }
    }
}
