using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ConfigSharp.Test
{
    [TestClass]
    public partial class ContainerHttpTest
    {
        [TestMethod]
        public async Task ExecuteCodeWithHttpIncludeAsync()
        {
            // Arrange
            const string code =
@"
namespace ConfigSharp.Test
{
    class ExecuteCodeWithHttpIncludeConfigFile : ConfigSharp.Test.TestConfig
    {
        public void Load()
        {
            Include(""http://localhost:19377"");
        }
    }
}
";
            var config = new TestConfig();
            var port = 19377;

            var host = new WebHostBuilder()
                    .UseKestrel()
                    .ConfigureKestrel((context, options) => { options.ListenLocalhost(port); })
                    .Configure(a => a.Run(c => c.Response.WriteAsync(
                        @"namespace ConfigSharp.Test
                            {
                                class ExecuteCodeWithHttpIncludeConfigFile2 : ConfigSharp.Test.TestConfig
                                {
                                    public void Load()
                                    {
                                        IntMember = 42;
                                    }
                                }
                            }
                        "
                    )))
                    .Build();

            await host.StartAsync();

            // Act
            config.Execute(code);

            await host.StopAsync();

            // Assert
            Assert.AreEqual(42, config.IntMember);
        }

    }
}
