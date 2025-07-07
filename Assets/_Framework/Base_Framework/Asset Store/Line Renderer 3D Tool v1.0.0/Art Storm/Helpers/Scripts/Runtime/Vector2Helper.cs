using UnityEngine;

namespace EA
{
    /// <summary>
    /// Extension methods Vector2
    /// </summary>
    public static class Vector2Helper
    {
        public static Vector3[] ToXZArray(this Vector2[] arr)
        {
            Vector3[] v3arr = new Vector3[arr.Length];
            for (int a = 0; a < v3arr.Length; a++)
                v3arr[a] = arr[a].ToVector3XZ();
            return v3arr;
        }

        public static Vector2 Min(this Vector2[] arr)
        {
            Vector2 min = Vector2.one * float.MaxValue;
            for (int a = 0; a < arr.Length; a++)
            {
                if (arr[a].x <= min.x) min.x = arr[a].x;
                if (arr[a].y <= min.y) min.y = arr[a].y;
            }
            return min;
        }

        public static Vector2 Max(this Vector2[] arr)
        {
            Vector2 max = Vector2.one * float.MinValue;
            for (int a = 0; a < arr.Length; a++)
            {
                if (arr[a].x >= max.x) max.x = arr[a].x;
                if (arr[a].y >= max.y) max.y = arr[a].y;
            }
            return max;
        }

        public static Vector2 Vector2FromXZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        public static Vector3 ToVector3XZ(this Vector2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }

        public static int IndexOf(this Vector2[] arr, Vector2 v)
        {
            for (int a = 0; a < arr.Length; a++)
                if (arr[a] == v)
                    return a;
            return -1;
        }

        public static Vector2 SetX(this Vector2 v, float x)
        {
            v.x = x;
            return v;
        }

        public static Vector2 SetY(this Vector2 v, float y)
        {
            v.y = y;
            return v;
        }

        public static Vector2 AddX(this Vector2 v, float x)
        {
            v.x += x;
            return v;
        }

        public static Vector2 AddY(this Vector2 v, float y)
        {
            v.y += y;
            return v;
        }
    }
}