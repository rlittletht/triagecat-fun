using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using TCore.Logging;
using tt.Executors;

namespace triagecat_fun;

public class Function1
{
    private readonly ILogger<Function1> _logger;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }

    [Function("Function1")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Function1")]
        HttpRequestData req,
        FunctionContext context,
        ILogger? log)
    {
        log?.LogInformation("C# HTTP trigger function processed a request.");
        ILogger logger = context.GetLogger("TriageCat");

        logger.LogError($"TriageCat: Test invoked: log = {log}");
        
        return await Executor.ExecuteGet<string>(
            req,
            context,
            new CorrelationID(),
            log,
            async _context =>
            {
                // Your logic here
                return "Hello from Function1!";
            });
    }
}