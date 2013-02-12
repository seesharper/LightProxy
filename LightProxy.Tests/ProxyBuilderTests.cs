namespace LightProxy.Tests
{
    using System;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleProxy.Tests;
    using Moq;
    [TestClass]
    public class ProxyBuilderTests
    {
        private ProxyBuilder proxyBuilder;

        [TestInitialize]
        public void TestInitialize()
        {
            proxyBuilder = new ProxyBuilder();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProxyAssembly.dll");
            AssemblyAssert.IsValidAssembly(assemblyPath);
        }

        [TestMethod]
        public void GetProxyType_ImplementsProxyInterface()
        {
            Type proxyType = proxyBuilder.GetProxyType(typeof(IMethodWithNoParameters), Type.EmptyTypes);
            Assert.IsTrue(typeof(IProxy).IsAssignableFrom(proxyType));           
        }

        [TestMethod]
        public void GetProxyType_MethodWithGenericParameter_GeneratesValidAssembly()
        {
            Type proxyType = proxyBuilder.GetProxyType(typeof(IMethodWithGenericParameter), Type.EmptyTypes);
        }


        [TestMethod]
        public void Execute_MethodWithNoParameters_IsIntercepted()
        {
            var proxyFactory = new ProxyFactory();
            var targetMock = new Mock<IMethodWithNoParameters>();
            var interceptorMock = new Mock<IInterceptor>();
            interceptorMock.Setup(i => i.Invoke(It.IsAny<InvocationInfo>()));
            var proxy = (IMethodWithNoParameters)proxyFactory.CreateProxy(typeof(IMethodWithNoParameters), Type.EmptyTypes, targetMock.Object, interceptorMock.Object);
            proxy.Execute();
            interceptorMock.Verify(i => i.Invoke(It.IsAny<InvocationInfo>()), Times.Once());
        }


    }
}