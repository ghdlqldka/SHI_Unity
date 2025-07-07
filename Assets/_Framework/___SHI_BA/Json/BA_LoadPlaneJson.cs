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
    public class BA_LoadPlaneJson : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#25E04E><b>[BA_PlaneAndNormalVector]</b></color> {0}";

        [Header("설정")]
        [Tooltip("Assets/StreamingAssets 폴더 기준의 JSON 파일 경로")]
        public string jsonPath = "PlaneCfg/plane2.json";

        [ReadOnly]
        [SerializeField]
        protected bool _isDataLoaded = false;

        public bool IsDataLoaded
        {
            get
            {
                return _isDataLoaded;
            }
            protected set
            {
                _isDataLoaded = value;
            }
        }

        public UnityAction OnDataLoaded;

        [Header("로드된 데이터")]
        [Tooltip("JSON에서 로드된 R 계열 면들이 저장되는 배열입니다.")]
        [ReadOnly]
        [SerializeField]
        protected Cube[] _R_Faces;
        public Cube[] R_Faces
        {
            get
            {
                return _R_Faces;
            }
        }

        [Tooltip("인스펙터 확인용: 로드된 W 계열 면들의 리스트입니다.")]
        [ReadOnly]
        [SerializeField]
        protected List<Cube> _W_Face_List = new List<Cube>();
        public List<Cube> W_Face_List
        {
            get
            {
                return _W_Face_List;
            }
        }

        [Tooltip("JSON에서 로드된 Edge 정보가 저장되는 리스트입니다.")]
        [ReadOnly]
        [SerializeField]
        protected List<Edge> _Edge_List = new List<Edge>();
        public List<Edge> Edge_List
        {
            get
            {
                return _Edge_List;
            }
        }

        [ReadOnly]
        [SerializeField]
        [SerializedDictionary("Json key", "Cube")]
        protected SerializedDictionary<string, Cube> _W_Faces = new SerializedDictionary<string, Cube>();
        public SerializedDictionary<string, Cube> W_Faces
        {
            get
            {
                return _W_Faces;
            }
        }

        protected virtual void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            StartCoroutine(LoadDataCoroutine());
        }

        private Vector3 ConvertToUnity(CustomVertex v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        protected IEnumerator LoadDataCoroutine()
        {
            Debug.LogFormat(LOG_FORMAT, "LoadDataCoroutine()");

            string json = null;

#if LOAD_FROM_NETWORK
            string uri;

            Debug.Log($"[Plane_and_NV] WebGL: URL에서 JSON 로드 시도 중: {uri}");
            UnityWebRequest www = UnityWebRequest.Get(uri);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Plane_and_NV] JSON 파일 로드 실패 (WebGL): {www.error}");
                yield break;
            }
            json = www.downloadHandler.text;
#elif LOAD_FROM_STREAMINGASSET
            string filePath = Path.Combine(Application.streamingAssetsPath, jsonPath);
            Debug.LogFormat(LOG_FORMAT, $"Editor/PC: 경로에서 JSON 로드 시도 중: {filePath}");
            if (File.Exists(filePath) == false)
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
                Debug.LogErrorFormat(LOG_FORMAT, "" + e.ToString());
                yield break;
            }
#else
#if false // @1
            if (string.IsNullOrEmpty(BA_HardcodingPlaneData.JsonContent) == false)
            {
                Debug.LogFormat("@1");
                json = BA_HardcodingPlaneData.JsonContent;
            }
            else
            {
                Debug.Assert(false);
            }
#else // @1
            if (string.IsNullOrEmpty(EmbeddedPlaneData.JsonContent) == false)
            {
                json = EmbeddedPlaneData.JsonContent;
            }
            else
            {
                Debug.Assert(false);
            }
#endif // @1
#endif

            if (string.IsNullOrEmpty(json) == true)
            {
                Debug.Assert(false);
                yield break;
            }

            ParseJsonData(json);

            OnDataLoaded?.Invoke();
            Debug.LogFormat(LOG_FORMAT, $"<color=cyan>JSON 로드 완료: R 계열 면 {_R_Faces.Length}개, W 계열 면 {_W_Faces.Count}개, Edge {_Edge_List.Count}개</color>");
        }

        protected void ParseJsonData(string jsonContent)
        {
            Debug.LogFormat(LOG_FORMAT, "+++++ ParseJsonData() +++++");

            _W_Faces.Clear();
            _W_Face_List.Clear();
            _Edge_List.Clear();

            List<Cube> rFaceList = new List<Cube>();

            CustomRoot root = JsonUtility.FromJson<CustomRoot>(jsonContent);

            FieldInfo[] fields = typeof(CustomRoot).GetFields();
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(CustomQuad))
                {
                    CustomQuad quad = (CustomQuad)field.GetValue(root);
                    if (quad != null && quad.top_left != null)
                    {
                        Cube c = new Cube
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
                            if (_W_Faces.ContainsKey(c.Name) == false)
                            {
                                _W_Faces.Add(c.Name, c);
                                _W_Face_List.Add(c);
                            }
                        }
                        else if (c.Name.StartsWith("R", StringComparison.OrdinalIgnoreCase))
                        {
                            rFaceList.Add(c);
                        }

                        // <<< NEW: R7의 Edge 데이터 처리 로직 >>>
                        if (c.Name == "R7")
                        {
                            // edge_1 데이터가 유효하면 Edge 객체 생성 후 리스트에 추가
                            if (quad.edge_1_start != null && quad.edge_1_end != null)
                            {
                                _Edge_List.Add(new Edge
                                {
                                    Name = "R7_Edge_1",
                                    Start = ConvertToUnity(quad.edge_1_start),
                                    End = ConvertToUnity(quad.edge_1_end)
                                });
                            }
                            // edge_2 데이터 처리
                            if (quad.edge_2_start != null && quad.edge_2_end != null)
                            {
                                _Edge_List.Add(new Edge
                                {
                                    Name = "R7_Edge_2",
                                    Start = ConvertToUnity(quad.edge_2_start),
                                    End = ConvertToUnity(quad.edge_2_end)
                                });
                            }
                            // edge_3 데이터 처리
                            if (quad.edge_3_start != null && quad.edge_3_end != null)
                            {
                                _Edge_List.Add(new Edge
                                {
                                    Name = "R7_Edge_3",
                                    Start = ConvertToUnity(quad.edge_3_start),
                                    End = ConvertToUnity(quad.edge_3_end)
                                });
                            }
                            // edge_4 데이터 처리
                            if (quad.edge_4_start != null && quad.edge_4_end != null)
                            {
                                _Edge_List.Add(new Edge
                                {
                                    Name = "R7_Edge_4",
                                    Start = ConvertToUnity(quad.edge_4_start),
                                    End = ConvertToUnity(quad.edge_4_end)
                                });
                            }
                        }
                    }
                }
            }

            _R_Faces = rFaceList.ToArray();

            IsDataLoaded = true;
            Debug.LogFormat(LOG_FORMAT, "----- ParseJsonData() -----");
        }
    }
}