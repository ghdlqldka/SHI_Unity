using UnityEngine;

namespace EA
{
    /// <summary>
    /// Color extension methods
    /// </summary>
    public static class ColorHelper
    {
        public static string ToHex(this Color c)
        {
            Color32 c32 = c;
            return string.Format("#{0}{1}{2}{3}", c32.r.ToString("X2"), c32.g.ToString("X2"), c32.b.ToString("X2"), c32.a.ToString("X2"));
        }

        public static Color ToColor(this string hex)
        {
            hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
            byte a = 255;//assume fully visible unless specified in hex
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            //Only use alpha if the string has enough characters
            if (hex.Length == 8) a = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, a);
        }

        public static Color SetA(this Color c, float a)
        {
            c.a = a;
            return c;
        }


        public static bool TryParseGumboColor(string gumboString, out Color color)
        {
            if (gumboString.IsNullOrEmpty() || !gumboString.Contains("rgba") || gumboString.Length <= 0)
            {
                color = Color.magenta;
                return false;
            }

            int startIndex = 5;
            int substringLength = gumboString.Length - 1 - startIndex;
            string rgbaString = gumboString.Substring(5, substringLength);
            string[] rgbaSplits = rgbaString.Split(',');

            if (rgbaSplits.Length < 3)
            {
                color = Color.magenta;
                return false;
            }

            Color result = new Color();
            result.r = float.Parse(rgbaSplits[0]) / 255f;
            result.g = float.Parse(rgbaSplits[1]) / 255f;
            result.b = float.Parse(rgbaSplits[2]) / 255f;
            result.a = float.Parse(rgbaSplits[3]);
            color = result;
            return true;
        }
    }
}