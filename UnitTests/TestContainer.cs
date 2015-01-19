using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ConfigSharp;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;

namespace UnitTests
{
    [TestClass]
    public class TestContainer
    {
        public class TestConfig : Container
        {
            public int IntMember;
            public string StringMember;
            public int IntProperty { get; set; }
            public string StringProperty { get; set; }
            public DateTime DateTimeMember;
            public DateTime DateTimeProperty { get; set; }
            public string ExecuteCodeWithReferenceResult { get; set; }
        }

        private class TestLogLine
        {
            public string Level { get; set; }
            public string Message { get; set; }
        }

        public void DummyAvoidReSharperWarning()
        {
            var config = new TestConfig();
            config.IntMember = -1;
            config.DateTimeMember = DateTime.Now;
            config.StringMember = "xx";
            config.ExecuteCodeWithReferenceResult = "xx";
            var intProperty = config.IntProperty;
            var stringProperty = config.StringProperty;
            var dateTimeProperty = config.DateTimeProperty;
            config.IntProperty = intProperty;
            config.StringProperty = stringProperty;
            config.DateTimeProperty = dateTimeProperty;
        }

        // ----------------------------------------------------------------------

        [TestMethod]
        public void LoadCodeFromAbsolutePath()
        {
            // Arrange
            const string code =
@"
namespace UnitTests
{
    class LoadCodeFromAbsolutePathConfigFile : UnitTests.TestContainer.TestConfig
    {
        public void Load()
        {
            IntMember = 42;
        }
    }
}
";
            string fileName = Path.GetTempPath() + "ConfigSharp-UnitTest-LoadCodeFromAbsolutePath.cs";
            File.WriteAllText(fileName, code);
            var config = new TestConfig();

            // Act
            var loadedCode = config.Load(fileName);

            // Assert
            Assert.AreEqual(code, loadedCode);
        }

        [TestMethod]
        public void LoadCodeFromRelativePath()
        {
            // Arrange
            const string code =
@"
namespace UnitTests
{
    class LoadCodeFromRelativePathConfigFile : UnitTests.TestContainer.TestConfig
    {
        public void Load()
        {
            IntMember = 42;
        }
    }
}
";
            const string fileName = "ConfigSharp-UnitTest-LoadCodeFromRelativePath.cs";
            File.WriteAllText(fileName, code);

            var config = new TestConfig();

            // Act
            var loadedCode = config.Load(fileName);

            // Assert
            Assert.AreEqual(code, loadedCode);
        }

        [TestMethod]
        public void GetReferences()
        {
            // Arrange
            var refPath = typeof(Uri).Assembly.Location;
            var refName = typeof(Uri).AssemblyQualifiedName;
            //string refPath = @"C:\Windows\Microsoft.Net\assembly\GAC_MSIL\System\v4.0_4.0.0.0__b77a5c561934e089\System.dll";
            //string refName = @"System.Uri, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
            string code =
@"
//reference """ + refPath + @"""
//reference """ + refName + @"""

namespace UnitTests
{
    class ExecuteCodeConfigFile : UnitTests.TestContainer.TestConfig
    {
        public void Load()
        {
            IntMember = 42;
        }
    }
}
";
            var config = new TestConfig();

            // Act
            var references = config.GetReferences(code).ToArray();

            // Assert
            Assert.AreEqual(2, references.Length);
            Assert.AreEqual(refPath, references[0]);
            Assert.AreEqual(refPath, references[1]);
        }

        [TestMethod]
        public void ExecuteCode()
        {
            // Arrange
            const string code =
@"
namespace UnitTests
{
    class ExecuteCodeConfigFile : UnitTests.TestContainer.TestConfig
    {
        public void Load()
        {
            IntMember = 42;
        }
    }
}
";
            var config = new TestConfig { IntMember = 41 };

            // Act
            config.Execute(code, new List<string>());

            // Assert
            Assert.AreEqual(42, config.IntMember);
        }

        [TestMethod]
        public void ExecuteLoadAllStaticMembersBackwardCompatibility()
        {
            // Arrange
            const string code =
@"
namespace UnitTests
{
    public class ExecuteLoadAllStaticMembersBackwardCompatibilityConfigFile
    {
        public static void AnyStaticMemberName(UnitTests.TestContainer.TestConfig config)
        {
            config.IntMember = 42;
        }
    }
}
";
#pragma warning disable 0618
            var config = new TestConfig { IntMember = 41, LoadAllStaticMembers = true };
#pragma warning restore 0618

            // Act
            config.Execute(code, new List<string>());

            // Assert
            Assert.AreEqual(42, config.IntMember);
        }

