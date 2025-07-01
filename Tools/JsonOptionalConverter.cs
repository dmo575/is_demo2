using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Update;

// runs whenever a JSON token gets paired with an Optional<T> during deserialization
// and whenever an Optional<T> needs to be serialized iknto JSON
public class JsonOptionalConverter<T> : JsonConverter<Optional<T>>
{
    public override Optional<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {

        // If the JSON token is explicitly set to NULL
        if (reader.TokenType == JsonTokenType.Null)
        {
            // then for its corresponsing property value which is an Optional<T>, return
            // an Optional<T> whos Value is set to null
            return new Optional<T> { value = default };
        }

        // deserialize the token's value to T
        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        // assign that value to the Optional that we will return here
        return new Optional<T> { value = value };
    }

    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        // when writing the JSON, the token's value should hold the Value of the
        // Optional (value.Value), not the Optional
        JsonSerializer.Serialize(writer, value.value, options);
    }
}