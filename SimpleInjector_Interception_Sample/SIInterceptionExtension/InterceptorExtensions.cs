using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SimpleInjector;

namespace SimpleInjector_Interception_Sample
{
    public static class InterceptorExtensions
    {
        public static void InterceptWith<TInterceptor>(this Container container,
            Func<Type, bool> predicate)
            where TInterceptor : class, IInterceptor
        {
            RequiresIsNotNull(container, "container");
            RequiresIsNotNull(predicate, "predicate");
            container.Options.ConstructorResolutionBehavior.GetConstructor(
                typeof(TInterceptor), typeof(TInterceptor));

            var interceptWith = new InterceptionHelper(container)
            {
                BuildInterceptorExpression =
                    e => BuildInterceptorExpression<TInterceptor>(container),
                Predicate = type => predicate(type)
            };

            container.ExpressionBuilt += interceptWith.OnExpressionBuilt;
        }

        public static void InterceptWith(this Container container,
            Func<IInterceptor> interceptorCreator, Func<Type, bool> predicate)
        {
            RequiresIsNotNull(container, "container");
            RequiresIsNotNull(interceptorCreator, "interceptorCreator");
            RequiresIsNotNull(predicate, "predicate");

            var interceptWith = new InterceptionHelper(container)
            {
                BuildInterceptorExpression =
                    e => Expression.Invoke(Expression.Constant(interceptorCreator)),
                Predicate = type => predicate(type)
            };

            container.ExpressionBuilt += interceptWith.OnExpressionBuilt;
        }

        public static void InterceptWith(this Container container,
            Func<ExpressionBuiltEventArgs, IInterceptor> interceptorCreator,
            Func<Type, bool> predicate)
        {
            RequiresIsNotNull(container, "container");
            RequiresIsNotNull(interceptorCreator, "interceptorCreator");
            RequiresIsNotNull(predicate, "predicate");

            var interceptWith = new InterceptionHelper(container)
            {
                BuildInterceptorExpression = e => Expression.Invoke(
                    Expression.Constant(interceptorCreator),
                    Expression.Constant(e)),
                Predicate = type => predicate(type)
            };

            container.ExpressionBuilt += interceptWith.OnExpressionBuilt;
        }

        public static void InterceptWith(this Container container,
            IInterceptor interceptor, Func<Type, bool> predicate)
        {
            RequiresIsNotNull(container, "container");
            RequiresIsNotNull(interceptor, "interceptor");
            RequiresIsNotNull(predicate, "predicate");

            var interceptWith = new InterceptionHelper(container)
            {
                BuildInterceptorExpression = e => Expression.Constant(interceptor),
                Predicate = predicate
            };

            container.ExpressionBuilt += interceptWith.OnExpressionBuilt;
        }

        [DebuggerStepThrough]
        private static Expression BuildInterceptorExpression<TInterceptor>(
            Container container)
            where TInterceptor : class
        {
            var interceptorRegistration = container.GetRegistration(typeof(TInterceptor));

            if (interceptorRegistration == null)
            {
                // This will throw an ActivationException
                container.GetInstance<TInterceptor>();
            }

            return interceptorRegistration.BuildExpression();
        }

        private static void RequiresIsNotNull(object instance, string paramName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        private class InterceptionHelper
        {
            private static readonly MethodInfo NonGenericInterceptorCreateProxyMethod = (
                from method in typeof(Interceptor).GetMethods()
                where method.Name == "CreateProxy"
                where method.GetParameters().Length == 3
                select method)
                .Single();

            public InterceptionHelper(Container container)
            {
                this.Container = container;
            }

            internal Container Container { get; private set; }

            internal Func<ExpressionBuiltEventArgs, Expression> BuildInterceptorExpression
            {
                get;
                set;
            }

            internal Func<Type, bool> Predicate { get; set; }

            [DebuggerStepThrough]
            public void OnExpressionBuilt(object sender, ExpressionBuiltEventArgs e)
            {
                if (this.Predicate(e.RegisteredServiceType))
                {
                    ThrowIfServiceTypeNotAnInterface(e);
                    e.Expression = this.BuildProxyExpression(e);
                }
            }

            [DebuggerStepThrough]
            private static void ThrowIfServiceTypeNotAnInterface(ExpressionBuiltEventArgs e)
            {
                // NOTE: We can only handle interfaces, because
                // System.Runtime.Remoting.Proxies.RealProxy only supports interfaces.
                if (!e.RegisteredServiceType.IsInterface)
                {
                    throw new NotSupportedException("Can't intercept type " +
                                                    e.RegisteredServiceType.Name + " because it is not an interface.");
                }
            }

            [DebuggerStepThrough]
            private Expression BuildProxyExpression(ExpressionBuiltEventArgs e)
            {
                var interceptor = this.BuildInterceptorExpression(e);

                // Create call to
                // (ServiceType)Interceptor.CreateProxy(Type, IInterceptor, object)
                var proxyExpression =
                    Expression.Convert(
                        Expression.Call(NonGenericInterceptorCreateProxyMethod,
                            Expression.Constant(e.RegisteredServiceType, typeof(Type)),
                            interceptor,
                            e.Expression),
                        e.RegisteredServiceType);

                if (e.Expression is ConstantExpression && interceptor is ConstantExpression)
                {
                    return Expression.Constant(CreateInstance(proxyExpression),
                        e.RegisteredServiceType);
                }

                return proxyExpression;
            }

            [DebuggerStepThrough]
            private static object CreateInstance(Expression expression)
            {
                var instanceCreator = Expression.Lambda<Func<object>>(expression,
                    new ParameterExpression[0])
                    .Compile();

                return instanceCreator();
            }
        }
    }
}