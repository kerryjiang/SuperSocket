using System;
using System.Reflection;
using Xunit.v3;

namespace SuperSocket.Tests
{
    public class TestLifeAttribute : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest, IXunitTest test)
        {
            Console.WriteLine($"Start to test {test.TestDisplayName}...");
        }

        public override void After(MethodInfo methodUnderTest, IXunitTest test)
        {
            Console.WriteLine($"Finished the test {test.TestDisplayName}...");
        }
    }
}
