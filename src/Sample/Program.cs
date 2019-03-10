using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            CSharpScript.EvaluateAsync("System.Console.WriteLine(\"Hello World\")").Wait();

            var code = @"
namespace A
{
    class B
    {
        public void C()
        {
            var D = ""E""; 
        }
    }
}
";

            var parserOptions = new CSharpParseOptions().WithKind(SourceCodeKind.Regular);
            var syntaxTree = CSharpSyntaxTree.ParseText(code, parserOptions);
            var compilerOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                //.WithUsings(new string[] { "System" })
                ;
            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof (object).Assembly.Location),
            };
            var compilation = CSharpCompilation.Create("ConfigSnippet", new SyntaxTree[] { syntaxTree }, references, compilerOptions);
            using (var assemblyStream = new MemoryStream())
            {
                var compilationResult = compilation.Emit(assemblyStream);
                if (compilationResult.Success)
                {
                    var compiledAssembly = assemblyStream.ToArray();
                    var loadedAssembly = Assembly.Load(compiledAssembly);
                    var assemblyTypes = loadedAssembly.GetTypes();
                    foreach (var type in assemblyTypes)
                    {
                        try
                        {
                            List<string> Functions = new List<string> { "Load" };
                            var loadMethod = type.GetMembers(BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).FirstOrDefault(m => Functions.Contains(m.Name));
                            if (loadMethod != null)
                            {
                                // If there is a Load() method then load it
                                var cfgObject = (dynamic)Activator.CreateInstance(type);
                                CopyValues(this, cfgObject);
                                type.InvokeMember(loadMethod.Name, BindingFlags.InvokeMethod, null, cfgObject, new object[] { });
                                CopyValues(cfgObject, this);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
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
                if (targetProp == null) { continue; }

                targetProp.SetValue(toObj, propertyInfo.GetValue(fromObj));
            }

            foreach (var fieldInfo in fromType.GetFields()) {
                var targetField = toType.GetField(fieldInfo.Name);
                if (targetField == null) { continue; }

                targetField.SetValue(toObj, fieldInfo.GetValue(fromObj));
            }
        }

    }

}
