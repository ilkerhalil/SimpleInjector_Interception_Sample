using System.Reflection;

namespace SimpleInjector_Interception_Sample
{
    public interface IInvocation
    {
        object InvocationTarget { get; }
        object ReturnValue { get; set; }
        object[] Arguments { get; }
        void Proceed();
        MethodBase GetConcreteMethod();
    }
}