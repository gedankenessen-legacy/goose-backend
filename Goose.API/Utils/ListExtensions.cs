using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            list[indexOfOldItem] = newItem;
        }
    }
}
