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
    using System.Linq;

    public class Container
    {
        public int Get(string key, int defaultValue) { return this.GetMemberValue(key, defaultValue); }
        public string Get(string key, string defaultValue) { return this.GetMemberValue(key, defaultValue); }
        public long Get(string key, long defaultValue) { return this.GetMemberValue(key, defaultValue); }
        public double Get(string key, double defaultValue) { return this.GetMemberValue(key, defaultValue); }
        public bool Get(string key, bool defaultValue) { return this.GetMemberValue(key, defaultValue); }
        public T Get<T>(string key) { return (T)this.GetMemberValue(key); }

        public ILoader Loader { get; set; } // public so that CopyValues copies it
        public Container Use(ILoader loader) { Loader = loader; return this; }

        public string BaseFolder { get; set; }
        public string CurrentFile { get; protected set; }

        [Obsolete("Loading all members of all classes is still supported, but must be activated. The feature will be removed later.")]
        public bool LoadAllStaticMembers = false;

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
            string code;

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
                code = sr.ReadToEnd();
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
                ;

            var refList = references.ToList();
            refList.Insert(0, typeof(Uri).Assembly.Location); // System.dll
            refList.Insert(0, typeof(Container).Assembly.Location); // ConfigSharp.dll
            refList.Insert(0, typeof(object).Assembly.Location); // mscorelib

            // Adding the application dll is a bit iffy
            var baseType = GetType().BaseType;
            var typeAssembly = GetType().Assembly;
            var appLocation = typeAssembly.Location;
            if (string.IsNullOrEmpty(appLocation) && baseType != null) {
                var baseTypeAssembly = baseType.Assembly;
                appLocation = baseTypeAssembly.Location;
            }
            refList.Insert(0, appLocation);

            foreach (var sRef in refList) {
                //if (!string.IsNullOrEmpty(sRef)) {
                compilation = compilation.AddReferences(new MetadataFileReference(sRef));
                //}
            }

            using (var assemblyStream = new MemoryStream()) {
                var compilationResult = compilation.Emit(assemblyStream);
                if (compilationResult.Success) {
                    var compiledAssembly = assemblyStream.ToArray();
                    var loadedAssembly = Assembly.Load(compiledAssembly);
                    var assemblyTypes = loadedAssembly.GetTypes();
                    foreach (var type in assemblyTypes) {
                        try {
                            var loadMethod = type.GetMembers(BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).FirstOrDefault(m => m.Name == "Load");
                            if (loadMethod != null) {
                                // If there is a Load() methid then load it
                                var cfgObject = (dynamic)Activator.CreateInstance(type);
                                CopyValues(this, cfgObject);
                                type.InvokeMember(loadMethod.Name, BindingFlags.InvokeMethod, null, cfgObject, new object[] { });
                                CopyValues(cfgObject, this);
                            } else {
#pragma warning disable 0618
                                if (LoadAllStaticMembers) {
#pragma warning restore 0618
                                    // Otherwise load all static members (backward compatibility)
                                    var members = type.GetMembers(BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static);
                                    foreach (var member in members) {
                                        type.InvokeMember(member.Name, BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { this });
                                    }
                                }
                            }
                        } catch (Exception ex) {
                            Log.Error(CurrentFile + " Exception: " + ex.Message);
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

        private static void CopyValues(object fromObj, object toObj)
        {
            var fromType = fromObj.GetType();
            var toType = toObj.GetType();

            foreach (var propertyInfo in fromType.GetProperties()) {
                var targetProp = toType.GetProperty(propertyInfo.Name);
                if (targetProp == null) continue;

                targetProp.SetValue(toObj, propertyInfo.GetValue(fromObj));
            }

            foreach (var fieldInfo in fromType.GetFields()) {
                var targetField = toType.GetField(fieldInfo.Name);
                if (targetField == null) continue;

                targetField.SetValue(toObj, fieldInfo.GetValue(fromObj));
            }
        }

        public IEnumerable<string> GetReferences(string code)
        {
            var references = new List<string>();

            var lines = code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines) {
                if (line.StartsWith("//reference ")) {
                    var tokens = line.ParseCommandline();
                    if (tokens.Count != 2) {
                        throw new Exception("//reference needs an argument (absolute path or AQN)");
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
