using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace CancellationTokenDemo.Attributes;

//Sample of a TypeFilterAttribute that accepts DI and handles `OperationCanceledException`
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class HandleTimeoutAttribute(Type t) : TypeFilterAttribute(t)
{
    public HandleTimeoutAttribute() : this(typeof(HandleTimeoutAttributeImpl))
    {}

    private class HandleTimeoutAttributeImpl(ILogger logger) : ExceptionFilterAttribute
    {
        //Using this TypeFilter pattern, you can inject dependencies on attributes...

        //...and handle whatever you need.
        public override void OnException(ExceptionContext context)
        {
            logger.LogWarning("Handling exception response...");
            if(context.Exception is OperationCanceledException)
                context.Result = new ObjectResult(null)
                {
                    StatusCode = 408,
                    Value = "Request cancelled. Please, try again"
                };
                
        }
    }
}