using UnityEngine;

namespace EA
{
    /// <summary>
    /// Bounds extension methods
    /// </summary>
    public static class BoundsHelper
    {
        public static Vector3 CornerUpLeftBack(this Bounds b)
        {
            return b.min + new Vector3(0, b.extents.y, 0);
        }

        public static Vector3 CornerUpRightBack(this Bounds b)
        {
            return b.min + new Vector3(b.size.x, b.extents.y, 0);
        }

        public static Vector3 CornerUpLeftForward(this Bounds b)
        {
            return b.min + new Vector3(0, b.extents.y, b.size.z);
        }

        public static Vector3 CornerUpRightForward(this Bounds b)
        {
            return b.max;
        }

        public static Vector3 CornerDownLeftBack(this Bounds b)
        {
            return b.min;
        }

        public static Vector3 CornerDownRightBack(this Bounds b)
        {
            return b.min + new Vector3(b.size.x, 0, 0);
        }

        public static Vector3 CornerDownLeftForward(this Bounds b)
        {
            return b.min + new Vector3(0, 0, b.size.z);
        }

        public static Vector3 CornerDownRightForward(this Bounds b)
        {
            return b.min + new Vector3(b.size.x, 0, b.size.z);
        }
    }
}