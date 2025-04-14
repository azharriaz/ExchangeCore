using System.Text.Json;

namespace ExchangeCore.Tests.Common;

public class JsonHelper
{
    public static async Task<T> GetRequestModelAsync<T>(string filePath, ActionTypeEnum type)
    {
        T requestModel = default;
        string json = await File.ReadAllTextAsync(filePath);
        JsonDocument doc = JsonDocument.Parse(json);

        // Get the root element
        JsonElement root = doc.RootElement;

        // Check if the root element contains the property you're interested in
        if (root.TryGetProperty(type.ToString(), out JsonElement jsonElement))
        {
            var result = jsonElement.GetRawText();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            requestModel = JsonSerializer.Deserialize<T>(result, options);
        }

        return requestModel;
    }  
}

