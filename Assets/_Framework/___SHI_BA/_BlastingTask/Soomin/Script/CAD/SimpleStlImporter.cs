using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SimpleStlImporter
{
    public static Mesh Import(byte[] data, float scale = 1.0f, bool flipWinding = false, bool zUpToYUp = true)
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        if (data.Length < 84)
        {
            Debug.LogError("Binary STL 데이터가 너무 짧습니다.");
            return null;
        }

        uint faceCount = System.BitConverter.ToUInt32(data, 80);
        int index = 84;

        for (uint i = 0; i < faceCount; i++)
        {
            if (index + 50 > data.Length)
            {
                Debug.LogWarning($"데이터가 부족하여 {i}/{faceCount} face 이후 중단");
                break;
            }

            index += 12; // normal 건너뛰기

            int baseIndex = vertices.Count;

            for (int v = 0; v < 3; v++)
            {
                float x = System.BitConverter.ToSingle(data, index) * scale;
                float y = System.BitConverter.ToSingle(data, index + 4) * scale;
                float z = System.BitConverter.ToSingle(data, index + 8) * scale;

                Vector3 vertex;
                if (zUpToYUp)
                {
                    vertex = new Vector3(x, z, y);
                }
                else
                {
                    vertex = new Vector3(x, y, z);
                }

                vertices.Add(vertex);
                index += 12;
            }

            if (flipWinding)
            {
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 0);
            }
            else
            {
                triangles.Add(baseIndex + 0);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
            }

            index += 2; // attribute byte count
        }
        // 메쉬 생성 및 설정
        var mesh = new Mesh();
        mesh.name = "STL_Imported_Mesh"; // 메쉬 이름 설정

        // 큰 메쉬 지원
        if (vertices.Count > 65535)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // hideFlags 설정 - 에러 방지
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // 에디터에서는 DontSave 플래그 설정
            mesh.hideFlags = HideFlags.DontSaveInBuild;
        }
        else
        {
            // 런타임에서는 DontSave 플래그 설정
            mesh.hideFlags = HideFlags.DontSave;
        }
#else
        // 빌드된 게임에서는 DontSave 설정
        mesh.hideFlags = HideFlags.DontSave;
#endif

        Debug.Log($"Binary STL Mesh 생성 완료: {vertices.Count} verts, {triangles.Count / 3} tris");
        return mesh;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 Asset으로 저장하고 싶을 때 사용
    /// </summary>
    public static Mesh ImportAsAsset(byte[] data, string assetPath, float scale = 1.0f, bool flipWinding = false, bool zUpToYUp = true)
    {
        var mesh = Import(data, scale, flipWinding, zUpToYUp);
        if (mesh != null)
        {
            mesh.hideFlags = HideFlags.None; // Asset으로 저장할 때는 플래그 제거
            AssetDatabase.CreateAsset(mesh, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        return mesh;
    }
#endif
}