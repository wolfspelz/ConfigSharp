ConfigSharp
===========

Configure your .NET application with C# config files. Config files are C# source files, managed by Visual Studio like any other code file, intellisensed, refactorable, resharpered, syntax checked, compiled, type safe. 

Write real code with control structures and classes in config files. Include other local or remote (HTTP) config files. 

No more key-value lists of string based app settings from XML. These settings are typed properties of CLR objects, even aggregated types. No more workarounds for complex settings, which do not fit properly in strings. 

Devops friendly, because admins can *program* their config files. Developers can document settings with examples in *their own code*, which is also the *admins's config file*.

**nuget download: https://www.nuget.org/packages/ConfigSharp/**

### 1. What it does

You have a config class which contains your app settings. An instance is populated by loading/executing C# based config files. You can then use properties of the config object anywhere in your app. 

### 2. Example

Your config file is C#: ConfigFile.cs:

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

ConfigSharp will execute the Load() method of your config file. 

Your program: Program.cs:

    static void Main(string[] args)
    {
        var config = new MyConfig();
        config.Include("ConfigFile.cs");
        ...
        var prop1 = config.SomeProperty;
    }

    public class MyConfig : ConfigSharp.Container
    {
        public string SomeProperty { get; set; }
        public int OrAsMemberVariable = 41;
        public DateTime OrPlainOldCLRTypes;
        public string DatabasePassword { get; set; }
        public string SetupName { get; set; }
    }
    
### 3. Typical Use Case

#### 3.1 Global Accessor

Usually you want a globally available config accessor, not just a variable like the *var config* above. ConfigSharp has a global instance which can be used. Also, depending on your environment, the location of the config file may need a path relative to the project or the binary. 

    ConfigSharp.Global.Instance = new MyConfig();
    ConfigSharp.Global.Instance.Include("bin/Configuration/ConfigFile.cs");
    ...
    var prop1 = ConfigSharp.Global.Instance.SomeProperty;

#### 3.2 Global Wrapper

Since I don't like the writing *ConfigSharp.Global.Instance* all the time, I shorten this by defining my own global wrapper:

    public class Global
    {
        public static MyConfig Config { get { return (MyConfig)ConfigSharp.Global.Instance; } }
    }

Then I can use everywhere in my program:

    var prop1 = Global.Config.ApplicationName;

(Usually I add more *global* instances of other libraries to my global wrapper, like a logger.)

You might also call it:

    public class App
    {
        public static MyConfig Settings { get { return (MyConfig)ConfigSharp.Global.Instance; } }
    }

and use it like:

    var prop1 = App.Settings.ApplicationName;

#### 3.3 Copy Config Files to Output Directory

The goal of ConfigSharp is to edit config files like code files. But config files will not be compiled into the application. They must be copied as files to the binaries. In the properties of all config files change *Copy to Output Directory* from *Do not copy* to *Copy if newer*.

#### 3.4 Configuration Folder

You might put all config files into a *Configuration* folder. The folder will appear in the output and all config files (if there are more than one) will be available in *bin/Configuration*.

You might also put your config base class (*MyConfig.cs*) into the *Configuration* folder and also *Copy if newer* to the output folder. This might serve as a reference of available config parameters for devops. 

Putting *MyConfig.cs* into the *Configuration* folder also allows to get rid of a namespace in config files. Config classes are derived from the base config (*MyConfig*) and they are siblings in the *Configuration* folder. ConfigFile.cs:

    namespace MyProgram.Configuration // Named by the folder
    {
        class ConfigFile : MyConfig // Because the base config class is a sibling in the folder (=namespace)
        {
            public void Load()
            {
                SomeProperty = "42";
                Include("OtherConfigFile.cs");
            }
        }
    }

### 4. Using Config Properties

After loading the settings into the config object...

    new MyConfig().Include("../../ConfigFile.cs");
    
...you will use the properties. Your app settings / config properties / members of the config object can be accessed in different ways:

#### 4.1 Properties of a config object instance

    var serverAddress = config.ServerAddress;

#### 4.2 Properties of a global config object

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

#### 4.3 Getter functions with string based property name and default value

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

### 5. Absolute and Relative Paths

#### 5.1 Local Files

The first config.Include() must point to the include file *relative to the working directory* or must be an *absolute path*. Include()s inside config files will take file names relative to the first include file. The config object has a BaseFolder property. If it is set, then it is used a s a base for relative paths instead of the app's working directory.

Example (in the debug environment the first call might be):

    config.Include("../../ConfigFile.cs");

Example (first call in the production environment):

    config.Include("Configuration/ConfigFile.cs");
    // or
    config.Include(@"D:\MyApp\ConfigFile.cs");

Example (inside config file relative to parent but no "config" instance, because "this" is the instance):

    Include("AdditionalConfigFile.cs");

#### 5.2 HTTP Remote Include

The Include() method also digests http:// and https:// URLs. Example:

    Include("https://my.config.server/MyApp/Configuration");

### 6. Additional Assemblies

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

### 7. What you can change when using ConfigSharp

Many libraries are opinionated. They require you to do things in certain ways. ConfigSharp has a very small API and it gives a lot of freedom with respect to naming and policies. Here are the things you can change:
- namespace in config files
- class names in config files
- config object class name, e.g. MyConfig
- property names (of course)
- C# properties or memeber variables with initialization
- global config accessor name, e.g. Config.Global.MyProperty or App.Settings.MyProperty
- configuration management policy, because it is implemented by your config files
- value of the BaseFolder property of the config object from inside the config file

### 8. My Configuration Managament Policy

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

