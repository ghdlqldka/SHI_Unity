// #define LOAD_FROM_NETWORK
// #define LOAD_FROM_STREAMINGASSET

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using System.Collections; // IEnumerator 사용을 위해 추가
using UnityEngine.Networking;
using AYellowpaper.SerializedCollections;
using static _SHI_BA.BA_Motion;

// EmbeddedPlaneData 스크립트가 동일한 네임스페이스에 있지 않다면 'using EmbeddedPlaneData;'를 추가하거나
// EmbeddedPlaneData.JsonContent를 사용할 때 'global::EmbeddedPlaneData.JsonContent'와 같이 전체 경로를 사용해야 합니다.
// 여기서는 EmbeddedPlaneData가 전역 스코프에 있다고 가정합니다.

namespace _SHI_BA
{
    public class _BA_LoadPlaneJson : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#25E04E><b>[_BA_LoadPlaneJson]</b></color> {0}";

        [ReadOnly]
        [SerializeField]
        protected bool _isDataLoaded = false;
        public bool IsDataLoaded 
        { 
            get
            {
                return _isDataLoaded;
            }
            private set
            {
                _isDataLoaded = value;
            }
        }

        [Header("설정")]
        [Tooltip("Assets/StreamingAssets 폴더 기준의 JSON 파일 경로")]
        public string jsonPath = "PlaneCfg/plane2.json";

        [Header("로드된 데이터")]
        [Tooltip("JSON에서 로드된 R 계열 면들이 저장되는 배열입니다.")]
        public BA_Motion.Cube[] R_Faces;

        [Tooltip("인스펙터 확인용: 로드된 W 계열 면들의 리스트입니다.")]
        public List<BA_Motion.Cube> W_Face_List = new List<BA_Motion.Cube>();

        [Tooltip("JSON에서 로드된 Edge 정보가 저장되는 리스트입니다.")]
        public List<BA_Motion.Edge> Edge_List = new List<BA_Motion.Edge>();

        [SerializedDictionary("string", "Cube")]
        public SerializedDictionary<string, BA_Motion.Cube> W_Faces = new SerializedDictionary<string, BA_Motion.Cube>();

        void Start()
        {
            StartCoroutine(LoadDataCoroutine());
        }

        public IEnumerator LoadDataCoroutine()
        {
            string json = null;

            if (string.IsNullOrEmpty(EmbeddedPlaneData.JsonContent) == false)
            {
                json = EmbeddedPlaneData.JsonContent;
            }
            else
            {
                string filePath = Path.Combine(Application.streamingAssetsPath, jsonPath);
#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log($"[Plane_and_NV] WebGL: URL에서 JSON 로드 시도 중: {filePath}");
            UnityWebRequest www = UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Plane_and_NV] JSON 파일 로드 실패 (WebGL): {www.error}");
                yield break;
            }
            json = www.downloadHandler.text;
#else
                Debug.LogFormat(LOG_FORMAT, $"Editor/PC: 경로에서 JSON 로드 시도 중: {filePath}");
                if (!File.Exists(filePath))
                {
                    Debug.AssertFormat(false, $"JSON 파일을 찾을 수 없습니다: {filePath}");
                    yield break;
                }
                try
                {
                    json = File.ReadAllText(filePath);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Plane_and_NV] JSON 파일 읽기 실패: {e.Message}");
                    yield break;
                }
