using System;

namespace SimpleInjector_Interception_Sample
{
    public class ConsoleLogger : ILogger
    {

        public void Log(string log)
        {
            Console.Out.Write(log);
        }
    }
}