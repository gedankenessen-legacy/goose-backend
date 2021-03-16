using MongoDB.Bson;

namespace System
{
    public static class StringToObjectIdExtension
    {
        public static ObjectId ToObjectId(this string id)
        {
            if (ObjectId.TryParse(id, out var newId) is false)
                throw new Exception("Cannot parse issue string id to a valid object id.");
            //  new HttpStatusException(StatusCodes.Status400BadRequest, "Cannot parse issue string id to a valid object id.");
            return newId;
        }
    }
}