namespace LightProxy.Tests
{
    using System.Reflection;

    public class SampleInterceptor : IInterceptor
    {
        public object Invoke(InvocationInfo invocationInfo)
        {
            return invocationInfo.Proceed();
        }        
    }
}