        [TestMethod]
        public void ExecuteLoadAllStaticMembersDefaultNoBackwardCompatibility()
        {
            // Arrange
            const string code =
@"
namespace UnitTests
{
    public class ExecuteLoadAllStaticMembersDefaultNoBackwardCompatibilityConfigFile
    {
        public static void AnyStaticMemberName(UnitTests.TestContainer.TestConfig config)
        {
            config.IntMember = 42;
        }
    }
}
";
            var config = new TestConfig { IntMember = 41 };

            // Act
            config.Execute(code, new List<string>());

            // Assert
            Assert.AreEqual(41, config.IntMember);
        }

        [TestMethod]
        public void ExecuteCodeWithReference()
        {
            // Arrange
            const string code =
@"
using System;
namespace UnitTests
{
    class ExecuteCodeWithReferenceConfigFile : UnitTests.TestContainer.TestConfig
    {
        public void Load()
        {
            var ub = new UriBuilder(""http"", ""blog.wolfspelz.de"");
            ExecuteCodeWithReferenceResult = ub.ToString();
        }
    }
}
";
            var config = new TestConfig();

            // Act
            config.Execute(code, new List<string> { typeof(Uri).Assembly.Location });

            // Assert
            Assert.AreEqual("http://blog.wolfspelz.de/", config.ExecuteCodeWithReferenceResult);
        }

        [TestMethod]
        public void Getter()
        {
            // Arrange
            var config = new TestConfig {
                IntMember = 41,
                IntProperty = 41,
                StringMember = "41",
                StringProperty = "41",
            };

            // Assert
            Assert.AreEqual(41, config.Get("IntMember", 0));
            Assert.AreEqual(41, config.Get("IntProperty", 0));
            Assert.AreEqual("41", config.Get("StringMember", ""));
            Assert.AreEqual("41", config.Get("StringProperty", ""));
            Assert.AreEqual("-default-", config.Get("NotAProperty", "-default-"));
        }

        [TestMethod]
        public void TypedGetter()
        {
            // Arrange
            var config = new TestConfig {
                DateTimeMember = new DateTime(2014, 1, 2, 3, 4, 5),
                DateTimeProperty = new DateTime(2015, 6, 7, 8, 9, 10),
            };

            // Assert
            Assert.AreEqual(2014, config.Get<DateTime>("DateTimeMember").Year);
            Assert.AreEqual(2015, config.Get<DateTime>("DateTimeProperty").Year);
        }

        [TestMethod]
        public void HierarchicalTypedGetter()
        {
            // Arrange
            var config = new TestConfig {
                DateTimeMember = new DateTime(2014, 1, 2, 3, 4, 5),
                DateTimeProperty = new DateTime(2015, 6, 7, 8, 9, 10),
            };

            // Assert
            Assert.AreEqual(2014, config.Get("DateTimeMember.Date.Year", 0));
            Assert.AreEqual(2015, config.Get("DateTimeProperty.Date.Year", 0));
        }

        private class ExecuteCodeWithIncludeLoader : ILoader
        {
            public string Load(string fileName)
            {
                return @"
namespace UnitTests
{
    class ExecuteCodeWithIncludeConfigFile2 : UnitTests.TestContainer.TestConfig
    {
        public void Load()
        {
            IntMember = 42;
        }
    }
}
";
            }
        }
        [TestMethod]
        public void ExecuteCodeWithInclude()
        {
            // Arrange
            const string code =
@"
namespace UnitTests
{
    class ExecuteCodeWithIncludeConfigFile : UnitTests.TestContainer.TestConfig
    {
        public void Load()
        {
            Include(""ExecuteCodeWithIncludeConfigFile2.cs"");
        }
    }
}
";
            var config = new TestConfig();
            config.Use(new ExecuteCodeWithIncludeLoader());

            // Act
            config.Execute(code, new List<string>());

            // Assert
            Assert.AreEqual(42, config.IntMember);
        }

