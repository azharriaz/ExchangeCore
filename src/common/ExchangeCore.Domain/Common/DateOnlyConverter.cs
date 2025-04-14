using ExchangeCore.Domain.Common;

using System;

using Newtonsoft.Json;

public class DateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.String)
        {
            throw new JsonException($"Unexpected token parsing DateOnly. Expected String, got {reader.TokenType}.");
        }

        var dateString = reader.Value.ToString();
        return DateOnly.ParseExact(dateString, Constants.DATEONLY_FORMAT);
    }

    public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString(Constants.DATEONLY_FORMAT));
    }
}

