using System;

namespace EA
{
    public static class Mathg
    {
        public static int Loop(this int value, int length)
        {
            if (length <= 0)
                throw new Exception($"Mathg.Loop({value}, {length}) length can't be equal or below zero!");

            if (value < 0)
            {
                int mod = value % length;
                return mod == 0 ? 0 : (length + mod);
            }
            else
            {
                return value % length;
            }
        }
    }
}