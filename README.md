ConfigSharp
===========

Configure your .NET application with C# config files. Config files are C# source files, managed by the Visual Studio like any other code file, intellisensed, resharpered, syntax checked, compiled, type safe. 

Write real code with control structures and classes in config files. Include other local or remote (HTTP) config files. 

No more key-value lists of string based app settings from XML. These settings are typed properties of CLR objects. 

Config files are not C# script (.csx) files. They are C#, because they are code and code wants to be intellisensed, resharpered, syntax checked, compiled, type safe. 

### 1. What it does

You have a config class which contains your app settings. An instance is populated by loading/executing C# based config files. You can then use memebrs/properties of the config object anywhere in your app. 

### 2. Examples

Program.cs:

    static void Main(string[] args)
    {
      var config = new MyConfig();
      config.Include("ConfigFile.cs");
      ...
      Console.WriteLine("config.SomeProperty = " + config.SomeProperty);
      ...
    }

    public class MyConfig : ConfigSharp.Container
    {
        public string SomeProperty { get; set; }
        public int OrAsMemberVariable = 41;
        public DateTime OrPlainOldCLRTypes;
        public string DatabasePassword { get; set; }
        public string SetupName { get; set; }
    }
    
ConfigFile.cs:

    namespace MyProgram.Configuration // any namespace
    {
        class ConfigFile // Any class name
        {
            public static void Run(MyProgram.MyConfig config) // Any method name
            {
                config.SomeProperty = "42";
                config.Include("OtherConfigFile.cs");
            }
        }
    }

ConfigSharp will execute all public methods of all public classes of any namespace in a config file. 

### 3. Using config properties

After loading the settings into the config object...

    new MyConfig().Include("ConfigFile.cs");
    
...you will use the properties. Your app settings / config properties / members of the config object can be accessed in different ways:

#### 3.1 Properties of a config object instance

    var serverAddress = config.ServerAddress;

#### 3.2 Properties of a global config object

    var serverAddress = Config.Global.ServerAddress;
    // or:
    var serverAddress = App.Settings.ApiAddress;
    
This needs a wrapper in your app code like:

    public class App : ConfigSharp.Global
    {
        public static MyConfig Settings { get { return (MyConfig)ConfigSharp.Global.Instance; } }
    }
    
Without this wrapper you'd use:

    var serverAddress = ConfigSharp.Global.Instance.ServerAddress;

#### 3.3 Getter functions with string based property name and default value

    var serverAddress = Config.Global.Get("ServerAddress", "http://localhost:8080/");
    var maxSize = Config.Global.Get("MaxMessageSize", 100 * 1024 * 1024);
    var formatJson = Config.Global.Get("FormatResponseJson", true);
    // Complex properties with type:
    var endDate = Config.Global.Get<DateTime>("ServiceEndDate");

You could also omit the "Global." and do

    var serverAddress = Config.Get("ServerAddress", "http://localhost:8080/");
    // or
    var serverAddress = AppSettings.Get("ServerAddress", "http://localhost:8080/");
    // Your choice

If you augment the your wrapper with a getter:

    public static string Get(string key, string defaultValue)
    {
        return ConfigSharp.Global.Instance.Get(key, defaultValue);
    }

### 4. What you can change when using ConfigSharp

Most libraries are opinionated. They require you to do things in certain ways. ConfigSharp has a very small API and it gives a lot of freedom with respect to naming and policies. Here are the things you can change:
- namespace in config files
- class names in config files
- method names in config files
- config object class name, e.g. MyConfig
- property names (of course)
- C# properties or memeber variables with initialization
- global config accessor name, e.g. Config.Global.MyProperty or App.Settings.MyProperty
- configuration management policy, because it is implemented by your config files

### 5. Configuration managament policy

When running in different environments (Debug, Build, Production), then you need different config files. There are debug only settings, user names, passwords , and Web service addresses, which depend on the environment. The operating department might keep a few secrets with respect to production database passwords and payment provider accounts. So, we need a way to switch easily and automatically between environments. That's where a configuration management policy comes in.  

Configuration management should 
- make switching setups easy,
- default to a "Production" configuration,
- force developers to set default values when adding properties,
- allow developers to assign probable "production" values for new properties,
- allow admins to overwrite default values easily,
- allow for repository checkout (svn update, git pull) based deployment

The recommended policy is to 
- have a Configuration folder with all local config files
- include Root.cs, which 
- sets default values for all properties, then
- includes a Setup.cs, which
- sets a SetupName property, which
- is evaluated by a switch/case statement in Root.cs, which
- includes another config file with specific settings for Debug, Build, or Production, where
- Production.cs will not be in the source repository to protected sensitive data, and
- Setup.cs will not be in the source repository, because it will be different for each setup.

This is just a BCP (best current practice). It is not hard coded in the library. You can roll your own policy. After all it's plain C# and you write it.

Root.cs:

    namespace MyProgram.Configuration
    {
      class Root
      {
        public static void Run(MyProgram.MyConfig config)
        {
          config.SomeProperty = "http://localhost:8080/";
          config.OrAsMemberVariable = 42;
          config.OrPlainOldCLRTypes = DateTime.Now;
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




