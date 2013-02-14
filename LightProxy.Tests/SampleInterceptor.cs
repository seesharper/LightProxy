namespace LightProxy.Tests
{
    public class SampleInterceptor : IInterceptor
    {
        public object Invoke(InvocationInfo invocationInfo)
        {
            return invocationInfo.Proceed(invocationInfo.Target, invocationInfo.Arguments);
        }
    }
}