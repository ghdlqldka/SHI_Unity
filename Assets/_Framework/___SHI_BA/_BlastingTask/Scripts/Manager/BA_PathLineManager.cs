using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace _SHI_BA
{
    public class BA_PathLineManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#FFC300><b>[BA_PathLineManager]</b></color> {0}";

        private static BA_PathLineManager _instance;
        public static BA_PathLineManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        [SerializeField]
        [SerializedDictionary("Path Name", "LineRenderer3D")]
        protected SerializedDictionary<string, BA_LineRenderer3D_Gizmo> _lineRendererDic = new SerializedDictionary<string, BA_LineRenderer3D_Gizmo>();
        public SerializedDictionary<string, BA_LineRenderer3D_Gizmo> LineRendererDic
        {
            get
            {
                return _lineRendererDic;
            }
        }

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            if (Instance == null)
            {
                Instance = this;

                foreach (KeyValuePair<string, BA_LineRenderer3D_Gizmo> pair in LineRendererDic)
                {
                    BA_LineRenderer3D_Gizmo renderer = pair.Value;

                    renderer.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }
    }
}