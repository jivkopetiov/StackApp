using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Globalization;

namespace Stacklash.iOS
{
    public static class BclEx
    {
        public static string RemoveHtmlTags(string text) {
            if (string.IsNullOrEmpty (text))
                return text;

            return Regex.Replace(text, @"<[^>]*>", "");
        }

		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> list) {
			if (list == null)
				return Enumerable.Empty <T> ();
			else 
				return list;
		} 

		public static string GetExtension(this Uri url)
		{
			string urlString = url.AbsoluteUri;
			
			int lastIndexOfDot = urlString.LastIndexOf (".", StringComparison.OrdinalIgnoreCase);
			if (lastIndexOfDot == -1)
				return null;
			
			if (urlString.EndsWithOrdinalNoCase("."))
				return null;
			
			string extension = urlString.Substring(lastIndexOfDot + 1);
			
			if (extension.Contains("/"))
				return null;
			else
				return extension;
		}
		
		public static string GetFileName(this Uri url)
		{
			int lastIndexOfSlash = url.AbsoluteUri.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
			if (lastIndexOfSlash == -1)
				return null;
			
			string filename = url.AbsoluteUri.Substring(lastIndexOfSlash + 1);
			
			if (filename.Contains("."))
				return filename;
			else
				return null;
		}

        public static string Fmt(this string target, params object[] parameters)
        {
            return string.Format(CultureInfo.InvariantCulture, target, parameters);
        }

