using Goose.API.Utils.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Goose.API.Utils
{
    public static class ListExtensions
    {
        /// <summary>
        /// List extension which allows to replace an item of a list with the provided selector. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="oldSelector">The selector to distinguish the item that will be replaced.</param>
        /// <param name="newItem">The item that will be take place on the place of the old one.</param>
        public static void Replace<T>(this List<T> list, Predicate<T> oldSelector, T newItem)
        {
            int indexOfOldItem = list.FindIndex(oldSelector);

            if (indexOfOldItem <= -1)
                throw new HttpStatusException(StatusCodes.Status500InternalServerError, "Error in replacing item in list, maybe item not found.");

            list[indexOfOldItem] = newItem;
        }
    }
}
