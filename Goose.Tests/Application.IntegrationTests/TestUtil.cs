using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Goose.API.Utils;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace Goose.Tests.Application.IntegrationTests
{
    public static class TestUtil
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, new ObjectIdConverter());
        }
        public static StringContent ToStringContent(this object obj)
        {
            var content = new StringContent(obj.ToJson(), Encoding.UTF8, "application/json");
            return content;
        }

        public static async Task<E> Parse<E>(this HttpContent content)
        {
            var json = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<E>(json, new ObjectIdConverter());
        }
    }

    public class ObjectIdConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            return reader.Value.ToString().ToObjectId();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ObjectId).IsAssignableFrom(objectType);
        }
    }
}