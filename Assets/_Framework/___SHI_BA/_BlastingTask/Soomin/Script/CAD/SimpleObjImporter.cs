using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public static class SimpleObjImporter 
{
    public static Mesh Import(byte[] objData, float scale = 1f, Encoding encoding = null)
    {
        if (objData == null || objData.Length == 0)
        {
            Debug.LogError("OBJ data is null or empty.");
            return null;
        }

        if (encoding == null)
            encoding = Encoding.UTF8;

        string objText = encoding.GetString(objData);

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        string[] lines = objText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            string trimmedLine = line.Trim();

            if (trimmedLine.StartsWith("v "))
            {
                // vertex position
                var parts = trimmedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4 &&
                    float.TryParse(parts[1], out float x) &&
                    float.TryParse(parts[2], out float y) &&
                    float.TryParse(parts[3], out float z))
                {
                    vertices.Add(new Vector3(x, y, z) * scale);
                }
            }
            else if (trimmedLine.StartsWith("vn "))
            {
                // normal
                var parts = trimmedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4 &&
                    float.TryParse(parts[1], out float x) &&
                    float.TryParse(parts[2], out float y) &&
                    float.TryParse(parts[3], out float z))
                {
                    normals.Add(new Vector3(x, y, z));
                }
            }
            else if (trimmedLine.StartsWith("vt "))
            {
                // uv
                var parts = trimmedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3 &&
                    float.TryParse(parts[1], out float u) &&
                    float.TryParse(parts[2], out float v))
                {
                    uvs.Add(new Vector2(u, v));
                }
            }
            else if (trimmedLine.StartsWith("f "))
            {
                // face
                var parts = trimmedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                // 각 face는 삼각형 또는 다각형. 여기선 삼각형만 가정 (3개 정점)
                // 각 정점은 v/vt/vn 또는 v//vn 또는 v 형식 가능
                if (parts.Length >= 4)
                {
                    // OBJ에서 삼각형이 아니고 다각형인 경우 삼각분할 필요함
                    // 여기서는 간단히 삼각형만 처리 (f v1 v2 v3)
                    for (int i = 1; i < 4; i++)
                    {
                        string vertexDef = parts[i];
                        var subParts = vertexDef.Split('/');

                        if (int.TryParse(subParts[0], out int vertexIndex))
                        {
                            // OBJ 인덱스는 1부터 시작, Unity는 0부터
                            triangles.Add(vertexIndex - 1);
                        }
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "ImportedOBJ";

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        if (uvs.Count == vertices.Count)
            mesh.SetUVs(0, uvs);

        if (normals.Count == vertices.Count)
            mesh.SetNormals(normals);
        else
            mesh.RecalculateNormals();

        mesh.RecalculateBounds();

        return mesh;
    }
}
