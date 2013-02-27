namespace LightProxy.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SimpleProxy.Tests;

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
        public void GetProxyType_MethodWithNoParameters_ImplementsProxyInterface()
        {
            Type proxyType = proxyBuilder.GetProxyType(typeof(IMethodWithNoParameters), Type.EmptyTypes);
            Assert.IsTrue(typeof(IProxy).IsAssignableFrom(proxyType));           
        }

        [TestMethod]
        public void GetProxyType_AdditionalInterface_ImplementsInterface()
        {
            Type proxyType = proxyBuilder.GetProxyType(typeof(IMethodWithNoParameters), new[] { typeof(IDisposable) });
            Assert.IsTrue(typeof(IDisposable).IsAssignableFrom(proxyType));
        }

        [TestMethod]
        public void GetProxyType_InterfaceWithClassGenericConstraints_ImplementsInterface()
        {
            Type proxyType = proxyBuilder.GetProxyType(typeof(IClassWithGenericClassContraint<ReferenceTypeFoo>), Type.EmptyTypes);
            Assert.IsTrue(typeof(IClassWithGenericClassContraint<ReferenceTypeFoo>).IsAssignableFrom(proxyType));
        }

        [TestMethod]
        public void CreateProxy_InterfaceProxy_OverridesGetHashCode()
        {            
            Type proxyType = proxyBuilder.GetProxyType(typeof(IMethodWithNoParameters), Type.EmptyTypes);

            Assert.AreEqual(proxyType, proxyType.GetMethod("GetHashCode").DeclaringType);
        }

        [TestMethod]
        public void CreateProxy_InterfaceProxy_OverridesToString()
        {
            Type proxyType = proxyBuilder.GetProxyType(typeof(IMethodWithNoParameters), Type.EmptyTypes);

            Assert.AreEqual(proxyType, proxyType.GetMethod("ToString").DeclaringType);
        }

        [TestMethod]
        public void CreateProxy_InterfaceProxy_OverridesEquals()
        {
            Type proxyType = proxyBuilder.GetProxyType(typeof(IMethodWithNoParameters), Type.EmptyTypes);

            Assert.AreEqual(proxyType, proxyType.GetMethod("Equals").DeclaringType);
        }

        [TestMethod]
        public void CreateProxy_WithoutMethodSelector_DoesNotInterceptGetHashCode()
        {
            var interceptorMock = new Mock<IInterceptor>();
            var targetMock = new Mock<IMethodWithNoParameters>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);
            proxy.GetHashCode();
            
            interceptorMock.Verify(m => m.Invoke(It.Is<InvocationInfo>(ii => ii.Method.Name == "GetHashCode")),Times.Never());
        }


        [TestMethod]
        public void Execute_MethodWithReferenceTypeParameter_PassesValueToInterceptor()
        {
            var interceptorMock = new Mock<IInterceptor>();
            var targetMock = new Mock<IMethodWithReferenceTypeParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);

            proxy.Execute("SomeValue");

            interceptorMock.Verify(
                i => i.Invoke(
                    It.Is<InvocationInfo>(ii => (string)ii.Arguments[0] == "SomeValue")));            
        }

        [TestMethod]
        public void Execute_MethodWithValueTypeParameter_PassesValueToInterceptor()
        {
            var interceptorMock = new Mock<IInterceptor>();
            var targetMock = new Mock<IMethodWithValueTypeParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);

            proxy.Execute(42);

            interceptorMock.Verify(
                i => i.Invoke(
                    It.Is<InvocationInfo>(ii => (int)ii.Arguments[0] == 42)));
        }

        [TestMethod]
        public void Execute_MethodWithNullableParameter_PassesValueToInterceptor()
        {
            var interceptorMock = new Mock<IInterceptor>();
            var targetMock = new Mock<IMethodWithNullableParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);

            proxy.Execute(42);

            interceptorMock.Verify(
                i => i.Invoke(
                    It.Is<InvocationInfo>(ii => (int)ii.Arguments[0] == 42)));
        }

        [TestMethod]
        public void Execute_MethodWithGenericParameter_PassesValueToInterceptor()
        {
            var interceptorMock = new Mock<IInterceptor>();
            var targetMock = new Mock<IMethodWithGenericParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);

            proxy.Execute(42);

            interceptorMock.Verify(
                i => i.Invoke(
                    It.Is<InvocationInfo>(ii => (int)ii.Arguments[0] == 42)));
        }

        [TestMethod]
        public void Execute_MethodWithReferenceTypeOutParameter_PassesNullToInterceptor()
        {            
            var interceptorMock = new Mock<IInterceptor>();            
            var targetMock = new Mock<IMethodWithReferenceTypeOutParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);

            string value;
            proxy.Execute(out value);

            interceptorMock.Verify(
                i => i.Invoke(
                    It.Is<InvocationInfo>(ii => (string)ii.Arguments[0] == null)));
        }

        [TestMethod]
        public void Execute_MethodWithReferenceTypeRefParameter_PassesValueToInterceptor()
        {
            var interceptorMock = new Mock<IInterceptor>();
            var targetMock = new Mock<IMethodWithReferenceTypeRefParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);

            var foo = new ReferenceTypeFoo();
            proxy.Execute(ref foo);

            interceptorMock.Verify(
                i => i.Invoke(
                    It.Is<InvocationInfo>(ii => (ReferenceTypeFoo)ii.Arguments[0] == foo)));
        }

        [TestMethod]
        public void Execute_MethodWithValueTypeOutParameter_PassesDefaultValueToInterceptor()
        {         
            var interceptorMock = new Mock<IInterceptor>();        
            var targetMock = new Mock<IMethodWithValueTypeOutParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);

            int value;
            proxy.Execute(out value);

            interceptorMock.Verify(
                i => i.Invoke(
                    It.Is<InvocationInfo>(ii => (int)ii.Arguments[0] == 0)));
        }

        [TestMethod]
        public void Execute_MethodWithReferenceTypeReturnValue_ReturnsValueFromInterceptor()
        {
            var interceptorMock = new Mock<IInterceptor>();
            interceptorMock.Setup(m => m.Invoke(It.IsAny<InvocationInfo>())).Returns("SomeValue");
            var targetMock = new Mock<IMethodWithReferenceTypeReturnValue>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);

            var result = proxy.Execute();

            Assert.AreEqual("SomeValue", result);
        }

        [TestMethod]
        public void Execute_MethodWithValueTypeReturnValue_ReturnsValueFromInterceptor()
        {
            var interceptorMock = new Mock<IInterceptor>();
            interceptorMock.Setup(m => m.Invoke(It.IsAny<InvocationInfo>())).Returns(42);
            var targetMock = new Mock<IMethodWithValueTypeReturnValue>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);

            var result = proxy.Execute();

            Assert.AreEqual(42, result);
        }

        [TestMethod]
        public void Execute_MethodWithReferenceTypeReturnValue_ReturnsValueFromTarget()
        {
            var interceptorMock = new SampleInterceptor();
            var targetMock = new Mock<IMethodWithReferenceTypeReturnValue>();
            targetMock.Setup(m => m.Execute()).Returns("SomeValue");
            var proxy = CreateProxy(targetMock.Object, interceptorMock);

            var result = proxy.Execute();

            Assert.AreEqual("SomeValue", result);
        }

        [TestMethod]
        public void Execute_MethodWithValueTypeReturnValue_ReturnsValueFromTarget()
        {
            var interceptorMock = new SampleInterceptor();            
            var targetMock = new Mock<IMethodWithValueTypeReturnValue>();
            targetMock.Setup(m => m.Execute()).Returns(42);
            var proxy = CreateProxy(targetMock.Object, interceptorMock);

            var result = proxy.Execute();

            Assert.AreEqual(42, result);
        }
       
        [TestMethod]
        public void Execute_MethodWithReferenceTypeOutParameter_ReturnsValueFromInterceptor()
        {
            var interceptorMock = new Mock<IInterceptor>();
            interceptorMock.Setup(m => m.Invoke(It.IsAny<InvocationInfo>())).Callback<InvocationInfo>(ii => ii.Arguments[0] = "AnotherValue");
            var targetMock = new Mock<IMethodWithReferenceTypeOutParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);

            string value;
            proxy.Execute(out value);

            Assert.AreEqual("AnotherValue", value);
        }

        [TestMethod]
        public void Execute_MethodWithValueTypeOutParameter_ReturnsValueFromInterceptor()
        {
            var interceptorMock = new Mock<IInterceptor>();
            interceptorMock.Setup(m => m.Invoke(It.IsAny<InvocationInfo>())).Callback<InvocationInfo>(ii => ii.Arguments[0] = 52);
            var targetMock = new Mock<IMethodWithValueTypeOutParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);

            int value;
            proxy.Execute(out value);

            Assert.AreEqual(52, value);
        }
        
        [TestMethod]
        public void Execute_MethodWithReferenceTypeRefParameter_ReturnsValueFromInterceptor()
        {
            var interceptorMock = new Mock<IInterceptor>();
            interceptorMock.Setup(m => m.Invoke(It.IsAny<InvocationInfo>()))
                .Callback<InvocationInfo>(ii => ii.Arguments[0] = new ReferenceTypeFoo { Value = "AnotherValue" });
            var targetMock = new Mock<IMethodWithReferenceTypeRefParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);
            var foo = new ReferenceTypeFoo { Value = "SomeValue" };

            proxy.Execute(ref foo);

            Assert.AreEqual("AnotherValue", foo.Value);
        }

        [TestMethod]
        public void Execute_MethodWithValueTypeRefParameter_ReturnsValueFromInterceptor()
        {
            var interceptorMock = new Mock<IInterceptor>();
            interceptorMock.Setup(m => m.Invoke(It.IsAny<InvocationInfo>())).Callback<InvocationInfo>(ii => ii.Arguments[0] = new ValueTypeFoo { Value = "AnotherValue" });
            var targetMock = new Mock<IMethodWithValueTypeRefParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);

            var foo = new ValueTypeFoo { Value = "SomeValue" };
            proxy.Execute(ref foo);

            Assert.AreEqual("AnotherValue", foo.Value);
        }
    
        [TestMethod]
        public void Execute_MethodWithReferenceTypeParameter_PassesValueToTarget()
        {
            var interceptor = new SampleInterceptor();
            var targetMock = new Mock<IMethodWithReferenceTypeParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptor);

            proxy.Execute("SomeValue");

            targetMock.Verify(t => t.Execute("SomeValue"));
        }

        [TestMethod]
        public void Execute_MethodWithValueTypeParameter_PassesValueToTarget()
        {
            var interceptor = new SampleInterceptor();
            var targetMock = new Mock<IMethodWithValueTypeParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptor);

            proxy.Execute(42);

            targetMock.Verify(t => t.Execute(42));
        }

        [TestMethod]
        public void Execute_MethodWithNullableParameter_PassesValueToTarget()
        {
            var interceptor = new SampleInterceptor();
            var targetMock = new Mock<IMethodWithNullableParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptor);

            proxy.Execute(42);

            targetMock.Verify(t => t.Execute(42));
        }

        [TestMethod]
        public void Execute_MethodWithGenericParameter_PassesValueToTarget()
        {
            var interceptor = new SampleInterceptor();
            var targetMock = new Mock<IMethodWithGenericParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptor);

            proxy.Execute(42);

            targetMock.Verify(t => t.Execute(42));
        }

        [TestMethod]
        public void Execute_MethodWithReferenceTypeOutParameter_ReturnsValueFromTarget()
        {
            var interceptor = new SampleInterceptor();            
            var targetMock = new Mock<IMethodWithReferenceTypeOutParameter>();
            string returnValue = "AnotherValue";
            targetMock.Setup(t => t.Execute(out returnValue));
            var proxy = CreateProxy(targetMock.Object, interceptor);

            string value;
            proxy.Execute(out value);

            Assert.AreEqual("AnotherValue", value);
        }

        [TestMethod]
        public void Execute_MethodWithValueTypeOutParameter_ReturnsValueFromTarget()
        {
            var interceptor = new SampleInterceptor();
            var targetMock = new Mock<IMethodWithValueTypeOutParameter>();
            int returnValue = 42;
            targetMock.Setup(t => t.Execute(out returnValue));
            var proxy = CreateProxy(targetMock.Object, interceptor);

            int value;
            proxy.Execute(out value);

            Assert.AreEqual(42, value);
        }

        [TestMethod]
        public void Execute_MethodWithReferenceTypeRefParameter_ReturnsValueFromTarget()
        {
            var interceptor = new SampleInterceptor();
            IMethodWithReferenceTypeRefParameter targetMock = new MethodWithReferenceTypeRefParameter();                        
            var proxy = CreateProxy(targetMock, interceptor);
            var foo = new ReferenceTypeFoo { Value = "SomeValue" };
            
            proxy.Execute(ref foo);

            Assert.AreEqual("AnotherValue", foo.Value);
        }

        [TestMethod]
        public void Execute_MethodWithValueTypeRefParameter_ReturnsValueFromTarget()
        {
            var interceptorMock = new Mock<IInterceptor>();
            interceptorMock.Setup(m => m.Invoke(It.IsAny<InvocationInfo>())).Callback<InvocationInfo>(ii => ii.Arguments[0] = new ValueTypeFoo { Value = "AnotherValue" });
            var targetMock = new Mock<IMethodWithValueTypeRefParameter>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);

            var foo = new ValueTypeFoo { Value = "SomeValue" };
            proxy.Execute(ref foo);

            Assert.AreEqual("AnotherValue", foo.Value);
        }

        [TestMethod]
        public void Execute_MethodWithGenericConstraint_PassedValueToInterceptor()
        {
            var interceptorMock = new Mock<IInterceptor>();
            var targetMock = new Mock<IMethodWithGenericConstraint>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object);

            var foo = new ReferenceTypeFoo();
            proxy.Execute(foo);

            interceptorMock.Verify(
                i => i.Invoke(
                    It.Is<InvocationInfo>(ii => (ReferenceTypeFoo)ii.Arguments[0] == foo)));
        }
       
        [TestMethod]
        public void Execute_MethodSelectorWithFalsePredicate_DoesNotInvokeInterceptor()
        {
            var interceptorMock = new Mock<IInterceptor>();
            var targetMock = new Mock<IMethodWithNoParameters>();
            var proxy = CreateProxy(targetMock.Object, interceptorMock.Object, mi => false);

            proxy.Execute();

            interceptorMock.Verify(i => i.Invoke(It.IsAny<InvocationInfo>()), Times.Never());
        }

        [TestMethod]
        public void GetProxyType_InterfaceWithProperty_ProxyImplementsProperty()
        {
            var proxyBuiler = new ProxyBuilder();
            var proxyType = proxyBuiler.GetProxyType(typeof(IClassWithReferenceTypeProperty), Type.EmptyTypes);
            Assert.AreEqual(1, proxyType.GetProperties().Length);
        }

        [TestMethod]
        public void GetProxyType_InterfaceWithEvent_ProxyImplementsEvent()
        {
            var proxyBuiler = new ProxyBuilder();
            var proxyType = proxyBuiler.GetProxyType(typeof(IClassWithEvent), Type.EmptyTypes);
            Assert.AreEqual(1, proxyType.GetEvents().Length);            
        }

        //[TestMethod]
        //public void GetProxyType_TargetFactory_InvokesTargetFactoryWhenProxyInstanceIsCreated()
        //{
        //    var proxyBuiler = new ProxyBuilder();
        //    var interceptorMock = new Mock<IInterceptor>();
        //    var targetMock = new Mock<IMethodWithNoParameters>();
        //    var targetFactoryMock = new Mock<ITargetFactory>();
        //    targetFactoryMock.Setup(f => f.GetTarget()).Returns(targetMock.Object);

        //    proxyBuiler.GetProxyType(typeof(IMethodWithNoParameters), Type.EmptyTypes, targetFactoryMock.Object);

        //}


        private static T CreateProxy<T>(T target, IInterceptor interceptor)
        {
            var proxyFactory = new ProxyFactory();
            return (T)proxyFactory.CreateProxy(typeof(T), Type.EmptyTypes, target, interceptor);
        }

        private static T CreateProxy<T>(T target, IInterceptor interceptor, Func<MethodInfo, bool> methodSelector)
        {
            var proxyFactory = new ProxyFactory();
            return (T)proxyFactory.CreateProxy(typeof(T), Type.EmptyTypes, target, interceptor, methodSelector);
        }
    }
}