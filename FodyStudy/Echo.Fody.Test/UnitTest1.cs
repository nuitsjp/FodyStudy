using System;
using Echo.Target;
using Xunit;
using Fody;

namespace Echo.Fody.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var weavingTask = new ModuleWeaver();
            var testResult = weavingTask.ExecuteTestRun("Echo.Target.dll", runPeVerify:false);
            var class1 = testResult.GetInstance("Echo.Target.Class1");
            var result = (int)class1.Add(1, 2);
        }
    }
}
