using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;

namespace Cactus.Fileserver.ImageResizer.Core.Utils
{
    public static class NameValueCollectionExtensions
    {
        private static NumberStyles floatingPointStyle = NumberStyles.Float | NumberStyles.AllowThousands;

        public static string GetAsString(this NameValueCollection t, string name, string defaultValue)
        {
            string str = t.Get(name);
            if (!string.IsNullOrEmpty(str))
                return str;
            return defaultValue;
        }

        /// <summary>
        /// Provides culture-invariant parsing of int, double, float, bool, and enum values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T? Get<T>(this NameValueCollection t, string name) where T : struct, IConvertible
        {
            return t.Get<T>(name, new T?());
        }

        /// <summary>
        /// Provides culture-invariant parsing of int, double, float, bool, and enum values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T? Get<T>(this NameValueCollection q, string name, T? defaultValue) where T : struct, IConvertible
        {
            return NameValueCollectionExtensions.ParsePrimitive<T>(q[name], defaultValue);
        }

        /// <summary>
        /// Provides culture-invariant parsing of int, double, float, bool, and enum values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T Get<T>(this NameValueCollection q, string name, T defaultValue) where T : struct, IConvertible
        {
            return NameValueCollectionExtensions.ParsePrimitive<T>(q[name], new T?(defaultValue)).Value;
        }

        /// <summary>
        /// Provides culture-invariant parsing of int, double, float, bool, and enum values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T? ParsePrimitive<T>(string value, T? defaultValue) where T : struct, IConvertible
        {
            if (value == null)
                return defaultValue;
            value = value.Trim();
            if (value.Length == 0)
                return defaultValue;
            Type type = typeof(T);
            if (type == typeof(byte))
            {
                byte result = 0;
                if (byte.TryParse(value, NumberStyles.Integer, (IFormatProvider)NumberFormatInfo.InvariantInfo, out result))
                    return (ValueType)result as T?;
            }
            else if (type == typeof(int))
            {
                int result = 0;
                if (int.TryParse(value, NumberStyles.Integer, (IFormatProvider)NumberFormatInfo.InvariantInfo, out result))
                    return (ValueType)result as T?;
            }
            else if (type == typeof(double))
            {
                double result = 0.0;
                if (double.TryParse(value, NameValueCollectionExtensions.floatingPointStyle, (IFormatProvider)NumberFormatInfo.InvariantInfo, out result))
                    return (ValueType)result as T?;
            }
            else if (type == typeof(float))
            {
                float result = 0.0f;
                if (float.TryParse(value, NameValueCollectionExtensions.floatingPointStyle, (IFormatProvider)NumberFormatInfo.InvariantInfo, out result))
                    return (ValueType)result as T?;
            }
            else if (type == typeof(bool))
            {
                string str = value;
                if ("true".Equals(str, StringComparison.OrdinalIgnoreCase) || "1".Equals(str, StringComparison.OrdinalIgnoreCase) || ("yes".Equals(str, StringComparison.OrdinalIgnoreCase) || "on".Equals(str, StringComparison.OrdinalIgnoreCase)))
                    return (ValueType)true as T?;
                if ("false".Equals(str, StringComparison.OrdinalIgnoreCase) || "0".Equals(str, StringComparison.OrdinalIgnoreCase) || ("no".Equals(str, StringComparison.OrdinalIgnoreCase) || "off".Equals(str, StringComparison.OrdinalIgnoreCase)))
                    return (ValueType)false as T?;
            }
            else if (type.IsEnum)
            {
                T? nullable = EnumExtensions.Parse<T>(value);
                if (nullable.HasValue)
                    return nullable;
            }
            else
            {
                T? nullable = value as T?;
                if (nullable.HasValue)
                    return nullable;
            }
            return defaultValue;
        }

        public static string SerializePrimitive<T>(T? val) where T : struct, IConvertible
        {
            if (!val.HasValue)
                return (string)null;
            T obj = val.Value;
            if (typeof(T).IsEnum)
                return ((ValueType)obj as Enum).ToPreferredString(true);
            return Convert.ToString((object)obj, (IFormatProvider)NumberFormatInfo.InvariantInfo).ToLowerInvariant();
        }

        /// <summary>
        /// Serializes the given value by calling .ToString(). If the value is null, the key is removed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q"></param>
        /// <param name="name"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static NameValueCollection SetAsString<T>(this NameValueCollection q, string name, T val) where T : class
        {
            if ((object)val == null)
                q.Remove(name);
            else
                q[name] = val.ToString();
            return q;
        }

        /// <summary>
        /// Provides culture-invariant serialization of value types, in lower case for querystring readability. Setting a key to null removes it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q"></param>
        /// <param name="name"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static NameValueCollection Set<T>(this NameValueCollection q, string name, T? val) where T : struct, IConvertible
        {
            if (!val.HasValue)
                q.Remove(name);
            else
                q[name] = NameValueCollectionExtensions.SerializePrimitive<T>(val);
            return q;
        }

        public static T[] GetList<T>(this NameValueCollection q, string name, T? fallbackValue, params int[] allowedSizes) where T : struct, IConvertible
        {
            return NameValueCollectionExtensions.ParseList<T>(q[name], fallbackValue, allowedSizes);
        }

