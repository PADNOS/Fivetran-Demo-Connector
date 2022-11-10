using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PADNOS.Fivetran.Functions;

namespace FivetranDemoConnector
{
  public class Connector
  {
    private readonly ILogger _logger;

    public Connector(ILoggerFactory loggerFactory)
    {
      _logger = loggerFactory.CreateLogger<Connector>();
    }

    [Function("Connector")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
      // Initialize context
      var context = await FivetranContext.FromRequestBodyAsync(req.Body);



      // Build response
      var response = req.CreateResponse(HttpStatusCode.OK);
      response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
      response.WriteString(await context.SerializeAsync());
      return response;
    }
  }
}
