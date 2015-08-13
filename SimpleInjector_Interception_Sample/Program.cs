using System;
using SimpleInjector;

namespace SimpleInjector_Interception_Sample
{
    class Program
    {
        static void Main()
        {
            var container = InitContanier();
            
            var credit = container.GetInstance<ICredit>();
            credit.CalculateCreditExpense("ilker", 0, 0);
            Console.ReadLine();

        }

        private static Container InitContanier()
        {
            var container = new Container();
            container.Register<ILogger, ConsoleLogger>();
            container.Register<ICredit, Credit>();
            //container.Register<IInterceptor,PerformanceLoggingInterceptor>();
            container.InterceptWith<PerformanceLoggingInterceptor>(type => type== typeof(ICredit));
            return container;

        }




    }
}
