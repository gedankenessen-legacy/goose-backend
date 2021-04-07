using Goose.API.Utils.Exceptions;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace Goose.API.Utils
{
    public static class StringToObjectIdExtension
    {
        public static ObjectId ToObjectId(this string id)
        {
            if (ObjectId.TryParse(id, out var newId) is false)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, $"[{id}] is not a valid object id.");
            return newId;
        }
    }
}