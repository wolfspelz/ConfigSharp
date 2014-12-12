ConfigSharp
===========

Configure your .NET application with C# config files. Config files are C# source files, managed by the Visual Studio like any other code file, intelli-sensed, syntax checked, compiled, type safe. 

Write real code with control structures and classes in config files. Include other config files and remote (HTTP) config snippets.

No more key-value lists of string based app settings. These settings are typed properties of CLR objects.

Program.cs:

    public class MyConfig : ConfigSharp.Container
    {
        public string SomeProperty { get; set; }
        public int OrAsMemberVariable = 41;
        public DateTime RealCLRTypes;
    }
    
    static void Main(string[] args)
    {
      var config = new MyConfig();
      config.Include("ConfigFile.cs");
      Console.WriteLine("config.SomeProperty = " + config.SomeProperty);
      ...
    }

ConfigFile.cs:

    namespace MyProgram.Configuration
    {
        class ConfigFile // Any class name, but preferably same as file name
        {
            public static void Run(MyProgram.MyConfig config) // Any method name
            {
                config.SomeProperty = "42";
                config.Include("OtherConfigFile.cs");
            }
        }
    }

Integrated support for a global config object:

    ConfigSharp.Global.Instance = new MyConfig().Include("ConfigFile.cs");
    Console.WriteLine("Config.Global.SomeProperty = " + Config.Global.SomeProperty);

