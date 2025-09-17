
namespace Superdev.Maui.Tests.TestData
{
    public class TestClass5
    {
        public ITestClass1 MyProperty
        {
            get;
            private set;
        }

        public TestClass5()
        {

        }

        public TestClass5(ITestClass1 myProperty)
        {
            this.MyProperty = myProperty;
        }
    }
}
