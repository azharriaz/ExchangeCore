using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExchangeCore.Domain.Common;

public class RatesConverter : JsonConverter<Dictionary<DateOnly, Dictionary<string, double>>>
{
    public override Dictionary<DateOnly, Dictionary<string, double>> ReadJson(JsonReader reader, Type objectType, Dictionary<DateOnly, Dictionary<string, double>> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var rates = new Dictionary<DateOnly, Dictionary<string, double>>();

        var jObject = JObject.Load(reader);
        foreach (var property in jObject.Properties())
        {
            var date = DateOnly.ParseExact(property.Name, Constants.DATEONLY_FORMAT);
            var rateDetails = property.Value.ToObject<Dictionary<string, double>>();
            rates[date] = rateDetails;
        }

        return rates;
    }

    public override void WriteJson(JsonWriter writer, Dictionary<DateOnly, Dictionary<string, double>> value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key.ToString(Constants.DATEONLY_FORMAT));
            serializer.Serialize(writer, kvp.Value);
        }
        writer.WriteEndObject();
    }
}
