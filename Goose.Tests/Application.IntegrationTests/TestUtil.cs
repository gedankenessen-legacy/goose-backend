using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Goose.API.Utils;
using MongoDB.Bson;
using Newtonsoft.Json;
using NUnit.Framework;

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

        public static async Task<E> Parse<E>(this HttpResponseMessage message)
        {
            return await message.Content.Parse<E>();
        }

        public static async Task<E> Parse<E>(this HttpContent content)
        {
            var value = await content.ReadAsStringAsync();
            return value.Parse<E>();
        }

        public static E Parse<E>(this string json)
        {
            return JsonConvert.DeserializeObject<E>(json, new ObjectIdConverter());
        }

        public static E DeepClone<E>(this E obj)
        {
            return obj.ToJson().Parse<E>();
        }

        public static void AssertEqualsJson(this object expected, object actual)
        {
            var expectedJson = expected.ToJson();
            var actualJson = actual.ToJson();
            Assert.AreEqual(expectedJson, actual.ToJson(), $"[{expectedJson}]\n does not equal [{actualJson}]");
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