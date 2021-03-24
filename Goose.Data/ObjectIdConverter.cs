using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JsonTokenType = System.Text.Json.JsonTokenType;

/// <summary>
/// In dieser Datei werden einige Hilfklassen bereitgestellt, die es erlauben
/// ObjectIds in den API-Schnittstellen zu verwenden.
/// </summary>
namespace Goose.Data
{
    /// <summary>
    /// ObjectIdJsonConverter kümmert sich um die Json-Repräsentation von
    /// ObjectIds in den DTOs. Er wird sowohl bei eingehenden und ausgehenden Nachrichten
    /// verwendet.
    /// </summary>
    public class ObjectIdJsonConverter : JsonConverter<ObjectId>
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
       
            if (ObjectId.TryParse(id, out ObjectId objectId))
            {
                return objectId;
            }
            else
            {
                throw new Exception("Cannot parse ObjectId");
            }
        }

        public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    /// <summary>
    /// ObjectIdBinder kann ObjectIds in den Pfaden auslesen. Ist die ObjectId ungültig,
    /// wird ein Validation Error geworfen und der Handler des Controllers wird gar nicht ausgeführt.
    /// </summary>
    public class ObjectIdBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var result = bindingContext.ValueProvider.GetValue(bindingContext.FieldName);
            if (ObjectId.TryParse(result.FirstValue, out ObjectId objectId))
            {
                bindingContext.Result = ModelBindingResult.Success(objectId);
            }
            else
            {
                // Ungültige ObjectId
                bindingContext.Result = ModelBindingResult.Failed();
                bindingContext.ModelState.AddModelError(bindingContext.FieldName, "Not a valid objectId");
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// ObjectIdBinderProvider registriert den ObjectIdBinder.
    /// </summary>
    public class ObjectIdBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(ObjectId))
            {
                return new ObjectIdBinder();
            }

            // kein binder gefunder
            return null;
        }
    }

    /// <summary>
    /// ObjectIdTypeConverter existiert nur, damit Swagger erkennt das man ObjectIds nun wie Strings
    /// behandeln kann - leider gibt es bei Routenparametern keine andere Möglichkeit.
    /// Die Methoden der Klasse selbst werden nie aufgerufen.
    /// </summary>
    public class ObjectIdTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return new ObjectId(value.ToString());
        }
    }
}
