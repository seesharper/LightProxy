namespace LightProxy.Tests
{
    using System;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MethodInvokerVerificationTests : MethodInvokerTests
    {
        protected override IMethodInvoker GetMethodInvoker()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DynamicMethodAssembly.dll");
            return new MethodInvoker(() => new MethodBuilderMethodSkeleton(path));
        }
    }
}