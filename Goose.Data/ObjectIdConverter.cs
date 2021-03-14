using MongoDB.Bson;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JsonTokenType = System.Text.Json.JsonTokenType;

namespace Goose.Data
{
    public class ObjectIdConverter : JsonConverter<ObjectId>
    {
        public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new Exception($"Unexpected token parsing ObjectId. Expected String, got: {reader.TokenType}.");

            var value = reader.GetString();
            if (ObjectId.TryParse(value, out ObjectId objectId))
            {
                return objectId;
            }
            else
            {
                // Error werfen wäre auch möglich.
                return new ObjectId();
            }
        }

        public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
