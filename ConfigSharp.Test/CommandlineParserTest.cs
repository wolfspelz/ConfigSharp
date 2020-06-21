using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfigSharp.Test
{
    [TestClass]
    public class CommandlineParserTest
    {
        [TestMethod]
        public void ParseCommandline()
        {
            // Arrange
            // Act
            // Assert
            Assert.AreEqual(3, @"a b c".ParseCommandline().Count);
            Assert.AreEqual(3, @"a ""b"" c".ParseCommandline().Count);
            Assert.AreEqual(3, @"a ""b c"" d".ParseCommandline().Count);
            Assert.AreEqual(3, "a \"b c\" d".ParseCommandline().Count);
            Assert.AreEqual(3, @"a ""b c"" ""d e""".ParseCommandline().Count);

            // A real case
            Assert.AreEqual(2, @"//reference ""System.Uri, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089""".ParseCommandline().Count);

            // No support for \ escaping " in "", because \ is needed in file system paths
            Assert.AreEqual(3, @"a ""b c\"" d".ParseCommandline().Count);
        }
    }
}
