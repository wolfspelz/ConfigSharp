using System;
using System.Collections.Generic;
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

        private ILoader Loader { get; set; }
        public Container Use(ILoader loader) { Loader = loader; return this; }

        public string BaseFolder { get; set; }
        public string CurrentFile { get; protected set; }

        public Container Include(string fileName)
        {
            string code = Loader == null ? Load(fileName) : Loader.Load(fileName);
            if (string.IsNullOrEmpty(code)) { throw new Exception("No code: " + fileName); }

            var references = GetReferences(code);

            CurrentFile = fileName;
            Execute(code, references);
            CurrentFile = "";

            return this;
        }

        public string Load(string fileName)
        {
            string code = "";

            if (fileName.StartsWith("http://") || fileName.StartsWith("https://")) {
                code = LoadHttp(fileName);
            } else {
                code = LoadFile(fileName);
            }

            return code;
        }

        public string LoadFile(string fileName)
        {
            string code = "";

            try {
                var pathPart = Path.GetDirectoryName(fileName);
                if (pathPart == null) {
                    throw new Exception("File name has no path");
                }
                var filePart = Path.GetFileName(fileName);

                if (string.IsNullOrEmpty(BaseFolder)) {
                    if (Path.IsPathRooted(fileName)) {
                        BaseFolder = pathPart;
                    } else {
                        BaseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathPart);
                    }
                    Log.Info("Base folder: " + BaseFolder);
                }

                var filePath = Path.Combine(BaseFolder, filePart);

                Log.Info("Read file: " + filePath);
                code = File.ReadAllText(filePath);
            } catch (Exception ex) {
                Log.Error(ex.Message + "(" + fileName + ")");
            }

            return code;
        }

        public static string LoadHttp(string url)
        {
            string code = "";

            try {
                Log.Info("HTTP request: " + url);
                var req = (HttpWebRequest)WebRequest.Create(url);
                var resp = (HttpWebResponse)req.GetResponse();
                var stream = resp.GetResponseStream();
                if (stream == null) {
                    throw new Exception("No response stream");
                }
                var sr = new StreamReader(stream, encoding: Encoding.UTF8);
                code = sr.ReadToEndAsync().Result;
            } catch (Exception ex) {
                Log.Error(ex.Message + "(" + url + ")");
            }

            return code;
        }

        public void Execute(string code, IEnumerable<string> references)
        {
            var syntaxTree = SyntaxTree.ParseText(code);

            var compilation = Compilation.Create("ConfigSnippet", new CompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(new MetadataFileReference(GetType().Assembly.Location))
                .AddReferences(new MetadataFileReference(typeof(object).Assembly.Location)) // mscorelib
                .AddReferences(new MetadataFileReference(typeof(Container).Assembly.Location))
                //.AddReferences(new MetadataFileReference(typeof(Uri).Assembly.Location)) // System.dll
                ;

            foreach (var sReference in references) {
                compilation = compilation.AddReferences(new MetadataFileReference(sReference));
            }

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

        public IEnumerable<string> GetReferences(string sCode)
        {
            var references = new List<string>();

            var lines = sCode.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines) {
                if (line.StartsWith("//reference ")) {
                    var tokens = line.ParseCommandline();
                    if (tokens.Count != 2) {
                        throw new Exception("//reference needs an argument");
                    }
                    var reference = tokens[1].Trim(new[] { '"' });
                    if (Path.IsPathRooted(reference)) {
                        // Absolute Path: use as given
                        references.Add(reference);
                    } else {
                        // AssemblyQualifiedName
                        var type = Type.GetType(reference);
                        if (type == null) {
                            throw new Exception("No type for " + reference);
                        }
                        references.Add(type.Assembly.Location);
                    }
                }
            }

            return references;
        }
    }
}
