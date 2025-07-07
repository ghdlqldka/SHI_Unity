using UnityEngine;
using UnityEditor;

public class BakeScaleIntoMesh
{
    // Transform 컴포넌트의 컨텍스트 메뉴(...)에 "Bake Scale into Mesh" 항목을 추가합니다.
    [MenuItem("CONTEXT/Transform/Bake Scale into Mesh")]
    public static void BakeScale(MenuCommand command)
    {
        Transform targetTransform = (Transform)command.context;
        MeshFilter meshFilter = targetTransform.GetComponent<MeshFilter>();

        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter가 없는 오브젝트입니다. 스케일을 적용할 수 없습니다.", targetTransform);
            return;
        }

        Mesh originalMesh = meshFilter.sharedMesh;
        if (originalMesh == null)
        {
            Debug.LogError("적용할 메시가 없습니다.", targetTransform);
            return;
        }

        // 1. 원본 메시를 수정하지 않도록 메시를 복제합니다. (매우 중요!)
        Mesh newMesh = Object.Instantiate(originalMesh);

        // 2. 메시의 모든 정점(Vertex) 데이터를 가져옵니다.
        Vector3[] vertices = newMesh.vertices;
        Vector3 scale = targetTransform.localScale;

        // 3. 각 정점의 위치에 현재 스케일 값을 곱해줍니다.
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= scale.x;
            vertices[i].y *= scale.y;
            vertices[i].z *= scale.z;
        }

        // 4. 스케일이 적용된 새 정점 데이터로 메시를 업데이트합니다.
        newMesh.vertices = vertices;
        newMesh.RecalculateNormals(); // 법선(조명 방향)을 다시 계산합니다.
        newMesh.RecalculateBounds();  // 메시의 경계를 다시 계산합니다.

        // 5. 복제되고 스케일이 적용된 새 메시를 저장할 경로를 만듭니다.
        string path = "Assets/" + newMesh.name + "_scaled.asset";
        path = AssetDatabase.GenerateUniqueAssetPath(path);
        
        // 6. 새 메시를 프로젝트 에셋으로 저장합니다.
        AssetDatabase.CreateAsset(newMesh, path);
        AssetDatabase.SaveAssets();

        // 7. MeshFilter가 새 메시를 사용하도록 변경합니다.
        meshFilter.sharedMesh = newMesh;

        // 8. 마지막으로, 오브젝트의 스케일을 (1, 1, 1)로 초기화합니다.
        targetTransform.localScale = Vector3.one;

        Debug.Log("스케일이 메시에 성공적으로 적용되었습니다! 새 메시는 여기에 저장되었습니다: " + path, targetTransform);
    }
}