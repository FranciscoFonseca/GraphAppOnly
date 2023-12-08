using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using Microsoft.Graph.Models;
using System.IO;
using System.Text;
using System.Net.Http.Headers;

class GraphHelper
{
    // Settings object
    private static Settings? _settings;
    // App-ony auth token credential
    private static ClientSecretCredential? _clientSecretCredential;
    // Client configured with app-only authentication
    private static GraphServiceClient? _appClient;

    public static void InitializeGraphForAppOnlyAuth(Settings settings)
    {
        _settings = settings;

        // Ensure settings isn't null
        _ = settings ??
            throw new System.NullReferenceException("Settings cannot be null");

        _settings = settings;

        if (_clientSecretCredential == null)
        {
            _clientSecretCredential = new ClientSecretCredential(
                _settings.TenantId, _settings.ClientId, _settings.ClientSecret);
        }

        if (_appClient == null)
        {
            _appClient = new GraphServiceClient(_clientSecretCredential,
                // Use the default scope, which will request the scopes
                // configured on the app registration
                // _settings.GraphUserScopes);
                new[] {"https://graph.microsoft.com/.default"});
        }
    }
    public static async Task<string> GetAppOnlyTokenAsync()
    {
        // Ensure credential isn't null
        _ = _clientSecretCredential ??
            throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

        // Request token with given scopes
        // var context = new TokenRequestContext(_settings.GraphUserScopes);
        var context = new TokenRequestContext(new[] {"https://graph.microsoft.com/.default"});
        var response = await _clientSecretCredential.GetTokenAsync(context);
        return response.Token;
    }
    public static Task<UserCollectionResponse?> GetUsersAsync()
    {
        // Ensure client isn't null
        _ = _appClient ??
            throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

        return _appClient.Users.GetAsync((config) =>
        {
            // Only request specific properties
            config.QueryParameters.Select = new[] { "displayName", "id", "mail" };
            // Get at most 25 results
            config.QueryParameters.Top = 25;
            // Sort by display name
            config.QueryParameters.Orderby = new[] { "displayName" };
        });
    }
        public async static Task CheckOnedrive(){
        _ = _appClient ??
            throw new System.NullReferenceException("Graph has not been initialized for user auth");
        try
        {
            var drive = await _appClient.Sites["ef850a69-22d8-4ed8-8c92-a901e4593253"].Lists["8a35f032-65b7-43bf-9cae-55ffeab40ab5"].Items["2"].DriveItem.GetAsync();
            var drive2 = await _appClient.Sites["ef850a69-22d8-4ed8-8c92-a901e4593253"].Sites.GetAsync();
            Console.WriteLine(JsonConvert.SerializeObject(drive));

            Console.WriteLine("/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////");
            var rootItem = await _appClient.Drives["b!aQqF79gi2E6MkqkB5FkyU5_xmBH_OZtNroxpPZPsQcMy8DWKt2W_Q5yuVf_qtAq1"].GetAsync();
            Console.WriteLine(JsonConvert.SerializeObject(rootItem));
             Console.WriteLine("/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////");
            var children = await _appClient.Drives["b!aQqF79gi2E6MkqkB5FkyU5_xmBH_OZtNroxpPZPsQcMy8DWKt2W_Q5yuVf_qtAq1"].Items["013M3PMVT5Y24DMPDPSVGKZJ3YX7UTPXRE"].Children.GetAsync();
            Console.WriteLine(JsonConvert.SerializeObject(children));

             Console.WriteLine("/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////");
            var filePath = @"C:\Users\francisco.fonseca\Downloads\2647b.jpg";
            var fileName = "smallfile.txt";
            Console.WriteLine("Uploading file: " + fileName);

            var appOnlyToken = await GetAppOnlyTokenAsync();

            string apiEndpoint = "https://graph.microsoft.com/v1.0/drives/b!aQqF79gi2E6MkqkB5FkyU5_xmBH_OZtNroxpPZPsQcMy8DWKt2W_Q5yuVf_qtAq1/items/013M3PMVT5Y24DMPDPSVGKZJ3YX7UTPXRE:/filename2.jpg:/content";
// string apiEndpoint = "https://graph.microsoft.com/v1.0/users/francisco.fonseca@grupocadelga.com/drive/items/root:/filename.jpg:/content";
        
            using (var client = new HttpClient())
            {
                // Read the binary data from the file
                byte[] fileData = File.ReadAllBytes(filePath);

                // Create a ByteArrayContent to hold the binary data
                ByteArrayContent content = new ByteArrayContent(fileData);

                // Set the content type based on the file type
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpg");

                // You can add additional headers if needed
                // content.Headers.Add("CustomHeader", "headerValue");
                // Add authorization header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", appOnlyToken);
 // Print information about the POST request
            Console.WriteLine("POST Request Information:");
            Console.WriteLine($"Endpoint: {apiEndpoint}");
            
            Console.WriteLine($"Authorization: Bearer {appOnlyToken}");
            Console.WriteLine($"Content Type: {content.Headers.ContentType}");
            // Add more information as needed

            // Optionally, you can print the content size
            Console.WriteLine($"Content Size: {fileData.Length} bytes");

            
                // Make the POST request to your API endpoint
                HttpResponseMessage response = await client.PutAsync(apiEndpoint, content); // Use PUT instead of POST for file upload


                // Check the response status
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("POST request successful!");
                    // Optionally, you can read the response content
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                }
                else
                {
                    Console.WriteLine($"POST request failed with status code {response.StatusCode}");
                }
            }
        // // Create file stream and upload request content
        // using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        // var uploadRequestContent = new DriveItemUploadContent(fs);

        // // Upload the file
        // var result = await _appClient.Drives["b!aQqF79gi2E6MkqkB5FkyU5_xmBH_OZtNroxpPZPsQcMy8DWKt2W_Q5yuVf_qtAq1"].Items.Request()
        //     .AddAsync(new DriveItem
        //     {
        //         Name = fileName,
        //         File = new File
        //         {
        //             MimeType = "text/plain"
        //         }
        //     }, uploadRequestContent);

        // // Check upload status
        // if (result.IsSuccessStatusCode)
        // {
        //     Console.WriteLine("File uploaded successfully!");
        // }
        // else
        // {
        //     Console.WriteLine($"File upload failed: {result.StatusCode}");
        // }

        }
        catch (Microsoft.Graph.Models.ODataErrors.ODataError me)
        {
            Console.WriteLine(me.Error.Code);
        }
    }
}