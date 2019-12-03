using System;
using Day3.Model;
using Microsoft.AspNetCore.Http;

namespace Day3.Extensions
{
    public static class HeaderDictionaryExtension
    {
        public static T GetEnum<T>(this IHeaderDictionary @this, string name)
        {
            if (@this.TryGetValue(name, out var value) == false)
            {
                return default;
            }

            if (Enum.TryParse(typeof(T), value, true, out var @event) == false)
            {
                return default;
            }

            return (T)@event;
        }
    }
}