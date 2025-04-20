namespace Superdev.Maui.Tests.TestData
{
    public class TestClass3
    {
        public ITestClass1 SavedProperty
        {
            get;
            set;
        }

        public TestClass3(ITestClass1 parameter)
        {
            this.SavedProperty = parameter;
        }
    }
}
