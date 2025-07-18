﻿// math helpers

using PointCloudViewer.Structs;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace PointCloudHelpers
{
    public static class PointCloudMath
    {

        public static readonly int seed = System.Guid.NewGuid().GetHashCode();
        public static System.Random rnd = new System.Random(seed);
        public static void ResetRandom()
        {
            rnd = new System.Random(seed);
        }

        // https://stackoverflow.com/a/110570/5452781
        public static void ShuffleXYZ<T>(System.Random rng, ref T[] array1)
        {
            int n = array1.Length;
            int maxVal = array1.Length / 3; // xyz key

            while (n > 3)
            {
                int k = rng.Next(maxVal) * 3; // multiples of 3 only
                n -= 3;

                T tempX = array1[n];
                array1[n] = array1[k];
                array1[k] = tempX;

                T tempY = array1[n + 1];
                array1[n + 1] = array1[k + 1];
                array1[k + 1] = tempY;

                T tempZ = array1[n + 2];
                array1[n + 2] = array1[k + 2];
                array1[k + 2] = tempZ;
            }
        }

        // https://gamedev.stackexchange.com/a/103714/73429
        public static float RayBoxIntersect2(Vector3 rpos, Vector3 irdir, Vector3 vmin, Vector3 vmax)
        {
            float t1 = (vmin.x - rpos.x) * irdir.x;
            float t2 = (vmax.x - rpos.x) * irdir.x;
            float t3 = (vmin.y - rpos.y) * irdir.y;
            float t4 = (vmax.y - rpos.y) * irdir.y;
            float t5 = (vmin.z - rpos.z) * irdir.z;
            float t6 = (vmax.z - rpos.z) * irdir.z;
            float aMin = t1 < t2 ? t1 : t2;
            float aMax = t1 > t2 ? t1 : t2;
            float bMin = t3 < t4 ? t3 : t4;
            float bMax = t3 > t4 ? t3 : t4;
            float cMin = t5 < t6 ? t5 : t6;
            float cMax = t5 > t6 ? t5 : t6;
            float fMax = aMin > bMin ? aMin : bMin;
            float fMin = aMax < bMax ? aMax : bMax;
            float t7 = fMax > cMin ? fMax : cMin;
            float t8 = fMin < cMax ? fMin : cMax;
            float t9 = (t8 < 0 || t7 > t8) ? -1 : t7;
            return t9;
        }

        public static float RayBoxIntersect2(Vector3 rpos, Vector3 irdir, float vminX, float vminY, float vminZ, float vmaxX, float vmaxY, float vmaxZ)
        {
            float t1 = (vminX - rpos.x) * irdir.x;
            float t2 = (vmaxX - rpos.x) * irdir.x;
            float t3 = (vminY - rpos.y) * irdir.y;
            float t4 = (vmaxY - rpos.y) * irdir.y;
            float t5 = (vminZ - rpos.z) * irdir.z;
            float t6 = (vmaxZ - rpos.z) * irdir.z;
            float aMin = t1 < t2 ? t1 : t2;
            float aMax = t1 > t2 ? t1 : t2;
            float bMin = t3 < t4 ? t3 : t4;
            float bMax = t3 > t4 ? t3 : t4;
            float cMin = t5 < t6 ? t5 : t6;
            float cMax = t5 > t6 ? t5 : t6;
            float fMax = aMin > bMin ? aMin : bMin;
            float fMin = aMax < bMax ? aMax : bMax;
            float t7 = fMax > cMin ? fMax : cMin;
            float t8 = fMin < cMax ? fMin : cMax;
            float t9 = (t8 < 0 || t7 > t8) ? -1 : t7;
            return t9;
        }

        public static float Distance(Vector3 a, Vector3 b)
        {
            float vecx = a.x - b.x;
            float vecy = a.y - b.y;
            float vecz = a.z - b.z;
            return vecx * vecx + vecy * vecy + vecz * vecz;
        }


        // http://answers.unity.com/answers/523979/view.html
        public static float DistanceToRay(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            return Vector3.Magnitude(ProjectPointLine(point, lineStart, lineEnd) - point);
        }

        public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 rhs = point - lineStart;
            Vector3 vector2 = lineEnd - lineStart;
            float magnitude = vector2.magnitude;
            Vector3 lhs = vector2;
            if (magnitude > 1E-06f)
            {
                lhs = (Vector3)(lhs / magnitude);
            }
            float num2 = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0f, magnitude);
            return (lineStart + ((Vector3)(lhs * num2)));
        }

        // note picks from behind also https://gamedev.stackexchange.com/a/131401/73429
        public static float SqDistPointSegment(Vector3 start, Vector3 end, Vector3 point)
        {
            var ab = end - start;
            var ac = point - start;
            var bc = point - end;
            float e = Vector3.Dot(ac, ab);
            // Handle cases where c projects outside ab
            //if (e <= 0.0f) return Vector3.Dot(ac, ac);
            float f = Vector3.Dot(ab, ab);
            if (e >= f) return Vector3.Dot(bc, bc);
            // Handle cases where c projects onto ab
            return Vector3.Dot(ac, ac) - e * e / f;
        }

        // TODO scale this to keep equal size for all distances?
        const float lineLen = 0.25f;

        public static void DebugHighLightPointGray(System.Object op)
        {
            var p = (Vector3)op;
            var c = Color.gray;
            Debug.DrawRay(p, Vector3.up * lineLen, c, 33);
            Debug.DrawRay(p, -Vector3.up * lineLen, c, 33);
            Debug.DrawRay(p, Vector3.right * lineLen, c, 33);
            Debug.DrawRay(p, -Vector3.right * lineLen, c, 33);
            Debug.DrawRay(p, Vector3.forward * lineLen, c, 33);
            Debug.DrawRay(p, -Vector3.forward * lineLen, c, 33);
        }

        public static void DebugHighLightPointGreen(System.Object op)
        {
            var p = (Vector3)op;
            var c = Color.green;
            Debug.DrawRay(p, Vector3.up * lineLen, c, 33);
            Debug.DrawRay(p, -Vector3.up * lineLen, c, 33);
            Debug.DrawRay(p, Vector3.right * lineLen, c, 33);
            Debug.DrawRay(p, -Vector3.right * lineLen, c, 33);
            Debug.DrawRay(p, Vector3.forward * lineLen, c, 33);
            Debug.DrawRay(p, -Vector3.forward * lineLen, c, 33);
        }

        public static void DebugHighLightPointRed(System.Object op)
        {
            var p = (Vector3)op;
            var c = Color.red;
            Debug.DrawRay(p, Vector3.up * lineLen, c, 33);
            Debug.DrawRay(p, -Vector3.up * lineLen, c, 33);
            Debug.DrawRay(p, Vector3.right * lineLen, c, 33);
            Debug.DrawRay(p, -Vector3.right * lineLen, c, 33);
            Debug.DrawRay(p, Vector3.forward * lineLen, c, 33);
            Debug.DrawRay(p, -Vector3.forward * lineLen, c, 33);
        }

        public static void DrawRay(System.Object op)
        {
            var r = (Ray)op;
            Debug.DrawRay(r.origin, r.direction * 100f, Color.red, 22); // NOTE 100 is the length here
        }

        public static void DebugHighLightPointYellow(System.Object op)
        {
            var p = (Vector3)op;
            var c = Color.yellow;
            Debug.DrawRay(p, Vector3.up * lineLen, c, 33);
            Debug.DrawRay(p, -Vector3.up * lineLen, c, 33);
            Debug.DrawRay(p, Vector3.right * lineLen, c, 33);
            Debug.DrawRay(p, -Vector3.right * lineLen, c, 33);
            Debug.DrawRay(p, Vector3.forward * lineLen, c, 33);
            Debug.DrawRay(p, -Vector3.forward * lineLen, c, 33);
        }

        // color (0-1), y
        public static float SuperPacker(float f1, float f2)
        {
            float truncatedF2 = (float)System.Math.Truncate(f2 * 1024);
            return truncatedF2 + f1;
        }

        // color, y
        public static Vector2 SuperUnpacker(float f, float _GridSizeAndPackMagic)
        {
            return new Vector2(f - Mathf.Floor(f), Mathf.Floor(f) / _GridSizeAndPackMagic);
        }

        // returns color, scalar, position
        public static Vector3 SuperUnpacker(int packed)
        {
            byte aUnpacked = (byte)(packed >> 24);
            byte bUnpacked = (byte)(packed >> 16);
            byte cIntegralUnpacked = (byte)(packed >> 8);
            byte cFractionalUnpacked = (byte)packed;
            float cUnpacked = cIntegralUnpacked + (cFractionalUnpacked / 255.0f);
            return new Vector3(aUnpacked / 255f, bUnpacked / 255f, cUnpacked);
        }

        unsafe public static float BytesToFloat(byte[] value, int startIndex)
        {
            int val = value[startIndex] | value[startIndex + 1] << 8 | value[startIndex + 2] << 16 | value[startIndex + 3] << 24;
            return *(float*)&val;
        }

        unsafe public static float BytesToFloat(byte value0, byte value1, byte value2, byte value3)
        {
            int val = value0 | value1 << 8 | value2 << 16 | value3 << 24;
            return *(float*)&val;
        }

        unsafe public static int BytesToInt(byte value0, byte value1, byte value2, byte value3)
        {
            int val = value0 | value1 << 8 | value2 << 16 | value3 << 24;
            return val;
        }

        public static int BytesToInt(byte[] value, int startIndex)
        {
            return value[startIndex] | value[startIndex + 1] << 8 | value[startIndex + 2] << 16 | value[startIndex + 3] << 24;
        }

        // https://forum.unity.com/threads/copy-any-nativearray-t-into-a-byte-array-and-back.549727/#post-3635716
