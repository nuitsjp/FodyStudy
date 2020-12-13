using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Echo.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = @"..\..\..\..\Echo.Target\bin\Debug\netstandard2.0";
            var moduleFullName = Path.Combine(path, "Echo.Target.dll");
            var module = ModuleDefinition.ReadModule(moduleFullName);
            var type = module.Types.Single(x => x.Name == "Class1");
            var method = type.Methods.Single(x => x.Name == "Add");
            var first = method.Body.Instructions.First();
            var processor = method.Body.GetILProcessor();
            processor.InsertBefore(first, Instruction.Create(OpCodes.Nop));
            processor.InsertBefore(first, Instruction.Create(OpCodes.Ldstr, $"{type.Name}#{method.Name}()"));
            var consoleWriteLine = typeof(System.Console)
                .GetTypeInfo()
                .DeclaredMethods
                .Where(x => x.Name == nameof(System.Console.WriteLine))
                .Single(x =>
                {
                    var parameters = x.GetParameters();
                    return parameters.Length == 1 &&
                           parameters[0].ParameterType == typeof(string);
                });
            processor.InsertBefore(first, Instruction.Create(OpCodes.Call, module.ImportReference(consoleWriteLine)));
            module.Write(@"..\..\..\..\Echo.Target.Console\Echo.Target.dll");
        }
    }
}
