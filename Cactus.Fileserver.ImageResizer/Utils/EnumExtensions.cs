using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Cactus.Fileserver.ImageResizer.Utils
{
    /// <summary>
    /// Extends enumerations by allowing them to define alternate strings with the [EnumString("Alternate Name",true)]  attribute, and support it through TryParse and ToPreferredString
    /// </summary>
    public static class EnumExtensions
    {
        private static Dictionary<Type, Dictionary<string, Enum>> values;
        private static Dictionary<Type, Dictionary<Enum, string>> preferredValues;

        private static void LoadValues(Type t)
        {
            Dictionary<Type, Dictionary<string, Enum>> values = EnumExtensions.values;
            Dictionary<Type, Dictionary<Enum, string>> preferredValues = EnumExtensions.preferredValues;
            Dictionary<Type, Dictionary<string, Enum>> dictionary1 = values != null ? new Dictionary<Type, Dictionary<string, Enum>>((IDictionary<Type, Dictionary<string, Enum>>)values) : new Dictionary<Type, Dictionary<string, Enum>>();
            Dictionary<Type, Dictionary<Enum, string>> dictionary2 = preferredValues != null ? new Dictionary<Type, Dictionary<Enum, string>>((IDictionary<Type, Dictionary<Enum, string>>)preferredValues) : new Dictionary<Type, Dictionary<Enum, string>>();
            FieldInfo[] fields = t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField);
            Dictionary<string, Enum> dictionary3 = new Dictionary<string, Enum>(fields.Length * 2, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
            Dictionary<Enum, string> dictionary4 = new Dictionary<Enum, string>(fields.Length);
            foreach (FieldInfo fieldInfo in fields)
            {
                string name = fieldInfo.Name;
                Enum key = Enum.ToObject(t, fieldInfo.GetRawConstantValue()) as Enum;
                dictionary3[name] = key;
                string str = name;
                if (!dictionary4.ContainsKey(key))
                    dictionary4[key] = str;
            }
            dictionary1[t] = dictionary3;
            dictionary2[t] = dictionary4;
            EnumExtensions.values = dictionary1;
            EnumExtensions.preferredValues = dictionary2;
        }

        private static Dictionary<string, Enum> GetValues(Type t)
        {
            if (EnumExtensions.values == null)
                EnumExtensions.LoadValues(t);
            Dictionary<string, Enum> dictionary = (Dictionary<string, Enum>)null;
            if (!EnumExtensions.values.TryGetValue(t, out dictionary))
                EnumExtensions.LoadValues(t);
            if (!EnumExtensions.values.TryGetValue(t, out dictionary))
                return (Dictionary<string, Enum>)null;
            return dictionary;
        }

        private static Dictionary<Enum, string> GetPreferredStrings(Type t)
        {
            if (EnumExtensions.preferredValues == null)
                EnumExtensions.LoadValues(t);
            Dictionary<Enum, string> dictionary;
            if (!EnumExtensions.preferredValues.TryGetValue(t, out dictionary))
                EnumExtensions.LoadValues(t);
            if (!EnumExtensions.preferredValues.TryGetValue(t, out dictionary))
                return (Dictionary<Enum, string>)null;
            return dictionary;
        }

        /// <summary>
        /// Attempts case-insensitive parsing of the specified enum. Returns the specified default value if parsing fails.
        /// Supports [EnumString("Alternate Value")] attributes and parses flags. If any segment of a comma-delimited list isn't parsed as either a number or string, defaultValue will be returned.
        /// </summary>
        /// <param name="en"></param>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T Parse<T>(this T en, string value, T defaultValue) where T : struct, IConvertible
        {
            T? nullable = en.Parse<T>(value);
            if (nullable.HasValue)
                return nullable.Value;
            return defaultValue;
        }

        /// <summary>
        /// Attempts case-insensitive parsing of the specified enum. Returns null if parsing failed.
        /// Supports [EnumString("Alternate Value")] attributes and parses flags. If any segment of a comma-delimited list isn't parsed as either a number or string, null will be returned.
        /// </summary>
        /// <param name="en"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T? Parse<T>(this T en, string value) where T : struct, IConvertible
        {
            return EnumExtensions.Parse<T>(value);
        }

        public static T? Parse<T>(string value) where T : struct, IConvertible
        {
            if (string.IsNullOrEmpty(value))
                return new T?();
            value = value.Trim();
            if (string.IsNullOrEmpty(value))
                return new T?();
            Type type = typeof(T);
            Dictionary<string, Enum> values = EnumExtensions.GetValues(type);
            long num = 0;
            bool flag = false;
            string str1 = value;
            char[] chArray = new char[1] { ',' };
            foreach (string str2 in str1.Split(chArray))
            {
                string s = str2.Trim();
                if (s.Length != 0)
                {
                    long result;
                    if ((char.IsDigit(s[0]) || (int)s[0] == 45 || (int)s[0] == 43) && long.TryParse(s, NumberStyles.Integer, (IFormatProvider)NumberFormatInfo.InvariantInfo, out result))
                    {
                        num |= result;
                        flag = true;
                    }
                    else
                    {
                        Enum @enum;
                        if (!values.TryGetValue(value, out @enum))
                            return new T?();
                        num |= Convert.ToInt64((object)@enum);
                        flag = true;
                    }
                }
            }
            if (!flag)
                return new T?();
            return Enum.ToObject(type, num) as T?;
        }

        /// <summary>
        /// Retuns the string representation for the given enumeration
        /// </summary>
        /// <param name="en"></param>
        /// <param name="lowerCase"></param>
        /// <returns></returns>
        public static string ToPreferredString(this Enum en, bool lowerCase)
        {
            Type type = en.GetType();
            bool flag = false;
            Dictionary<Enum, string> preferredStrings = EnumExtensions.GetPreferredStrings(type);
            if (flag)
                return (string)null;
            string str;
            if (!preferredStrings.TryGetValue(en, out str))
                str = en.ToString();
            if (!lowerCase)
                return str;
            return str.ToLowerInvariant();
        }
    }
}