﻿using MongoDB.Bson;
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

            var id = reader.GetString();

            // Wir sollten hier liberaler als Validate sein, und auch den leeren String akzeptieren,
            // damit bei Post nicht unnötig eine id mitgeschickt werden muss.
            if (id == string.Empty)
            {
                return new ObjectId();
            }
       
            return Validate(id);
        }

        public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }

        /// <summary>
        /// The general purpose method to convert a string to an ObjectId.
        /// It will check if the string is a valid ObjectId and then return the ObjectId.
        /// If the string is invalid, it will throw an Exception.
        /// </summary>
        /// <param name="id">The string representaion of an ObjectId. 24 hex chars.</param>
        /// <returns>The ObjectId object represented by the string</returns>
        public static ObjectId Validate(string id)
        {
            if (ObjectId.TryParse(id, out ObjectId objectId))
            {
                return objectId;
            }
            else
            {
                // TODO Hier kommt nach dem Merge ne HttpStatusException hin, ich bin nur zu faul für ein rebase
                throw new Exception("Cannot parse issue string id to a valid object id.");
            }
        }
    }
}