#endif
            }

            if (string.IsNullOrEmpty(json) == true)
            {
                Debug.LogError("[Plane_and_NV] 로드된 JSON 내용이 비어있습니다.");
                yield break;
            }

            ParseJsonAndApplyData(json);

            IsDataLoaded = true;
            // OnDataLoaded?.Invoke();
            Debug.Log($"<color=cyan>JSON 로드 완료: R 계열 면 {R_Faces.Length}개, W 계열 면 {W_Faces.Count}개, Edge {Edge_List.Count}개</color>");
        }

        private void ParseJsonAndApplyData(string jsonContent)
        {
            var rFaceList = new List<BA_Motion.Cube>();
            W_Faces.Clear();
            W_Face_List.Clear();
            Edge_List.Clear(); // <<< NEW: Edge 리스트 초기화

            BA_Motion.CustomRoot root = JsonUtility.FromJson<BA_Motion.CustomRoot>(jsonContent);

            FieldInfo[] fields = typeof(BA_Motion.CustomRoot).GetFields();
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(BA_Motion.CustomQuad))
                {
                    BA_Motion.CustomQuad quad = (BA_Motion.CustomQuad)field.GetValue(root);
                    if (quad != null && quad.top_left != null)
                    {
                        BA_Motion.Cube c = new BA_Motion.Cube
                        {
                            Name = field.Name,
                            R1 = ConvertToUnity(quad.top_left),
                            R2 = ConvertToUnity(quad.top_right),
                            R4 = ConvertToUnity(quad.bottom_left),
                            R3 = ConvertToUnity(quad.bottom_right),
                            normal = -Vector3.Cross(
                                ConvertToUnity(quad.top_right) - ConvertToUnity(quad.top_left),
                                ConvertToUnity(quad.bottom_left) - ConvertToUnity(quad.top_left)
                            ).normalized
                        };

                        if (c.Name.StartsWith("W", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!W_Faces.ContainsKey(c.Name))
                            {
                                W_Faces.Add(c.Name, c);
                                W_Face_List.Add(c);
                            }
                        }
                        else if (c.Name.StartsWith("R", StringComparison.OrdinalIgnoreCase))
                        {
                            rFaceList.Add(c);
                        }

                        if (c.Name == "R7")
                        {
                            if (quad.edge_1_start != null && quad.edge_1_end != null)
                            {
                                Edge_List.Add(new BA_Motion.Edge { Name = "R7_Edge_1", Start = ConvertToUnity(quad.edge_1_start), End = ConvertToUnity(quad.edge_1_end) });
                            }
                            // ... (나머지 edge들도 동일하게 수정) ...
                            if (quad.edge_2_start != null && quad.edge_2_end != null)
                                Edge_List.Add(new BA_Motion.Edge { Name = "R7_Edge_2", Start = ConvertToUnity(quad.edge_2_start), End = ConvertToUnity(quad.edge_2_end) });
                            if (quad.edge_3_start != null && quad.edge_3_end != null)
                                Edge_List.Add(new BA_Motion.Edge { Name = "R7_Edge_3", Start = ConvertToUnity(quad.edge_3_start), End = ConvertToUnity(quad.edge_3_end) });
                            if (quad.edge_4_start != null && quad.edge_4_end != null)
                                Edge_List.Add(new BA_Motion.Edge { Name = "R7_Edge_4", Start = ConvertToUnity(quad.edge_4_start), End = ConvertToUnity(quad.edge_4_end) });
                            if (quad.edge_5_start != null && quad.edge_5_end != null)
                                Edge_List.Add(new BA_Motion.Edge { Name = "R7_Edge_5", Start = ConvertToUnity(quad.edge_5_start), End = ConvertToUnity(quad.edge_5_end) });
                            if (quad.edge_6_start != null && quad.edge_6_end != null)
                                Edge_List.Add(new BA_Motion.Edge { Name = "R7_Edge_6", Start = ConvertToUnity(quad.edge_6_start), End = ConvertToUnity(quad.edge_6_end) });
                            if (quad.edge_7_start != null && quad.edge_7_end != null)
                                Edge_List.Add(new BA_Motion.Edge { Name = "R7_Edge_7", Start = ConvertToUnity(quad.edge_7_start), End = ConvertToUnity(quad.edge_7_end) });
                            if (quad.edge_8_start != null && quad.edge_8_end != null)
                                Edge_List.Add(new BA_Motion.Edge { Name = "R7_Edge_8", Start = ConvertToUnity(quad.edge_8_start), End = ConvertToUnity(quad.edge_8_end) });
                            // --- ---------------
                        }
                    }
                }
            }
            R_Faces = rFaceList.ToArray();
        }

        private Vector3 ConvertToUnity(CustomVertex v) => new Vector3(v.x, v.y, v.z);
    }
}