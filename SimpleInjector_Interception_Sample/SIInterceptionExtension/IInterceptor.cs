namespace SimpleInjector_Interception_Sample
{
    public interface IInterceptor
    {
        void Intercept(IInvocation invocation);
    }
}