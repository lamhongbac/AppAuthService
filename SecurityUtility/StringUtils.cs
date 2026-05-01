using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSA.Utils
{
    public static class Extention
    {
        public static string ToShareCode(this string text)
        {
            string result = string.Empty;

            int salt = 97;
            foreach (char c in text)
            {
                result += ((char)(int.Parse(c.ToString()) + salt)).ToString();
            }

            return result.ToUpper();
        }
        public static object GetPropValue(this object src, string propName)
        {

            return src.GetType().GetProperty(propName).GetValue(src, null);
        }
        public static string AddLeftString(this string src, int len, char addChar)
        {
            return src.ToString().PadLeft(len, addChar);



        }
        public static string AddRightString(this string src, int len, char addChar)
        {
            return src.ToString().PadRight(len, addChar);
        }

        public static int TryGetInt(this string value)
        {
            int result = 0;
            try
            {
                result = int.Parse(value);
            }
            catch
            {

            }

            return result;
        }

        public static long TryGetInt64(this string value)
        {
            long result = 0;
            try
            {
                result = long.Parse(value);
            }
            catch
            {

            }

            return result;
        }

        public static double TryGetDouble(this string value)
        {
            double result = 0;
            try
            {
                result = double.Parse(value);
            }
            catch
            {

            }

            return result;
        }

        public static T? GetContentAs<T>(this MSA.Shared.BOProcessResult result)
        {
            if (result?.Content == null) return default;
            if (result.Content is T direct) return direct;

            if (result.Content is System.Text.Json.JsonElement elem)
            {
                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<T>(elem.GetRawText(), new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    return default;
                }
            }

            try
            {
                string jsonText = result.Content.ToString() ?? "{}";
                return System.Text.Json.JsonSerializer.Deserialize<T>(jsonText, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return default;
            }
        }
    }
}

