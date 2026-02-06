using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading.Tasks;

namespace TinyLogger
{
    public class LoggerInterceptor(ILogger logger) : IInterceptor
    {
        private readonly ILogger _logger = logger;
        private static readonly MethodInfo _interceptAsyncMethod = typeof(LoggerInterceptor)
            .GetMethod(nameof(InterceptAsync), BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Could not find method {nameof(InterceptAsync)} on {nameof(LoggerInterceptor)}");

        public void Intercept(IInvocation invocation)
        {
            _logger.LogInformation("Calling {Method} with arguments: {Args}",
                invocation.Method.Name,
                string.Join(", ", invocation.Arguments.Select(a => a?.ToString() ?? "null")));

            try
            {
                invocation.Proceed();

                var returnType = invocation.Method.ReturnType;
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var genericArg = returnType.GetGenericArguments()[0];
                    var genericMethod = _interceptAsyncMethod.MakeGenericMethod(genericArg);
                    invocation.ReturnValue = genericMethod.Invoke(this, [invocation, invocation.ReturnValue]);
                }
                else if (invocation.ReturnValue is Task task)
                {
                    invocation.ReturnValue = InterceptAsyncTask(invocation, task);
                }
                else
                {
                    _logger.LogInformation("Method {Method} returned: {ReturnValue}", invocation.Method.Name, invocation.ReturnValue);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Method {Method} threw an exception", invocation.Method.Name);
                throw;
            }
        }

        private async Task InterceptAsyncTask(IInvocation invocation, Task task)
        {
            try
            {
                await task;
                _logger.LogInformation("Method {Method} completed successfully.", invocation.Method.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Method {Method} failed with exception.", invocation.Method.Name);
                throw;
            }
        }

        private async Task<T> InterceptAsync<T>(IInvocation invocation, Task<T> task)
        {
            try
            {
                T result = await task;
                _logger.LogInformation("Method {Method} returned: {Result}", invocation.Method.Name, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Method {Method} failed with exception.", invocation.Method.Name);
                throw;
            }
        }
    }
}
