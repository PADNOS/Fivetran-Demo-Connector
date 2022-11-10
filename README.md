# FivetranDemoConnector
Example Azure Function for use as a Fivetran connector

All code related to Fivetran is stored in `Connector.cs`. All other files are generated as part of Visual Studio's Azure Function project template (with the HttpTrigger option). The API used by this project (https://weather.padnos.com/api) is private and is only meant as a placeholder for a real API.

## Process Overview
The general steps for creating a connector are:
1. Create a Visual Studio solution/project using the Azure Function project template.
   - When prompted, select the HttpTrigger option and keep the authentication as 'Function'. These are the settings compatible with Fivetran.
2. Install the `PADNOS.Fivetran.Functions` NuGet package.
3. Initialize the context. This allows reading the incoming request from Fivetran and serves as a "bucket" for storing data that will be sent back to Fivetran.
   ```csharp
   var context = await FivetranContext.FromRequestBodyAsync(req.Body);
   ```
4. Gather data from an API. This step can be vastly different between different APIs, notably with authenticated APIs. Some APIs may offer SDKs available as NuGet packages.
5. Create a table to store API data and populate the `CurrentItems` property with the appropriate records.
   ```csharp
   var table = new Table<dynamic>("tableName", "optionalPrimaryKey");
   table.CurrentItems = new List<object>(); // (replace with real data from the API)
   ```
6. Add the table to the context.
   ```csharp
   context.Tables.Add(table);
   ```
7. Serialize the context and return it. The `Content-Type` property *must* be `application/json` for the response to be accepted by Fivetran.
   ```csharp
   string responseData = await context.SerializeAsync();
   
   var response = req.CreateResponse(HttpStatusCode.OK);
   response.Headers.Add("Content-Type", MediaTypeNames.Application.Json);
   await response.WriteStringAsync(responseData);
   return response;
   ```

*These steps for specific to Azure Functions published to Azure. Fivetran allows functions to be developed with AWS and GCP.*
