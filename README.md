ConfigSharp
===========

Configure your .NET application with C# config files. Config files are C# source files, managed by Visual Studio like any other code file, intellisensed, refactorable, resharpered, syntax checked, compiled, type safe. 

Write real code with control structures and classes in config files. Include other local or remote (HTTP) config files. 

No more key-value lists of string based app settings from XML. These settings are typed properties of CLR objects, even aggregated types. No more workarounds for complex settings, which do not fit properly in strings. 

Devops friendly, because admins can *program* their config files. Developers can document settings with examples in *their own code*, which is also the *admins's config file*. 

### 1. What it does

You have a config class which contains your app settings. An instance is populated by loading/executing C# based config files. You can then use properties of the config object anywhere in your app. 

### *** Breaking change in version 1.0.7: Config file format changed ***

The class in your config file must derive from your config class and only the Load() method is loaded. This allows for local methods in config files and we can omit the argument of the config method in config files. 

Use the old format with:

    var config = new MyConfig { LoadAllStaticMembers = true };
    
(Judging from 26 nuget downloads, it is no heavily in use, so I made the change breaking by defaulting to the new file format. I promise, that this will be the last breaking change.)

### 2. Examples

Program.cs:

    static void Main(string[] args)
    {
        var config = new MyConfig();
        config.Include("ConfigFile.cs");
        ...
        var prop1 = config.SomeProperty;
        // or
        var prop2 = Config.Global.SomeProperty;
        var prop3 = App.Settings.SomeProperty;
        var prop4 = AppSettings.Get("SomeProperty", "default");
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
        class ConfigFile : MyProgram.MyConfig // Any class name derived from your config class
        {
            public void Load()
            {
                SomeProperty = "42";
                Include("OtherConfigFile.cs");
            }
        }
    }

ConfigSharp will execute the Load() method. 

### 3. Using config properties

After loading the settings into the config object...

    new MyConfig().Include("../../ConfigFile.cs");
    
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

You could also omit the *Global.* and do

    var serverAddress = Config.Get("ServerAddress", "http://localhost:8080/");
    // or
    var serverAddress = AppSettings.Get("ServerAddress", "http://localhost:8080/");
    // Your choice

If you augment the your wrapper with a getter:

    public static string Get(string key, string defaultValue)
    {
        return ConfigSharp.Global.Instance.Get(key, defaultValue);
    }

### 4. Absolute and relative paths

#### 4.1 Local files

The first config.Include() must point to the include file *relative to the working directory* or must be an *absolute path*. Include()s inside config files will take file names relative to the first include file. The config object has a BaseFolder property. If it is set, then it is used a s a base for relative paths instead of the app's working directory.

Example (in the debug environment the first call might be):

    config.Include("../../ConfigFile.cs");

Example (first call in the production environment):

    config.Include("Configuration/ConfigFile.cs");
    // or
    config.Include(@"D:\MyApp\ConfigFile.cs");

Example (inside config file relative to parent but no "config" instance, because "this" is the instance):

    Include("AdditionalConfigFile.cs");

#### 4.2 HTTP remote include

The Include() method also digests http:// and https:// URLs. Example:

    Include("https://my.config.server/MyApp/Configuration");

### 5. Additional assemblies

Config Sharp addes references to mscorelib and System.dll. That's enough for property assignments. If your config class or your config file code needs additional types, then the assemblies have to be referenced in the config file. 

Example (with absolute path):

    //reference "C:\Windows\Microsoft.Net\assembly\GAC_MSIL\System\v4.0_4.0.0.0__b77a5c561934e089\System.dll"

Example (with an Assembly Qualified Name of any type of the assembly):

    //reference "System.Uri, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
    
The *//reference* looks like a comment. It actually is a comment for the Visual Studio compiler. But *//reference* is recognized by ConfigSharp. 

Example config file:

    //reference "System.UriBuilder, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
    namespace MyProgram.Configuration
    {
        class ConfigFile : MyProgram.MyConfig
        {
            public void Load()
            {
                HomePageUrl = new UriBuilder("http", "blog.wolfspelz.de");
            }
        }
    }

Remark: unfortunately, the *#r* notation of C#-Script is not possible, because the code will be managed by Visual Studio, which will complain about an unknown preprocessor directive. Even *#pragma reference* gives a warning. 

### 6. What you can change when using ConfigSharp

Many libraries are opinionated. They require you to do things in certain ways. ConfigSharp has a very small API and it gives a lot of freedom with respect to naming and policies. Here are the things you can change:
- namespace in config files
- class names in config files
- config object class name, e.g. MyConfig
- property names (of course)
- C# properties or memeber variables with initialization
- global config accessor name, e.g. Config.Global.MyProperty or App.Settings.MyProperty
- configuration management policy, because it is implemented by your config files
- value of the BaseFolder property of the config object from inside the config file

### 7. Configuration managament policy

When running in different environments (Debug, Build, Production), then you need different config files. There are debug only settings, user names, passwords, and Web service addresses, which depend on the environment. The operating department might keep a few secrets with respect to production database passwords and payment provider accounts. We need a way to switch easily and automatically between environments. That's where a configuration management policy comes in.  

Configuration management should 
- make switching setups easy,
- encourage developers to set default values when adding properties,
- allow developers to assign *probable production values* for new properties,
- allow admins to overwrite default values easily,
- enable binary repository checkout based ready to run deployment (svn update, git pull),
- default to a working *production* configuration.

The recommended policy is to 
- have a Configuration folder with all local config files
- as a folder in the Visual Studio project, then
- include Root.cs, which 
- sets default values for all properties, then
- includes a Setup.cs, which
- sets a SetupName property, which
- is evaluated by a switch/case statement in Root.cs, which
- includes another config file with specific settings for Debug, Build, or Production, where
- Production.cs will not be in the source repository to protected sensitive data, and
- Setup.cs will not be in the source repository, because it will be different for each setup, and
- let the build process copy the entire folder to the bin output.

Root.cs:

    namespace MyProgram.Configuration
    {
      class Root : MyProgram.MyConfig
      {
        public void Load()
        {
            // Initialize all settings
            // This is a kind of config reference, which lists all settings with examples
            // especially for admins who do not check the source code, but read/write config files
            SomeProperty = "http://localhost:8080/";
            OrAsMemberVariable = 42;
            OrPlainOldCLRTypes = DateTime.Now;
            DatabasePassword = "-empty-";

            // Setup.cs just sets SetupName
            Include("Setup.cs");
            
            // Overwrite settings for the environment
            switch (SetupName) {
                case "Debug": Include("Debug.cs"); break;
                case "Production": Include("Production.cs"); break;
            }
        }
      }
    }

Setup.cs (of a production/live system):

    namespace MyProgram.Configuration
    {
      class Setup : MyProgram.MyConfig
      {
        public void Load()
        {
            //SetupName == "Debug") { 
            //SetupName == "Build") { 
            SetupName == "Production") { 
        }
      }
    }

Production.cs:

    namespace MyProgram.Configuration
    {
      class Production : MyProgram.MyConfig
      {
        public void Load()
        {
            DatabasePassword = "jDf2o3Gzdt6iZk"; // Real production DB password
        }
      }
    }

This is just a BCP (best current practice). It is not hard coded in the library. You can roll your own policy. After all it's plain C# and you write it. You could also:
- omit the Setup.cs and check the host name to find the proper configuration,
- have settings for all environments in one config file, except a few production secrets,
- implement settings as member variables of your config class with default values and have only the overrides in the config file.

