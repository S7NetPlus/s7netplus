using System;
using System.Collections.Generic;
using System.Linq;

namespace Sample.UWP
{
    public static class EnumHelper
    {
        public static List<TEnum> GetValues<TEnum>() where TEnum : struct, IConvertible
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
        }
    }
}