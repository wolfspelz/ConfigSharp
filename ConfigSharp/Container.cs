using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

// Install-Package Roslyn.Compilers.CSharp -Version 1.2.20906.2

namespace ConfigSharp
{
    public class Container
    {
        public int Get(string sKey, int defaultValue) { return this.GetMemberValue(sKey, defaultValue); }
        public string Get(string sKey, string defaultValue) { return this.GetMemberValue(sKey, defaultValue); }
        public long Get(string sKey, long defaultValue) { return this.GetMemberValue(sKey, defaultValue); }
        public double Get(string sKey, double defaultValue) { return this.GetMemberValue(sKey, defaultValue); }
        public bool Get(string sKey, bool defaultValue) { return this.GetMemberValue(sKey, defaultValue); }
        public T Get<T>(string sKey) { return (T)this.GetMemberValue(sKey); }

        protected string BaseFolder { get; set; }
        public string CurrentFile { get; protected set; }

        public void Load(string code)
        {
            var syntaxTree = SyntaxTree.ParseText(code);

            // Should support #r syntax:
            // #r "MyAssembly.dll" 
            var compilation = Compilation.Create("ConfigSnippet", new CompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(new MetadataFileReference(GetType().Assembly.Location))
                .AddReferences(new MetadataFileReference(typeof(object).Assembly.Location)) // mscorelib
                .AddReferences(new MetadataFileReference(typeof(Uri).Assembly.Location)) // System.dll
                .AddReferences(new MetadataFileReference(typeof(Container).Assembly.Location))
                ;

            using (var assemblyStream = new MemoryStream()) {
                var compilationResult = compilation.Emit(assemblyStream);
                if (compilationResult.Success) {
                    var compiledAssembly = assemblyStream.ToArray();
                    var loadedAssembly = Assembly.Load(compiledAssembly);
                    var assemblyTypes = loadedAssembly.GetTypes();
                    foreach (var type in assemblyTypes) {
                        var members = type.GetMembers(BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static);
                        foreach (var member in members) {
                            type.InvokeMember(member.Name, BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { this });
                        }
                    }
                } else {
                    Log.Error(CurrentFile + " Diagnostics:");
                    foreach (var diagnostic in compilationResult.Diagnostics) {
                        Log.Error(diagnostic.ToString());
                    }
                }
            }
        }

        public Container Include(string fileName)
        {
            string code = "";

            try {
                if (fileName.StartsWith("http://") || fileName.StartsWith("https://")) {
                    Log.Verbose("HTTP request: " + fileName);
                    var req = (HttpWebRequest)WebRequest.Create(fileName);
                    var resp = (HttpWebResponse)req.GetResponse();
                    var stream = resp.GetResponseStream();
                    if (stream == null) { throw new Exception("No response stream"); }
                    var sr = new StreamReader(stream, encoding: Encoding.UTF8);
                    code = sr.ReadToEnd();
                    CurrentFile = fileName;
                } else {
                    var pathPart = Path.GetDirectoryName(fileName);
                    if (pathPart== null) { throw new Exception("File name has no path"); }
                    var filePart = Path.GetFileName(fileName);

                    if (string.IsNullOrEmpty(BaseFolder)) {
                        if (Path.IsPathRooted(fileName)) {
                            BaseFolder = pathPart;
                        } else {
                            BaseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathPart);
                        }
                        Log.Info("BaseFolder: " + BaseFolder);
                    }

                    var filePath = Path.Combine(BaseFolder, filePart);

                    Log.Verbose("File open: " + filePath);
                    code = File.ReadAllText(filePath);
                    CurrentFile = filePath;
                }
            } catch (Exception ex) {
                Log.Error(ex.Message + "(" + fileName + ")");
            }

            if (!string.IsNullOrEmpty(code)) {
                Load(code);
            }

            CurrentFile = "";

            return this;
        }
    }
}
