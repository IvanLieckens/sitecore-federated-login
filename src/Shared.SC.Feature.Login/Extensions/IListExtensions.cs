using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Shared.SC.Feature.Login.Extensions
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "IList is the interface we're extending")]
    public static class IListExtensions
    {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            List<T> thisList = list as List<T>;
            if (thisList != null)
            {
                thisList.AddRange(items);
            }
            else
            {
                foreach (T item in items)
                {
                    list.Add(item);
                }
            }
        }
    }
}