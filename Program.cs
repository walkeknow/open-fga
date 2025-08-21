using System.Text.Json;
using OpenFga.Sdk.Client;
using OpenFga.Sdk.Client.Model;
using OpenFga.Sdk.Model;
using Environment = System.Environment;

namespace open_fga;

class MyProgram
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    static async Task Main()
    {
        // Create config
        var configuration = new ClientConfiguration()
        {
            ApiUrl = Environment.GetEnvironmentVariable("FGA_API_URL") ?? "http://localhost:8080",
            StoreId = Environment.GetEnvironmentVariable("FGA_STORE_ID"), // optional, not needed for \`CreateStore\` and \`ListStores\`, required before calling for all other methods
            AuthorizationModelId = Environment.GetEnvironmentVariable("FGA_MODEL_ID"), // optional, can be overridden per request
        };
        // Create open FGA client
        var fgaClient = new OpenFgaClient(configuration);

        var store = await fgaClient.CreateStore(
            new ClientCreateStoreRequest() { Name = "FGA Demo Store" }
        );

        fgaClient.StoreId = store.Id;

        Console.WriteLine($"Store created successfully: {store.Id}");

        var body = JsonSerializer.Deserialize<ClientWriteAuthorizationModelRequest>(
            // add document.json here
            File.ReadAllText("stores/models/document.json")
        );

        var response = await fgaClient.WriteAuthorizationModel(body);
        Console.WriteLine(
            $"Authorization model written successfully: {response.AuthorizationModelId}"
        );

        // Writing Tuples
        // Tuples require:
        // the actual write request
        // write options

        var body2 = new ClientWriteRequest()
        {
            Writes =
            [
                new()
                {
                    User = "user:anne",
                    Relation = "editor",
                    Object = "document:Z",
                }
            ],
        };

        var response2 = await fgaClient.Write(body2);

        Console.WriteLine($"Tuples written successfully: {response2.ToJson()}");

        var body3 = new ClientCheckRequest
        {
            User = "user:anne",
            Relation = "reader",
            Object = "document:Z",
            ContextualTuples =
            [
                new()
                {
                    User = "user:anne",
                    Relation = "editor",
                    Object = "document:Z"
                }
            ]
        };

        var response3 = await fgaClient.Check(body3);

        Console.WriteLine($"Check result: {response3.ToJson()}");

        var body4 = new ClientListObjectsRequest
        {
            User = "user:anne",
            Relation = "reader",
            Type = "document",
        };
        var body5 = new ClientListUsersRequest
        {
            Relation = "reader",
            Object = new FgaObject { Type = "document", Id = "Z" },
            UserFilters = [new() { Type = "user" }]
        };

        var response4 = await fgaClient.ListObjects(body4);
        var response5 = await fgaClient.ListUsers(body5);

        Console.WriteLine($"List objects result: {response4.ToJson()}");
        Console.WriteLine($"List users result: {JsonSerializer.Serialize(response5, JsonOptions)}");
    }
}
