ConfigSharp
===========

Configure your .NET application with C# config files. Config files are C# source files, managed by the Visual Studio like any other code file, intellisensed, syntax checked, compiled, type safe. 

Write real code with control structures and classes in config files. Include other local or remote (HTTP) config files.

No more key-value lists of string based app settings from XML. These settings are typed properties of CLR objects.

### Examples

Program.cs:

    public class MyConfig : ConfigSharp.Container
    {
        public string SomeProperty { get; set; }
        public int OrAsMemberVariable = 41;
        public DateTime OrRealCLRTypes;
        public string DatabasePassword { get; set; }
        public string SetupName { get; set; }
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

### Ubiquitous access to config settings

Integrated support for a global config object:

    ConfigSharp.Global.Instance = new MyConfig().Include("ConfigFile.cs");
    Console.WriteLine("Config.Global.SomeProperty = " + Config.Global.SomeProperty);

### Configuration managament policy

The policy is to 
- include a Root.cs, which 
- sets default values for all properties, then
- includes a Setup.cs, which
- sets a SetupName property, which
- is evaluated by a switch/case statement in Root.cs, which
- includes another config file with specific settings for Debug, Build, or Production, where
- Production.cs will not be in the source repository to protected sensitive data, and
- Setup.cs will not be in the source repository, because it will be different for each setup.

Root.cs:

    namespace MyProgram.Configuration
    {
      class Root
      {
        public static void Run(MyProgram.MyConfig config)
        {
          config.SomeProperty = "http://localhost:8080/";
          config.OrAsMemberVariable = 42;
          config.OrRealCLRTypes = DateTime.Now;
          config.DatabasePassword = "-empty-";
          
          config.Include("Setup.cs");

          switch (config.SetupName) {
            case "Debug": config.Include("Debug.cs"); break;
            case "Production": config.Include("Production.cs"); break;
          }
        }
      }
    }

Setup.cs (of a production/live system):

    namespace MyProgram.Configuration
    {
      class Setup
      {
        public static void Run(MyProgram.MyConfig config)
        {
          //config.SetupName == "Debug") { 
          //config.SetupName == "Build") { 
          config.SetupName == "Production") { 
        }
      }
    }

Production.cs:

    namespace MyProgram.Configuration
    {
      class Production
      {
        public static void Run(MyProgram.MyConfig config)
        {
          config.DatabasePassword = "jDf2o3Gzdt6iZk"; // Real production DB password
        }
      }
    }


