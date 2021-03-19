using Goose.API.Utils.Exceptions;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Utils.Validators
{
    public static class Validators
    {
        public static ObjectId ValidateObjectId(string id, string errorMsg = "InvalidObjectId")
        {
            if (ObjectId.TryParse(id, out ObjectId result))
                 return result;

            throw new HttpStatusException(StatusCodes.Status400BadRequest, errorMsg);
        }
    }
}
