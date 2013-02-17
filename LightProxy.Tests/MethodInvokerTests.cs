﻿namespace LightProxy.Tests
{
    using System;
    using System.Diagnostics;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    [TestClass]
    public class MethodInvokerTests
    {
        [TestMethod]
        public void Invoke_MethodWithNoParameters_IsInvoked()
        {
            var targetMock = new Mock<IMethodWithNoParameters>();
            var method = typeof(IMethodWithNoParameters).GetMethods()[0];
            var methodInvoker = GetMethodInvoker();

            methodInvoker.Invoke(method, targetMock.Object, new object[] { });

            targetMock.Verify(t => t.Execute(), Times.Once());
        }

        [TestMethod]
        public void Invoke_MethodWithReferenceTypeParameter_IsInvoked()
        {
            var targetMock = new Mock<IMethodWithReferenceTypeParameter>();
            var method = typeof(IMethodWithReferenceTypeParameter).GetMethods()[0];
            var methodInvoker = GetMethodInvoker();

            methodInvoker.Invoke(method, targetMock.Object, new object[] { "SomeValue" });

            targetMock.Verify(t => t.Execute("SomeValue"), Times.Once());
        }

        [TestMethod]
        public void Invoke_MethodWithValueTypeParameter_IsInvoked()
        {
            var targetMock = new Mock<IMethodWithValueTypeParameter>();
            var method = typeof(IMethodWithValueTypeParameter).GetMethods()[0];
            var methodInvoker = GetMethodInvoker();

            methodInvoker.Invoke(method, targetMock.Object, new object[] { 42 });

            targetMock.Verify(t => t.Execute(42), Times.Once());
        }

        [TestMethod]
        public void Invoke_MethodWithReferenceTypeOutParameter_ReturnsValueFromTarget()
        {
            var targetMock = new Mock<IMethodWithReferenceTypeOutParameter>();
            string returnValue = "AnotherValue";
            targetMock.Setup(t => t.Execute(out returnValue));
            var method = typeof(IMethodWithReferenceTypeOutParameter).GetMethods()[0];
            var methodInvoker = GetMethodInvoker();
            var arguments = new object[] { "SomeValue" };

            methodInvoker.Invoke(method, targetMock.Object, arguments);

            Assert.AreEqual("AnotherValue", (string)arguments[0]);
        }

        [TestMethod]
        public void Invoke_MethodWithValueTypeOutParameter_ReturnsValueFromTarget()
        {
            var targetMock = new Mock<IMethodWithValueTypeOutParameter>();
            int returnValue = 52;
            targetMock.Setup(t => t.Execute(out returnValue));
            var method = typeof(IMethodWithValueTypeOutParameter).GetMethods()[0];
            var methodInvoker = GetMethodInvoker();
            var arguments = new object[] { 42 };

            methodInvoker.Invoke(method, targetMock.Object, arguments);

            Assert.AreEqual(52, (int)arguments[0]);
        }

        [TestMethod]
        public void Invoke_MethodWithReferenceTypeRefParameter_ReturnsValueFromTarget()
        {                                
            var method = typeof(IMethodWithReferenceTypeRefParameter).GetMethods()[0];
            var methodInvoker = GetMethodInvoker();
            var arguments = new object[] { new ReferenceTypeFoo { Value = "SomeValue" } };
            
            methodInvoker.Invoke(method, new MethodWithReferenceTypeRefParameter(), arguments);
            Assert.AreEqual("AnotherValue", ((ReferenceTypeFoo)arguments[0]).Value);
        }

        [TestMethod]
        public void Invoke_MethodWithValueTypeRefParameter_ReturnsValueFromTarget()
        {
            var method = typeof(IMethodWithValueTypeRefParameter).GetMethods()[0];
            var methodInvoker = GetMethodInvoker();
            var arguments = new object[] { new ValueTypeFoo() { Value = "SomeValue" } };

            methodInvoker.Invoke(method, new MethodWithValueTypeRefParameter(), arguments);
            Assert.AreEqual("AnotherValue", ((ValueTypeFoo)arguments[0]).Value);
        }

        [TestMethod]
        public void Invoke_MethodWithNullableTypeParameter_PassedValueToTarget()
        {
            var targetMock = new Mock<IMethodWithNullableParameter>();
            var method = typeof(IMethodWithNullableParameter).GetMethods()[0];
            var methodInvoker = GetMethodInvoker();

            methodInvoker.Invoke(method, targetMock.Object, new object[] { 42 });

            targetMock.Verify(t => t.Execute(42), Times.Once());
        }

        [TestMethod]
        public void Invoke_MethodWithReferenceTypeReturnValue_ReturnsValue()
        {
            var targetMock = new Mock<IMethodWithReferenceTypeReturnValue>();
            targetMock.Setup(t => t.Execute()).Returns("SomeValue");
            var method = typeof(IMethodWithReferenceTypeReturnValue).GetMethods()[0];
            var methodInvoker = GetMethodInvoker();

            var result = methodInvoker.Invoke(method, targetMock.Object, new object[] { });

            Assert.AreEqual("SomeValue", (string)result);
        }

        [TestMethod]
        public void Invoke_MethodWithValueTypeReturnValue_ReturnsValue()
        {
            var targetMock = new Mock<IMethodWithValueTypeReturnValue>();
            targetMock.Setup(t => t.Execute()).Returns(42);
            var method = typeof(IMethodWithValueTypeReturnValue).GetMethods()[0];
            var methodInvoker = GetMethodInvoker();

            var result = methodInvoker.Invoke(method, targetMock.Object, new object[] { });

            Assert.AreEqual(42, (int)result);
        }

        [TestMethod]
        public void PerformanceTest()
        {
            int iterations = 1000000;
            IMethodWithReferenceTypeParameter target = new MethodWithReferenceTypeParameter();
            Measure(() => target.Execute("SomeValue"), iterations, "Direct");

            var method = typeof(IMethodWithReferenceTypeParameter).GetMethods()[0];
            //Measure(() => method.Invoke(target, new object[]{"SomeValue"}), iterations, "Reflection");

            var methodInvoker = GetMethodInvoker();
            Measure(() => methodInvoker.Invoke(method, target, new object[] { "SomeValue" }), iterations, "MethodInvoker");

            Func<object, object[], object> del = GetMethodInvoker().CreateDelegate(method);
            Measure(() => del(target, new object[] { "someValue" }), iterations, "CachedDelegate");

            Lazy<Func<object, object[], object>> lazy = new Lazy<Func<object, object[], object>>(() => methodInvoker.CreateDelegate(method));
            Measure(() => lazy.Value(target, new object[] { "someValue" }), iterations, "LazyDelegate");

        }

        private void Measure(Action action, int iterations, string description)
        {
            Console.Write(description);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                action();
            }
            stopwatch.Stop();
            Console.WriteLine(" -> ElapsedMilliSeconds: " + stopwatch.ElapsedMilliseconds);
        }


        protected virtual IMethodBuilder GetMethodInvoker()
        {
            return new MethodBuilder(() => new DynamicMethodSkeleton());
        }
    }
}