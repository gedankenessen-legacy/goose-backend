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
        public static void Replace<T>(this IList<T> list, Predicate<T> oldSelector, T newItem)
        {
            int indexOfOldItem = list.FindIndex(oldSelector);

            if (indexOfOldItem <= -1)
                throw new HttpStatusException(StatusCodes.Status500InternalServerError, "Error in replacing item in list, maybe item not found.");

            list[indexOfOldItem] = newItem;
        }

        /// <summary>
        /// List extension which allows to replace an item of a list with the provided selector.
        /// If the item is not found, it is added to the list instead
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="oldSelector">The selector to distinguish the item that will be replaced.</param>
        /// <param name="newItem">The item that will be take place on the place of the old one.</param>
        public static void ReplaceOrInsert<T>(this IList<T> list, Predicate<T> oldSelector, T newItem)
        {
            int indexOfOldItem = list.FindIndex(oldSelector);

            if (indexOfOldItem >= 0)
            {
                // ELement existiert bereits
                list[indexOfOldItem] = newItem;
            }
            else
            {
                // ELement existiert noch nicht
                list.Add(newItem);
            }
        }

        /// <summary>
        /// List extension which allows to delete the first item that matches the selector.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="selector">The selector to distinguish the item that will be deleted.</param>
        /// <returns>true, if an object was deleted</returns>
        public static bool Remove<T>(this IList<T> list, Predicate<T> selector)
        {
            var index = list.FindIndex(selector);
            if (index <= 0)
            {
                // No element found
                return false;
            }

            list.RemoveAt(index);
            return true;
        }

        // FindIndex gibt es nur für List<T>, was manchmal lästig ist. Dies hier funktioniert auch für IList<T>.
        private static int FindIndex<T>(this IList<T> list, Predicate<T> match)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (match(list[i]))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
