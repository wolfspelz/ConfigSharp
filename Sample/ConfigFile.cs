namespace Sample
{
    class ConfigFile : SampleConfig
    {
        public void Load()
        {
            Greeting += "Hello";
            Include("ConfigInclude.cs");
        }
    }
}
