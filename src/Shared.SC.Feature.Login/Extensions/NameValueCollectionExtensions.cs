using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Shared.SC.Feature.Login.Extensions
{
    public static class NameValueCollectionExtentions
    {
        public static string ToQueryString(this NameValueCollection nvc)
        {
            if (nvc == null)
            {
                throw new ArgumentNullException(nameof(nvc));
            }

            IEnumerable<string> qs = from key in nvc.AllKeys
                                     let values = nvc.GetValues(key)
                                     where values != null
                                     select
                                     $"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(string.Join(",", values))}";

            return string.Join("&", qs);
        }
    }
}