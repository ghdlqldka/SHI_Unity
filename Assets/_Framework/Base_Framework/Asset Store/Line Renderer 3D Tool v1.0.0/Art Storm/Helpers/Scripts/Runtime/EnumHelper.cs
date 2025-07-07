using System;

namespace EA
{
    /// <summary>
    /// Enum extension methods
    /// </summary>
    public static class EnumHelper
    {
        public static Enum Add(Enum x, Enum y)
        {
            int ix = Convert.ToInt32(x);
            int iy = Convert.ToInt32(x);
            int rez = ix | iy;
            return (Enum)Enum.ToObject(y.GetType(), rez);
        }

        public static T Substract<T>(Enum x, Enum y) where T : Enum
        {
            int ix = Convert.ToInt32(x);
            int iy = Convert.ToInt32(x);
            int rez = ix & ~iy;
            return (T)Enum.ToObject(typeof(T), rez);
        }
    }
}