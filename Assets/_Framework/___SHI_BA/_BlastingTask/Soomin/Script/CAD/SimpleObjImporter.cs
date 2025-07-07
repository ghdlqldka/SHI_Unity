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
                // �� face�� �ﰢ�� �Ǵ� �ٰ���. ���⼱ �ﰢ���� ���� (3�� ����)
                // �� ������ v/vt/vn �Ǵ� v//vn �Ǵ� v ���� ����
                if (parts.Length >= 4)
                {
                    // OBJ���� �ﰢ���� �ƴϰ� �ٰ����� ��� �ﰢ���� �ʿ���
                    // ���⼭�� ������ �ﰢ���� ó�� (f v1 v2 v3)
                    for (int i = 1; i < 4; i++)
                    {
                        string vertexDef = parts[i];
                        var subParts = vertexDef.Split('/');

                        if (int.TryParse(subParts[0], out int vertexIndex))
                        {
                            // OBJ �ε����� 1���� ����, Unity�� 0����
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
