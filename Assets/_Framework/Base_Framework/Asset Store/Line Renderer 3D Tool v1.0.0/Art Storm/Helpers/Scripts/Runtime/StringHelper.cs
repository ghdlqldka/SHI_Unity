using System.Globalization;

namespace EA
{
    /// <summary>
    /// String extension methods
    /// </summary>
    public static class StringHelper
    {
        public static bool IsNullOrEmpty(this string str) => str == null || str.Length == 0;

        public static int FindAll(this string str, char c)
        {
            int count = 0;

            if (string.IsNullOrEmpty(str)) return 0;

            for (int a = 0; a < str.Length; a++)
                if (str[a] == c) count++;
            return count;
        }

        public static bool IsInt(this string val) => int.TryParse(val, out _);

        public static bool IsBool(this string val) => bool.TryParse(val, out _);

        public static int AsInt(this string str) => int.Parse(str);

        public static float AsFloat(this string str) => float.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);

        public static bool AsBool(this string str) => bool.Parse(str);

        public static int FindAllParamSlots(this string str, int max = 8)
        {
            if (str.Length < 3) return 0;
            int count = 0;
            for (int a = 0; a < max; a++)
            {
                string look = "{" + a + "}";
                if (str.IndexOf(look) != -1) count++;
                else return count;
            }
            return count;
        }
    }
}