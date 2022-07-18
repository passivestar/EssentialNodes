using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace EssentialNodes
{
    internal static class EnumerableHelpers
    {
        /// <summary>
        /// Converts an IEnumerable to an IList, without copying when possible
        /// </summary>
        /// <param name="enumerable">The IEnumerable to convert or copy</param>
        /// <returns>
        /// Either the input if it already implements IList, or a copy in a new IList
        /// </returns>
        /// <remarks>
        /// Avoids the copy-everything-at-least-three-times pattern the built in UVS ConversionUtility method does,
        /// but doesn't do the type compatibility check since there's no generic types
        /// </remarks>
        public static IList ToIList(this IEnumerable enumerable)
        {
            if (enumerable is IList list)
                return list;

            if (enumerable is ICollection coll)
            {
                var arr = new object[coll.Count];
                coll.CopyTo(arr, 0);
                return arr;
            }

            // We could do a more efficient version of this if needed
            return new List<object>(enumerable.Cast<object>());
        }
    }
}
