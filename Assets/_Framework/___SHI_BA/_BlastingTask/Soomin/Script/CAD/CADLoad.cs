using Dummiesman;
using PaintIn3D;
using System.IO;
using UnityEngine;
using System.Collections;
public class CADLoad : MonoBehaviour
{
    private SystemController robotSystemController;
    private string Path = @"C:\Users\user\Desktop\MyWebBuild\ImageToStl.com_testblock.obj";
    public Material setMaterial;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
#if UNITY_EDITOR
     //   Test();
#endif
    }
    
    public void Test()
    {

        if (File.Exists(Path))
        {
            var loader = new OBJLoader();
            GameObject obj = loader.Load(Path);

            obj.transform.localScale = Vector3.one * 10f;
            obj.transform.localPosition = new Vector3(-8.1f, 2.48f, 8.29f);
            obj.transform.localRotation = Quaternion.Euler(-90, 90, 0);

            SetLayerRecursively(obj, LayerMask.NameToLayer("Block"));

            foreach (var renderer in obj.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.material = setMaterial;
            }
            Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.Optimize();
            obj.AddComponent<CwPaintableMesh>();

        }
        // else
        // {
        //     Debug.LogError("STL 파일을 찾을 수 없습니다: " + Path);
        // }
    }
    void CreateMeshFromObj(byte[] objBytes)
    {

        if (File.Exists(Path))
        {
            var loader = new OBJLoader();
            GameObject obj = loader.Load(Path);

            obj.transform.localScale = Vector3.one * 10f;
            obj.transform.localPosition = new Vector3(-8.1f, 2.48f, 8.29f);
            obj.transform.localRotation = Quaternion.Euler(-90, 90, 0);

            SetLayerRecursively(obj, LayerMask.NameToLayer("Block"));

            foreach (var renderer in obj.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.material = setMaterial;
            }
            Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.Optimize();
            obj.AddComponent<CwPaintableMesh>();

        }
        else
        {
            Debug.LogError("OBJ 파일을 찾을 수 없습니다: " + Path);
        }
        // STL 파싱 및 Mesh 생성
        /* Mesh mesh = SimpleObjImporter.Import(objBytes, 0.0001f);

         if (mesh != null)
         {
             CreateMeshObjectWithPaint(mesh);

             Debug.Log("Blob 데이터로부터 오브젝트 생성 완료");
         }
         else
         {
             Debug.LogError("Blob 데이터 파싱 실패");
         }*/
    }
    public void ShowBlockLoad(byte[] Bytes)
    {

        string headText = System.Text.Encoding.ASCII.GetString(Bytes, 0, Mathf.Min(100, Bytes.Length)).ToLower();
        if (headText.Contains("v ") || headText.Contains("f ") || headText.Contains("#"))
        {
            CreateMeshFromObj(Bytes);

        }
        else if (headText.Contains("solid"))
        {
            // ASCII STL
            Debug.LogError("ASCII STL은 없어요");
        }
        else
        {
            CreateMeshFromBinaryStl(Bytes);
        }

    }
    private void CreateMeshFromBinaryStl(byte[] data)
    {

        UnityEngine.Mesh mesh = SimpleStlImporter.Import(data, 0.01f, true); // scale: 0.01 (mm → m)

        if (mesh != null)
        {
            CreateMeshObjectWithPaint(mesh);
            Debug.Log("Binary STL 데이터로부터 오브젝트 생성 완료");
        }
        else
        {
            Debug.LogError("Binary STL 파싱 실패");
        }
    }
    private void CreateMeshObjectWithPaint(UnityEngine.Mesh mesh)
    {
        GameObject obj = new GameObject("CADObject");
        obj.layer = LayerMask.NameToLayer("Block");
        obj.AddComponent<MeshFilter>().mesh = mesh;
        obj.AddComponent<MeshRenderer>().material = setMaterial;
        obj.transform.localScale = Vector3.one * 996.6f;
        obj.transform.localPosition = new Vector3(-8.1f, 2.48f, 8.29f);
        obj.transform.localRotation = UnityEngine.Quaternion.Euler(0, -90, 0);
        obj.AddComponent<CwPaintableMesh>();

        StartCoroutine(CreateConvexCollider(obj, mesh));

    }
    private IEnumerator CreateConvexCollider(GameObject obj, UnityEngine.Mesh mesh)
    {
        robotSystemController = SystemController.Instance;
        if (robotSystemController.cadCollider != null)
        {
            Destroy(robotSystemController.cadCollider.gameObject);
            robotSystemController.cadCollider = null;
        }
        yield return null; // 다음 프레임
        var collider = obj.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
        //collider.convex = true;
        SystemController.Instance.SetCollider(obj.GetComponent<MeshCollider>());
        //collider.convex = false;

    }
    void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

}
