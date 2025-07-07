using UnityEngine;

namespace EA
{
    /// <summary>
    /// Rect extension methods
    /// </summary>
    public static class RectHelper
    {
        public static Rect AddX(this Rect rect, float x)
        {
            rect.x += x;
            return rect;
        }

        public static Rect AddY(this Rect rect, float y)
        {
            rect.y += y;
            return rect;
        }

        public static Rect SetX(this Rect rect, float x)
        {
            rect.x = x;
            return rect;
        }

        public static Rect SetY(this Rect rect, float y)
        {
            rect.y = y;
            return rect;
        }

        public static Rect SetWidth(this Rect rect, float width)
        {
            rect.width = width;
            return rect;
        }

        public static Rect SetHeight(this Rect rect, float height)
        {
            rect.height = height;
            return rect;
        }
    }
}