using System.Collections.Immutable;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using System.Net.Quic;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TCore.Exceptions;
using TCore.Logging;
using tt.Types.Model;
using tt.Extensions;
using tt.Types.Model;

namespace tt.Executors;

public class Executor
{
    public delegate Task<U> PostExecuteDelegate<T, U>(ExecutorContext context, T? t);

    public delegate Task CustomizeResponseDelegate<TRet>(HttpResponseData response, TRet ret);

    public delegate Task<U> ExecuteGetDelAsync<U>(ExecutorContext context);

    public static async Task<HttpResponseData> BuildResponse<TRet>(
        HttpRequestData req, TRet ret, CustomizeResponseDelegate<TRet>? customizeResponse = null, HttpStatusCode status = HttpStatusCode.OK)
    {
        HttpResponseData response = req.CreateResponse(status);

        response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
        response.Headers.Add("Access-Control-Allow-Credentials", "true");

        if (customizeResponse != null)
            await customizeResponse(response, ret);

        await response.WriteAsJsonAsync(ret);

        return response;
    }

    public static async Task<TBody?> GetParamsFromPostBody<TBody>(HttpRequestData req, CorrelationID crid, ILogger? logger) where TBody : class
    {
#if DEBUG
        logger?.LogInformation($"executing post");
#endif
        string? jsonRequest = await req.ReadAsStringAsync();
        TBody? body = jsonRequest != null ? JsonConvert.DeserializeObject<TBody>(jsonRequest) : null;

        return body;
    }

    public static async Task<HttpResponseData> ExecutePost<TBody, TRet>(
        HttpRequestData request, FunctionContext context, CorrelationID crid, ILogger? logger, PostExecuteDelegate<TBody, TRet> executePost,
        CustomizeResponseDelegate<TRet>? customizeResponse = null)
        where TBody : class
    {
        try
        {
            ExecutorContext executorContext = new(
                request,
                context,
                crid,
                logger,
                context.Claims());

            TBody? body = await GetParamsFromPostBody<TBody>(request, crid, logger);
            TRet ret = await executePost(executorContext, body);

            return await BuildResponse(request, ret, customizeResponse);
        }
        catch (TtExceptionAuthenticationFailure exc)
        {
            return await BuildResponse(request, $"unauthorized: {exc.Message}", null, HttpStatusCode.Unauthorized);
        }
        catch (SqlException sqlex)
        {
            return await BuildResponse(request, $"fatal sql exception: {sqlex.Message}", null, HttpStatusCode.InternalServerError);
        }
        catch (TcException tcex)
        {
            return await BuildResponse(request, $"fatal core exception: {tcex.Message}", null, HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            return await BuildResponse(request, $"fatal general exception: {ex.Message}", null, HttpStatusCode.InternalServerError);
        }
    }

    public static async Task<HttpResponseData> ExecuteGet<TRet>(
        HttpRequestData request, FunctionContext context, CorrelationID crid, ILogger? logger, ExecuteGetDelAsync<TRet> del,
        CustomizeResponseDelegate<TRet>? customizeResponse = null)
    {
        try
        {
#if DEBUG
            logger?.LogInformation($"executing get");
#endif
            ExecutorContext executorContext = new(
                request,
                context,
                crid,
                logger,
                context.Claims());

            TRet ret = await del(executorContext);
            return await BuildResponse(request, ret, customizeResponse);
        }
        catch (TtExceptionAuthenticationFailure exc)
        {
            return await BuildResponse(request, $"unauthorized: {exc.Message}", null, HttpStatusCode.Unauthorized);
        }
        catch (SqlException sqlex)
        {
            return await BuildResponse(request, $"fatal sql exception: {sqlex.Message}", null, HttpStatusCode.InternalServerError);
        }
        catch (TcException tcex)
        {
            return await BuildResponse(request, $"fatal core exception: {tcex.Message}", null, HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            return await BuildResponse(request, $"fatal general exception: {ex.Message}", null, HttpStatusCode.InternalServerError);
        }
    }
}
