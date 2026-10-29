using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using tt.Extensions;

namespace tt.Middleware;

public class FetchOptions : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        HttpRequestData? request = await context.GetHttpRequestDataAsync();

        if (request == null || request.Method != "OPTIONS")
        {
            // If not an HTTP request or not OPTIONS
            await next(context);
            return;
        }

        List<HttpTriggerAttribute> triggerAttributes = ReflectionHelp.GetCustomAttributesOnParameters<HttpTriggerAttribute>(context);

        List<string> methods = new();

        // figure out the type of methods allowed
        foreach (HttpTriggerAttribute attribute in triggerAttributes)
        {
            if (attribute.Methods != null)
                methods.AddRange(attribute.Methods);
        }

        bool needOptions = true;

        for (int i = 0; i < methods.Count; i++)
        {
            methods[i] = methods[i].ToUpper();
            if (methods[i] == "OPTIONS")
                needOptions = false;
        }

        if (needOptions)
            methods.Add("OPTIONS");

        string? origin = request.Origin();

        HttpResponseData response = request.CreateResponse(System.Net.HttpStatusCode.OK);

        response.Headers.Add("Access-Control-Allow-Credentials", "true");
        
        ILogger? logger = context.GetLogger("FetchOptionsMiddleware");

        logger?.Log(LogLevel.Error, "Starting FetchOptions handling");

        if (origin != null)
            response.Headers.Add("Access-Control-Allow-Origin", origin);

        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        response.Headers.Add("Access-Control-Methods", string.Join(",", methods).ToUpper());

        context.GetInvocationResult().Value = response;
        // don't process the remaining middleware. we are done.
    }
}
