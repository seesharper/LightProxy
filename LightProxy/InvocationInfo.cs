namespace LightProxy
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Represents a class that intercepts method calls.
    /// </summary>
    public interface IInterceptor
    {
        /// <summary>
        /// Intercepts a method call.
        /// </summary>
        /// <param name="invocationInfo">The <see cref="InvocationInfo"/> instance that 
        /// contains information about the current method call.</param>
        /// <returns>The return value from the method.</returns>
        object Intercept(InvocationInfo invocationInfo);
    }

    /// <summary>
    /// Represents a class that is capable of creating a proxy <see cref="Type"/>.
    /// </summary>
    public interface IProxyBuilder
    {
        /// <summary>
        /// Gets a proxy <see cref="Type"/> that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.
        /// </summary>
        /// <param name="baseType">The base <see cref="Type"/> that the proxy will inherit from.</param>
        /// <param name="interfaces">A list of additional interfaces that the proxy type will implement.</param>
        /// <returns>A proxy type that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.</returns>
        Type GetProxyType(Type baseType, Type[] interfaces);

        /// <summary>
        /// Gets a proxy <see cref="Type"/> that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.
        /// </summary>
        /// <param name="baseType">The base <see cref="Type"/> that the proxy will inherit from.</param>
        /// <param name="interfaces">A list of additional interfaces that the proxy type will implement.</param>
        /// <param name="targetFactory">A function delegate use to create the target instance.</param>
        /// <returns>A proxy type that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.</returns>
        /// <remarks>
        /// The <paramref name="targetFactory"/> allows for lazy proxies that will have their target 
        /// created when the proxy type itself is created. The <paramref name="targetFactory"/> is implemented 
        /// as a static field member of the proxy type and invoked by the constructor.
        /// </remarks>
        Type GetProxyType(Type baseType, Type[] interfaces, Func<object> targetFactory);
    }

    /// <summary>
    /// Represents a class that is capable of creating proxy objects.
    /// </summary>
    public interface IProxyFactory
    {
        /// <summary>
        /// Creates a proxy object that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.
        /// </summary>
        /// <param name="baseType">The base <see cref="Type"/> that the proxy will inherit from.</param>
        /// <param name="interfaces">A list of additional interfaces that the proxy type will implement.</param>
        /// <param name="target">The target object to proxy.</param>
        /// <param name="interceptor">The <see cref="IInterceptor"/> used to intercept method calls.</param>
        /// <returns>A new object that implements the <paramref name="baseType"/> along with the 
        /// additional list of <paramref name="interfaces"/>.</returns>
        object CreateProxy(Type baseType, Type[] interfaces, object target, IInterceptor interceptor);
    }

    /// <summary>
    /// Implemented by all proxy types.
    /// </summary>
    public interface IProxy
    {
        /// <summary>
        /// Gets or sets the proxy target.
        /// </summary>
        object Target { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Interceptor"/> used to intercept method calls.
        /// </summary>
        IInterceptor Interceptor { get; set; }
    }

    /// <summary>
    /// Contains information about the current method invocation.
    /// </summary>
    public class InvocationInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvocationInfo"/> class.
        /// </summary>
        /// <param name="method">The <see cref="MethodInfo"/> currently being invoked.</param>
        /// <param name="target">The target object currently being invoked.</param>
        /// <param name="arguments">The arguments currently being passed to the target method.</param>
        /// <param name="proceed">The function delegate use to invoke the target method.</param>
        public InvocationInfo(MethodInfo method, object target, object[] arguments, Func<object> proceed)
        {
            Method = method;
            Target = target;
            Arguments = arguments;
            Proceed = proceed;
        }

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> currently being invoked.
        /// </summary>
        public MethodInfo Method { get; private set; }

        /// <summary>
        /// Gets the target object currently being invoked.
        /// </summary>
        public object Target { get; private set; }

        /// <summary>
        /// Gets the arguments currently being passed to the target method.
        /// </summary>
        public object[] Arguments { get; private set; }

        /// <summary>
        /// Gets a function delegate use to invoke the target method.
        /// </summary>
        public Func<object> Proceed { get; private set; }
    }  
}