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
        /// <summary>
        /// The general purpose method to convert a string to an ObjectId.
        /// It will check if the string is a valid ObjectId and then return the ObjectId.
        /// If the string is invalid, it will throw an Exception.
        /// </summary>
        /// <param name="id">The string representaion of an ObjectId. 24 hex chars.</param>
        /// <param name="errorMsg">The error message that will be used if the id is invalid</param>
        /// <returns>The ObjectId object represented by the string</returns>
        public static ObjectId ValidateObjectId(string id, string errorMsg = "Invalid ObjectId")
        {
            if (ObjectId.TryParse(id, out ObjectId result))
                 return result;

            throw new HttpStatusException(StatusCodes.Status400BadRequest, errorMsg);
        }
    }
}