        [TestMethod]
        public void LoadRelative()
        {
            // Arrange
            const string code =
@"
namespace UnitTests
{
    class LoadRelativeConfigFile : UnitTests.TestContainer.TestConfig
    {
        public void Load()
        {
            Include(""ExecuteCodeWithIncludeConfigFile2.cs"");
        }
    }
}
";
            const string code2 =
@"
namespace UnitTests
{
    class LoadRelativeConfigFile2 : UnitTests.TestContainer.TestConfig
    {
        public void Load()
        {
            IntMember = 42;
        }
    }
}
";
            var tempPath = Path.GetTempPath();
            const string fileName = "ConfigSharp-UnitTest-LoadRelative.cs";
            File.WriteAllText(tempPath + fileName, code);
            const string fileName2 = "ConfigSharp-UnitTest-LoadRelative2.cs";
            File.WriteAllText(tempPath + fileName2, code2);

            var config = new TestConfig();
            config.Load(tempPath + fileName);

            // Act
            var loadedCode2 = config.Load(fileName2);

            // Assert
            Assert.AreEqual(code2, loadedCode2);
        }

        [TestMethod]
        public void AbsoluteLoadSetsBaseFolder()
        {
            // Arrange
            const string code =
@"
namespace UnitTests
{
    class AbsoluteLoadSetsBaseFolderConfigFile
    {
        public static void Run(UnitTests.TestContainer.TestConfig config)
        {
            config.Include(""ExecuteCodeWithIncludeConfigFile2.cs"");
        }
    }
}
";
            var tempPath = Path.GetTempPath();
            const string fileName = "ConfigSharp-UnitTest-AbsoluteLoadSetsBaseFolder.cs";
            File.WriteAllText(tempPath + fileName, code);
            var config = new TestConfig();

            // Act
            config.Load(tempPath + fileName);

            // Assert
            Assert.AreEqual(Path.Combine(tempPath, fileName), Path.Combine(config.BaseFolder, fileName));
        }

        [TestMethod]
        public void LogLoad()
        {
            // Arrange
            const string code =
@"
namespace UnitTests
{
    class LogLoadConfigFile : UnitTests.TestContainer.TestConfig
    {
        public void Load()
        {
            IntMember = 42;
        }
    }
}
";
            string fileName = Path.GetTempPath() + "ConfigSharp-UnitTest-LogLoad.cs";
            File.WriteAllText(fileName, code);
            var logs = new List<TestLogLine>();
            Global.Logger((logLevel, logMessage) => logs.Add(new TestLogLine { Level = logLevel, Message = logMessage }));
            var config = new TestConfig();

            // Act
            config.Load(fileName);

            // Assert
            Assert.AreEqual("Info", logs[0].Level);
            Assert.IsTrue(logs[0].Message.StartsWith("Base folder:"));
            Assert.AreEqual("Info", logs[1].Level);
            Assert.IsTrue(logs[1].Message.StartsWith("Read file:"));
        }

        public class Startup
        {
            public void Configuration(IAppBuilder appBuilder)
            {
                var config = new HttpConfiguration();
                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "{controller}/{id}",
                    defaults: new { controller = "Home", id = RouteParameter.Optional }
                );
                appBuilder.UseWebApi(config);
            }
        }
        public class HomeController : ApiController
        {
            public HttpResponseMessage Get()
            {
                const string body = @"
namespace UnitTests
{
    class ExecuteCodeWithHttpIncludeConfigFile2 : UnitTests.TestContainer.TestConfig
    {
        public void Load()
        {
            IntMember = 42;
        }
    }
}
";
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "text/plain") };
            }
        }
        [TestMethod]
        public void ExecuteCodeWithHttpInclude()
        {
            // Arrange
            const string code =
@"
namespace UnitTests
{
    class ExecuteCodeWithHttpIncludeConfigFile : UnitTests.TestContainer.TestConfig
    {
        public void Load()
        {
            Include(""http://localhost:19377"");
        }
    }
}
";
            var config = new TestConfig();
            string baseAddress = "http://localhost:19377/";
            var done = new AutoResetEvent(false);
            using (WebApp.Start<Startup>(url: baseAddress)) {
                Task.Factory.StartNew(() => {
                    //var client = new HttpClient();
                    //var response = client.GetAsync(baseAddress).Result;
                    //var result = response.Content.ReadAsStringAsync().Result;
                    //Console.WriteLine(result); 

                    // Act
                    config.Execute(code, new List<string>());

                    done.Set();
                });
                done.WaitOne(Timeout.Infinite, false);
            }

            // Assert
            Assert.AreEqual(42, config.IntMember);
        }

    }
}
