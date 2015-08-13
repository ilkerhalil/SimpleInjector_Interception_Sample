using System.Diagnostics;

namespace SimpleInjector_Interception_Sample
{
    public class PerformanceLoggingInterceptor : IInterceptor
    {

        private readonly ILogger logger;

        public PerformanceLoggingInterceptor(ILogger logger)
        {
            this.logger = logger;
        }


        public void Intercept(IInvocation invocation)
        {
            var watch = Stopwatch.StartNew();
            invocation.Proceed();
            var decoratedType = invocation.InvocationTarget.GetType();
            logger.Log($"{decoratedType.Name} executed in {watch.ElapsedMilliseconds} ms.");
        }
    }
}