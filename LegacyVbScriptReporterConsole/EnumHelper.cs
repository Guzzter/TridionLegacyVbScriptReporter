using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegacyVbScriptReporterConsole
{
    public static class EnumHelper<T>
    {
        public static T Parse(string value)
        {
            return EnumHelper<T>.Parse(value, true);
        }

        public static T Parse(string value, bool ignoreCase)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }

        public static bool TryParse(string value, out T returnedValue)
        {
            return EnumHelper<T>.TryParse(value, true, out returnedValue);
        }

        public static bool TryParse(string value, bool ignoreCase, out T returnedValue)
        {
            try
            {
                returnedValue = (T)Enum.Parse(typeof(T), value, ignoreCase);
                return true;
            }
            catch
            {
                returnedValue = default(T);
                return false;
            }
        }
    }
}