        public static string JoinStrings<T>(this IEnumerable<T> source,
                                                Func<T, string> projection, string separator)
        {
            if (source == null)
                return null;

            var builder = new StringBuilder();
            bool first = true;
            foreach (T element in source)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(separator);
                }
                builder.Append(projection(element));
            }
            return builder.ToString();
        }

        public static string JoinStrings<T>(this IEnumerable<T> source, string separator)
        {
            return JoinStrings(source, t => t.ToString(), separator);
        }

        public static bool IsNullOrEmpty(this string argument)
        {
            return string.IsNullOrEmpty(argument);
        }

        public static bool IsNotNullOrEmpty(this string argument)
        {
            return !string.IsNullOrEmpty(argument);
        }

        public static bool IsWhitespace(this string argument)
        {
            return Regex.IsMatch(argument, @"^(\s)*$");
        }

        public static string[] GetWords(this string camelCaseWord)
        {
            return Regex.Split(camelCaseWord, @"\W+");
        }

        public static string GetFileName(this string path)
        {
            int lastIndexOfSlash = path.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
            if (lastIndexOfSlash == -1)
                return null;

            string filename = path.Substring(lastIndexOfSlash + 1);

            if (filename.Contains("."))
                return filename;
            else
                return null;
        }

        public static string CollapseWhitespace(this string self)
        {
            return Regex.Replace(self, @"\s+", " ");
        }

		public static string NormalizeHtmlText(string text) {
			if (string.IsNullOrEmpty (text))
				return text;

			text = Regex.Replace(text, @"<[^>]*>", "");
			text = System.Web.HttpUtility.HtmlDecode (text);
			text = text.Trim ();

			return text;
		}

        public static string EmptyIfNull(this string self)
        {
            if (self == null)
                return "";
            else
                return self;
        }

        public static bool StartsWithOrdinal(this string self, string value)
        {
            return self.StartsWith(value, StringComparison.Ordinal);
        }

        public static bool StartsWithOrdinalNoCase(this string self, string value)
        {
            return self.StartsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EndsWithOrdinal(this string self, string value)
        {
            return self.EndsWith(value, StringComparison.Ordinal);
        }

        public static bool EndsWithOrdinalNoCase(this string self, string value)
        {
            return self.EndsWith(value, StringComparison.OrdinalIgnoreCase);
        }

		public static bool EqualsOrdinalNoCase(this string original, string value)
		{
			if (original == null)
				return value == null;

			return original.Equals(value, StringComparison.OrdinalIgnoreCase);
		}

		public static bool Contains(this string original,
                                    string value,
                                    StringComparison comparisonType)
        {
            return original.IndexOf(value, comparisonType) >= 0;
        }

        public static bool ContainsOrdinal(this string original, string value)
        {
            return original.IndexOf(value, StringComparison.Ordinal) >= 0;
        }

        public static bool ContainsOrdinalNoCase(this string original, string value)
        {
			if (original == null)
				return false;
			
            return original.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

		public static bool ContainsEnumeration(this Enum enumeration, string value)
		{
			var enumType = enumeration.GetType();

			if (Enum.IsDefined(enumType, value))
			{
				var intEnumValue = Convert.ToInt32(enumeration, CultureInfo.InvariantCulture);
				var intFlagValue = (int)Enum.Parse(enumType, value);

				return (intFlagValue & intEnumValue) == intFlagValue;
			}
			else
			{
				return false;
			}
		}

		public static bool ContainsEnumeration(this Enum enumeration, Enum value)
		{
			if (Enum.IsDefined(enumeration.GetType(), value))
			{
                int number = Convert.ToInt32(value, CultureInfo.InvariantCulture);

                return (number & Convert.ToInt32(enumeration, CultureInfo.InvariantCulture)) == number;
			}
			else
			{
				return false;
			}
		}

		public static TEnum ToEnum<TEnum>(this string enumValue, TEnum defaultValue)
		{
			if (!Enum.IsDefined(typeof(TEnum), enumValue))
			{
				return defaultValue;
			}

			return (TEnum)Enum.Parse(typeof(TEnum), enumValue);
		}

		public static TEnum ToEnum<TEnum>(this string enumValue)
		{
			if (!Enum.IsDefined(typeof(TEnum), enumValue))
			{
				throw new InvalidOperationException(
					"The enum {0} does not contain the value {1}".Fmt(typeof(TEnum).Name, enumValue));
			}

			return (TEnum)Enum.Parse(typeof(TEnum), enumValue);
		}

		public static TEnum ToEnumIgnoreCase<TEnum>(this string enumValue)
		{
			try
			{
				return (TEnum)Enum.Parse(typeof(TEnum), enumValue, true);
			}
			catch (ArgumentException)
			{
				throw new InvalidOperationException(
					"The enum {0} does not contain the value {1}".Fmt(typeof(TEnum).Name, enumValue));
			}
		}

		public static string OffsetFromNow(this DateTime date) {
			var span = DateTime.Now.Subtract (date);
			return span.ToShortReadableOffsetString ();
		}

        private static readonly DateTime _unixEpoch = new DateTime(1970,1,1,0,0,0,0);

        public static string OffsetFromNow(int unixTime) {
            var date = _unixEpoch.AddSeconds( unixTime ).ToLocalTime();
            return date.OffsetFromNow();
        }

        public static DateTime UnixToDate(int unixTime) {
            return _unixEpoch.AddSeconds(unixTime);
        }

		public static string ReadableDuration(this TimeSpan span) {
			if (span.Days > 365)
			{
				if (span.Days < 730)
				{
					return "1 year";
				}
				else
				{
                    return "{0} years".Fmt((span.Days / 365).ToString(CultureInfo.InvariantCulture));
				}
			}
			else if (span.Days > 60)
			{
                return "{0} months".Fmt((span.Days / 30).ToString(CultureInfo.InvariantCulture));
			}
			else if (span.Days > 0)
			{
				if (span.Days == 1)
				{
                    return span.Hours.ToString(CultureInfo.InvariantCulture) + " hours";
				}
				else
				{
                    return span.Days.ToString(CultureInfo.InvariantCulture) + " days";
				}
			}
			else if (span.Hours > 0)
			{
				if (span.Hours == 1)
				{
                    return span.Minutes.ToString(CultureInfo.InvariantCulture) + " minutes";
				}
				else
				{
                    return span.Hours.ToString(CultureInfo.InvariantCulture) + " hours";
				}
			}
			else if (span.Minutes > 0)
			{
				if (span.Minutes == 1)
				{
                    return span.Seconds.ToString (CultureInfo.InvariantCulture) + " seconds";
				}
				else
				{
                    return span.Minutes.ToString(CultureInfo.InvariantCulture) + " minutes " + span.Seconds.ToString (CultureInfo.InvariantCulture) + " seconds";
				}
			}
			else
			{
                return span.Seconds.ToString (CultureInfo.InvariantCulture) + " seconds";
			}
		}

		public static string ToShortReadableOffsetString(this TimeSpan span)
		{
			if (span.Days > 365)
			{
				if (span.Days < 730)
				{
					return "1 year ago";
				}
				else
				{
                    return "{0} years ago".Fmt((span.Days / 365).ToString(CultureInfo.InvariantCulture));
				}
			}
			else if (span.Days > 60)
			{
                return "{0} months ago".Fmt((span.Days / 30).ToString(CultureInfo.InvariantCulture));
			}
			else if (span.Days > 0)
			{
				if (span.Days == 1)
				{
					return "1 day ago";
				}
				else
				{
                    return span.Days.ToString(CultureInfo.InvariantCulture) + " days ago";
				}
			}
			else if (span.Hours > 0)
			{
				if (span.Hours == 1)
				{
					return "1 hour ago";
				}
				else
				{
                    return span.Hours.ToString(CultureInfo.InvariantCulture) + " hours ago";
				}
			}
			else if (span.Minutes > 0)
			{
				if (span.Minutes == 1)
				{
					return "1 minute ago";
				}
				else
				{
                    return span.Minutes.ToString(CultureInfo.InvariantCulture) + " minutes ago";
				}
			}
			else
			{
                return span.Seconds.ToString (CultureInfo.InvariantCulture) + " seconds ago";
			}
		}
    }
}