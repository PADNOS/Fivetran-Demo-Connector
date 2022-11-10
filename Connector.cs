using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

      var apiClient = new HttpClient();

      // Create and populate forecast table
      var forecastTable = new Table<dynamic>("forecast");
      var forecastData = await apiClient.GetFromJsonAsync<JsonElement>("https://weather.padnos.com/api/forecasts");

      foreach (var forecast in forecastData.EnumerateArray())
      {
        forecastTable.CurrentItems!.Add(forecast);
      }

      // Create and populate storm event table
      var eventTable = new Table<dynamic>("storm_event");
      var stormEventData = await apiClient.GetFromJsonAsync<JsonElement>("https://weather.padnos.com/api/events");

      foreach (var stormEvent in stormEventData.EnumerateArray())
      {
        eventTable.CurrentItems!.Add(stormEvent);
      }

      // Populate context
      context.Tables.Add(forecastTable);
      context.Tables.Add(eventTable);

      // Build response
      var response = req.CreateResponse(HttpStatusCode.OK);
      response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
      response.WriteString(await context.SerializeAsync());
      return response;
    }
  }
}