        /// <summary>
        /// Parses a comma-delimited list of primitive values. If there are unparsable items in the list, they will be replaced with 'fallbackValue'. If fallbackValue is null, the function will return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <param name="fallbackValue"></param>
        /// <param name="allowedSizes"></param>
        /// <returns></returns>
        public static T[] ParseList<T>(string text, T? fallbackValue, params int[] allowedSizes) where T : struct, IConvertible
        {
            if (text == null)
                return (T[])null;
            text = text.Trim(' ', '(', ')', ',');
            if (text.Length == 0)
                return (T[])null;
            string[] strArray = text.Split(new char[1] { ',' }, StringSplitOptions.None);
            bool flag = allowedSizes.Length == 0;
            foreach (int allowedSize in allowedSizes)
            {
                if (allowedSize == strArray.Length)
                    flag = true;
            }
            if (!flag)
                return (T[])null;
            T[] objArray = new T[strArray.Length];
            for (int index = 0; index < strArray.Length; ++index)
            {
                T? primitive = NameValueCollectionExtensions.ParsePrimitive<T>(strArray[index], fallbackValue);
                if (!primitive.HasValue)
                    return (T[])null;
                objArray[index] = primitive.Value;
            }
            return objArray;
        }

        private static string JoinPrimitives<T>(T[] array, char delimiter) where T : struct, IConvertible
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < array.Length; ++index)
            {
                stringBuilder.Append(NameValueCollectionExtensions.SerializePrimitive<T>(new T?(array[index])));
                if (index < array.Length - 1)
                    stringBuilder.Append(delimiter);
            }
            return stringBuilder.ToString();
        }

        public static NameValueCollection SetList<T>(this NameValueCollection q, string name, T[] values, bool throwExceptions, params int[] allowedSizes) where T : struct, IConvertible
        {
            if (values == null)
            {
                q.Remove(name);
                return q;
            }
            bool flag = allowedSizes.Length == 0;
            foreach (int allowedSize in allowedSizes)
            {
                if (allowedSize == values.Length)
                    flag = true;
            }
            if (!flag)
            {
                if (throwExceptions)
                    throw new ArgumentOutOfRangeException(nameof(values), "The specified array is not a valid length. Valid lengths are " + NameValueCollectionExtensions.JoinPrimitives<int>(allowedSizes, ','));
                return q;
            }
            q[name] = NameValueCollectionExtensions.JoinPrimitives<T>(values, ',');
            return q;
        }

        /// <summary>
        /// Returns true if any of the specified keys contain a value
        /// </summary>
        /// <param name="q"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static bool IsOneSpecified(this NameValueCollection q, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (!string.IsNullOrEmpty(q[key]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Normalizes a command that has two possible names.
        /// If either of the commands has a null or empty value, those keys are removed.
        /// If both the primary and secondary are present, the secondary is removed.
        /// Otherwise, the secondary is renamed to the primary name.
        /// </summary>
        /// <param name="q"></param>
        /// <param name="primary"></param>
        /// <param name="secondary"></param>
        public static NameValueCollection Normalize(this NameValueCollection q, string primary, string secondary)
        {
            if (string.IsNullOrEmpty(q[primary]))
                q.Remove(primary);
            if (string.IsNullOrEmpty(q[secondary]))
                q.Remove(secondary);
            if (q[secondary] == null)
                return q;
            if (q[primary] == null)
                q[primary] = q[secondary];
            q.Remove(secondary);
            return q;
        }

        /// <summary>
        /// Creates and returns a new NameValueCollection instance that contains only the specified keys from the current collection.
        /// </summary>
        /// <param name="q"></param>
        /// <param name="keysToKeep"></param>
        /// <returns></returns>
        public static NameValueCollection Keep(this NameValueCollection q, params string[] keysToKeep)
        {
            NameValueCollection nameValueCollection = new NameValueCollection();
            foreach (string index in keysToKeep)
            {
                if (q[index] != null)
                    nameValueCollection[index] = q[index];
            }
            return nameValueCollection;
        }

        public static NameValueCollection Exclude(this NameValueCollection q, params string[] keysToRemove)
        {
            NameValueCollection nameValueCollection = new NameValueCollection(q);
            foreach (string name in keysToRemove)
                nameValueCollection.Remove(name);
            return nameValueCollection;
        }

        /// <summary>
        /// Creates and returns a new NameValueCollection instance that contains all of the
        /// keys/values from 'q', and any keys/values from 'defaults' that 'q' does not already
        /// contain.
        /// </summary>
        /// <param name="q">The settings specific to a particular query</param>
        /// <param name="defaults">Default settings to use when not overridden by 'q'.</param>
        /// <returns></returns>
        public static NameValueCollection MergeDefaults(this NameValueCollection q, NameValueCollection defaults)
        {
            NameValueCollection nameValueCollection = new NameValueCollection(defaults);
            foreach (string allKey in q.AllKeys)
                nameValueCollection[allKey] = q[allKey];
            return nameValueCollection;
        }
    }
}