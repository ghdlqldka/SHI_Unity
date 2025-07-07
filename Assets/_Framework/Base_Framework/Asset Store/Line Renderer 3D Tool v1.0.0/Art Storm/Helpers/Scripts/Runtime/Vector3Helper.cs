using UnityEngine;

namespace EA
{
    /// <summary>
    /// Extension methods Vector3
    /// </summary>
    public static class Vector3Helper
    {
        public static float Distance(this Vector3[] arr)
        {
            float dist = 0;
            for (int a = 0; a < arr.Length - 1; a++)
                dist += Vector3.Distance(arr[a], arr[a + 1]);
            return dist;
        }

        public static bool IsBetween(this Vector3 v, Vector3 min, Vector3 max) =>
            v.x >= Mathf.Min(min.x, max.x) && v.x <= Mathf.Max(min.x, max.x) &&
                v.y >= Mathf.Min(min.y, max.y) && v.y <= Mathf.Max(min.y, max.y) &&
                    v.z >= Mathf.Min(min.z, max.z) && v.z <= Mathf.Max(min.z, max.z);

        public static bool IsZero(this Vector3 v) => Mathf.Approximately(v.x, 0) && Mathf.Approximately(v.y, 0) && Mathf.Approximately(v.z, 0);

        public static int IndexOf(this Vector3[] arr, Vector3 v)
        {
            for (int a = 0; a < arr.Length; a++)
                if (arr[a] == v) return a;
            return -1;
        }

        public static Vector3 Min(this Vector3[] arr)
        {
            Vector3 min = Vector3.one * float.MaxValue;
            for (int a = 0; a < arr.Length; a++)
            {
                if (arr[a].x <= min.x) min.x = arr[a].x;
                if (arr[a].y <= min.y) min.y = arr[a].y;
                if (arr[a].z <= min.z) min.z = arr[a].z;
            }
            return min;
        }

        public static Vector3 Max(this Vector3[] arr)
        {
            Vector3 max = Vector3.one * float.MinValue;
            for (int a = 0; a < arr.Length; a++)
            {
                if (arr[a].x >= max.x) max.x = arr[a].x;
                if (arr[a].y >= max.y) max.y = arr[a].y;
                if (arr[a].z >= max.z) max.z = arr[a].z;
            }
            return max;
        }

        public static Vector3 Size(this Vector3[] arr)
        {
            Vector3 min = Vector3.one * float.MaxValue;
            Vector3 max = Vector3.one * float.MinValue;
            for (int a = 0; a < arr.Length; a++)
            {
                if (arr[a].x <= min.x) min.x = arr[a].x;
                if (arr[a].y <= min.y) min.y = arr[a].y;
                if (arr[a].z <= min.z) min.z = arr[a].z;

                if (arr[a].x >= max.x) max.x = arr[a].x;
                if (arr[a].y >= max.y) max.y = arr[a].y;
                if (arr[a].z >= max.z) max.z = arr[a].z;
            }

            return max - min;
        }

        public static Vector2[] ToXZArray(this Vector3[] arr)
        {
            Vector2[] v2arr = new Vector2[arr.Length];
            for (int a = 0; a < v2arr.Length; a++)
                v2arr[a] = arr[a].Vector2FromXZ();
            return v2arr;
        }

        public static Vector3[] Add(this Vector3[] arr, Vector3 add)
        {
            for (int a = 0; a < arr.Length; a++)
                arr[a] += add;
            return arr;
        }

        public static Vector3[] SetX(this Vector3[] arr, float x)
        {
            for (int a = 0; a < arr.Length; a++)
                arr[a].x = x;
            return arr;
        }

        public static Vector3[] SetY(this Vector3[] arr, float y)
        {
            for (int a = 0; a < arr.Length; a++)
                arr[a].y = y;
            return arr;
        }

        public static Vector3[] SetZ(this Vector3[] arr, float z)
        {
            for (int a = 0; a < arr.Length; a++)
                arr[a].z = z;
            return arr;
        }

        public static Vector3 SetXY(this Vector3 v, float x, float y)
        {
            v.x = x;
            v.y = y;
            return v;
        }

        public static Vector3 SetX(this Vector3 v, float x)
        {
            v.x = x;
            return v;
        }

        public static Vector3 SetY(this Vector3 v, float y)
        {
            v.y = y;
            return v;
        }

        public static Vector3 SetZ(this Vector3 v, float z)
        {
            v.z = z;
            return v;
        }

        public static Vector3 AddX(this Vector3 v, float x)
        {
            v.x += x;
            return v;
        }

        public static Vector3 AddY(this Vector3 v, float y)
        {
            v.y += y;
            return v;
        }

        public static Vector3 AddZ(this Vector3 v, float z)
        {
            v.z += z;
            return v;
        }
    }
}