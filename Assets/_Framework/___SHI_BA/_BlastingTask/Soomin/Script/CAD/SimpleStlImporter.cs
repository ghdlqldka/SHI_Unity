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
            Debug.LogError("Binary STL �����Ͱ� �ʹ� ª���ϴ�.");
            return null;
        }

        uint faceCount = System.BitConverter.ToUInt32(data, 80);
        int index = 84;

        for (uint i = 0; i < faceCount; i++)
        {
            if (index + 50 > data.Length)
            {
                Debug.LogWarning($"�����Ͱ� �����Ͽ� {i}/{faceCount} face ���� �ߴ�");
                break;
            }

            index += 12; // normal �ǳʶٱ�

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
        // �޽� ���� �� ����
        var mesh = new Mesh();
        mesh.name = "STL_Imported_Mesh"; // �޽� �̸� ����

        // ū �޽� ����
        if (vertices.Count > 65535)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // hideFlags ���� - ���� ����
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // �����Ϳ����� DontSave �÷��� ����
            mesh.hideFlags = HideFlags.DontSaveInBuild;
        }
        else
        {
            // ��Ÿ�ӿ����� DontSave �÷��� ����
            mesh.hideFlags = HideFlags.DontSave;
        }
#else
        // ����� ���ӿ����� DontSave ����
        mesh.hideFlags = HideFlags.DontSave;
#endif

        Debug.Log($"Binary STL Mesh ���� �Ϸ�: {vertices.Count} verts, {triangles.Count / 3} tris");
        return mesh;
    }

#if UNITY_EDITOR
    /// <summary>
    /// �����Ϳ��� Asset���� �����ϰ� ���� �� ���
    /// </summary>
    public static Mesh ImportAsAsset(byte[] data, string assetPath, float scale = 1.0f, bool flipWinding = false, bool zUpToYUp = true)
    {
        var mesh = Import(data, scale, flipWinding, zUpToYUp);
        if (mesh != null)
        {
            mesh.hideFlags = HideFlags.None; // Asset���� ������ ���� �÷��� ����
            AssetDatabase.CreateAsset(mesh, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        return mesh;
    }
#endif
}