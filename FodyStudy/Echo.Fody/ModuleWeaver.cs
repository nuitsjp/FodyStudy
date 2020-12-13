using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Echo.Fody
{
    public class ModuleWeaver : BaseModuleWeaver
    {
        private MethodInfo DebugWriteLine { get; } =
            typeof(System.Diagnostics.Debug)
                .GetTypeInfo()
                .DeclaredMethods
                .Where(x => x.Name == nameof(System.Diagnostics.Debug.WriteLine))
                .Single(x =>
                {
                    var parameters = x.GetParameters();
                    return parameters.Length == 1 &&
                           parameters[0].ParameterType == typeof(string);
                });

        public override void Execute()
        {
            var methods = ModuleDefinition
                .Types
                .SelectMany(x => x.Methods);
            foreach (var method in methods)
            {
                var processor = method.Body.GetILProcessor();
                var current = method.Body.Instructions.First();

                processor.InsertBefore(current, Instruction.Create(OpCodes.Nop));
                processor.InsertBefore(current, Instruction.Create(OpCodes.Ldstr, $"DEBUG: {method.DeclaringType.Name}#{method.Name}()"));
                processor.InsertBefore(current, Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(DebugWriteLine)));
            }
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "mscorlib";
            yield return "System";
            yield return "netstandard";
            yield return "System.Diagnostics.Tools";
            yield return "System.Diagnostics.Debug";
            yield return "System.Runtime";
        }

        public override bool ShouldCleanReference => true;
    }
}