#if UNITY_2019_1_OR_NEWER
        public unsafe static void MoveFromByteArray<T>(ref byte[] src, ref NativeArray<T> dst) where T : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(NativeArrayUnsafeUtility.GetAtomicSafetyHandle(dst));
            if (src == null) throw new System.ArgumentNullException(nameof(src));
#endif
            //            var size = UnsafeUtility.SizeOf<T>();
            //            if (src.Length != (size * dst.Length))
            //            {
            //                dst.Dispose();
            //                dst = new NativeArray<T>(src.Length / size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            //#if ENABLE_UNITY_COLLECTIONS_CHECKS
            //                AtomicSafetyHandle.CheckReadAndThrow(NativeArrayUnsafeUtility.GetAtomicSafetyHandle(dst));
            //#endif
            //            }

            var dstAddr = (byte*)dst.GetUnsafeReadOnlyPtr();
            fixed (byte* srcAddr = src)
            {
                long size = src.Length < dst.Length ? src.Length : dst.Length;
                UnsafeUtility.MemCpy(&dstAddr[0], &srcAddr[0], size);
            }
        }
#endif

        public static float RayBoxIntersect3(Vector3 rpos, Vector3 rdir, Vector3 vmin, Vector3 vmax)
        {
            float t1 = (vmin.x - rpos.x) / rdir.x;
            float t2 = (vmax.x - rpos.x) / rdir.x;
            float t3 = (vmin.y - rpos.y) / rdir.y;
            float t4 = (vmax.y - rpos.y) / rdir.y;
            float t5 = (vmin.z - rpos.z) / rdir.z;
            float t6 = (vmax.z - rpos.z) / rdir.z;

            float aMin = t1 < t2 ? t1 : t2;
            float bMin = t3 < t4 ? t3 : t4;
            float cMin = t5 < t6 ? t5 : t6;

            float aMax = t1 > t2 ? t1 : t2;
            float bMax = t3 > t4 ? t3 : t4;
            float cMax = t5 > t6 ? t5 : t6;

            float fMax = aMin > bMin ? aMin : bMin;
            float fMin = aMax < bMax ? aMax : bMax;

            float t7 = fMax > cMin ? fMax : cMin;
            float t8 = fMin < cMax ? fMin : cMax;

            return (t8 < 0 || t7 > t8) ? -1 : t7;
        }

        public static bool rayAABBIntersection(Vector3 rayOrigin, Vector3 rayInverseDirection, NodeBox aabb)
        {
            float tx1 = (aabb.bounds.min.x - rayOrigin.x) * rayInverseDirection.x;
            float tx2 = (aabb.bounds.max.x - rayOrigin.x) * rayInverseDirection.x;

            float tmin = Mathf.Min(tx1, tx2);
            float tmax = Mathf.Max(tx1, tx2);

            float ty1 = (aabb.bounds.min.y - rayOrigin.y) * rayInverseDirection.y;
            float ty2 = (aabb.bounds.max.y - rayOrigin.y) * rayInverseDirection.y;

            tmin = Mathf.Max(tmin, Mathf.Min(ty1, ty2));
            tmax = Mathf.Min(tmax, Mathf.Max(ty1, ty2));

            float tz1 = (aabb.bounds.min.z - rayOrigin.z) * rayInverseDirection.z;
            float tz2 = (aabb.bounds.max.z - rayOrigin.z) * rayInverseDirection.z;

            tmin = Mathf.Max(tmin, Mathf.Min(tz1, tz2));
            tmax = Mathf.Min(tmax, Mathf.Max(tz1, tz2));

            if (tmin > tmax)
            {
                return false;// Mathf.Infinity;
            }

            if (tmin < 0)
            {
                return true;// tmax;
            }

            return tmin > 0;// tmin;
        }

        public static bool NodeContainsPoint(NodeBox tile, Vector3 point)
        {
            return tile.bounds.min.x < point.x && tile.bounds.max.x > point.x
                && tile.bounds.min.y < point.y && tile.bounds.max.y > point.y
                && tile.bounds.min.z < point.z && tile.bounds.max.z > point.z;
        }

        // https://answers.unity.com/questions/53989/test-to-see-if-a-vector3-point-is-within-a-boxcoll.html?childToView=1472403#comment-1472403
        public static bool IsPointInsideBoxCollider(Vector3 point, BoxCollider box)
        {
            point = box.transform.InverseTransformPoint(point);

            var c = box.center;
            var s = box.size;
            float X = s.x * 0.5f + point.x - c.x;
            if (X < 0 || X > s.x) return false;
            float Y = s.y * 0.5f + point.y - c.y;
            if (Y < 0 || Y > s.y) return false;
            float Z = s.z * 0.5f + point.z - c.z;
            if (Z < 0 || Z > s.z) return false;
            return true;
        }

        // can be called from thread
        public static bool IsPointInsideBoxCollider(Vector3 point, Vector3 boxPos, Quaternion boxRotation, Vector3 boxScale, Vector3 boxCenter, Vector3 boxSize)
        {
            point = InverseTransformPoint(boxPos, boxRotation, boxScale, point);
            float X = boxSize.x * 0.5f + point.x - boxCenter.x;
            if (X < 0 || X > boxSize.x) return false;
            float Y = boxSize.y * 0.5f + point.y - boxCenter.y;
            if (Y < 0 || Y > boxSize.y) return false;
            float Z = boxSize.z * 0.5f + point.z - boxCenter.z;
            if (Z < 0 || Z > boxSize.z) return false;
            return true;
        }

        // https://forum.unity.com/threads/transform-inversetransformpoint-without-transform.954939/#post-6224040
        public static Vector3 InverseTransformPoint(Vector3 transformPos, Quaternion transformRotation, Vector3 transformScale, Vector3 point)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(transformPos, transformRotation, transformScale);
            Matrix4x4 inverse = matrix.inverse;
            return inverse.MultiplyPoint3x4(point);
            //return inverse.MultiplyPoint(pos);
        }


        // https://studiofreya.com/3d-math-and-physics/simple-aabb-vs-aabb-collision-detection/
        public static bool AABBIntersectsAABB(Bounds a, float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            var acX = a.center.x;
            var acY = a.center.y;
            var acZ = a.center.z;

            var bcX = (minX + maxX) * 0.5f;
            var bcY = (minY + maxY) * 0.5f;
            var bcZ = (minZ + maxZ) * 0.5f;

            var arX = a.extents.x;
            var arY = a.extents.y;
            var arZ = a.extents.z;

            var brX = (maxX - minX) * 0.5f;
            var brY = (maxY - minY) * 0.5f;
            var brZ = (maxZ - minZ) * 0.5f;

            bool x = Mathf.Abs(acX - bcX) <= (arX + brX);
            bool y = Mathf.Abs(acY - bcY) <= (arY + brY);
            bool z = Mathf.Abs(acZ - bcZ) <= (arZ + brZ);

            return x && y && z;
        }

    }
}
