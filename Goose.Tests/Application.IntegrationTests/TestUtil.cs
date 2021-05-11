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

        public static async Task<E> Parse<E>(this Task<HttpResponseMessage> message)
        {
            return await (await message).Content.Parse<E>();
        }

        public static async Task<E> Parse<E>(this HttpResponseMessage message)
        {
            return await message.Content.Parse<E>();
        }

        public static async Task<E> Parse<E>(this HttpContent content)
        {
            var json = await content.ReadAsStringAsync();
            return json.ToObject<E>();
        }

        public static E ToObject<E>(this string json)
        {
            return JsonConvert.DeserializeObject<E>(json, new ObjectIdConverter());
        }

        public static E Copy<E>(this E e)
        {
            return e.ToJson().ToObject<E>();
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
            return typeof(ObjectId?).IsAssignableFrom(objectType);
        }
    }
}