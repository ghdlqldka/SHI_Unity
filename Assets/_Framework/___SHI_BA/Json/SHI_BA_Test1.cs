using System.Collections;
using System.Collections.Generic;
using BioIK;
using EA.Line3D;
using UnityEngine;
using static _SHI_BA.BA_Motion;

namespace _SHI_BA
{
    public class SHI_BA_Test1 : MonoBehaviour
    {
        [SerializeField]
        protected BA_LoadPlaneJson loadPlaneJson;

        [Header("��� ���� ����")]
        [Tooltip("���� ���� �� ���� (�ּ� 2)")]
        public int numHorizontalPoints = 2;
        [Tooltip("���� ���� �̵� �Ÿ�(m)")]
        private float verticalStep = 0.08f;
        [Tooltip("��ο� �۾��� ������ �Ÿ�(m)")]
        public float zigzagNormalOffset = 0.6f;

        [Header("��ֹ� ȸ�� ����")]
        [Tooltip("��ֹ� Collider�� �� ������ŭ Ű���� �����Ÿ��� Ȯ���մϴ�.")]
        private float obstacleScaleFactor = 1.1f;
        [Tooltip("��ֹ� ǥ�鿡�� �� �Ÿ�(m)��ŭ ��θ� ���ϴ�.")]
        private float obstacleOffset = 0.00f;

        [Header("��ֹ� ����")]
        public List<Collider> obstacleColliders = new List<Collider>();


        [Header("Result")]
        [SerializeField]
        protected BA_GeneratedData _generatedData = new BA_GeneratedData();

        // ++++++
        public GameObject TCP;
        private BA_MotionPathGenerator pathGenerator;

        [SerializeField]
        protected List<BA_LineRenderer3D_Gizmo> lineList;

        protected virtual void Awake()
        {
            pathGenerator = new BA_MotionPathGenerator(TCP.transform);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected virtual void Start()
        {
            Debug.Assert(loadPlaneJson != null);

            for (int i = 0; i < lineList.Count; i++)
            {
                lineList[i].gameObject.SetActive(false);
            }

            StartCoroutine(PostStart());
        }

        protected IEnumerator PostStart()
        {
            while (loadPlaneJson.IsDataLoaded == false)
            {
                yield return null;
            }

            var config = new PathGenerationConfig
            {
                NumHorizontalPoints = this.numHorizontalPoints,
                VerticalStep = this.verticalStep,
                NormalOffset = this.zigzagNormalOffset,
                ObstacleOffset = obstacleOffset,
                ObstacleScaleFactor = obstacleScaleFactor
            };

            List<BA_MotionPath> orderedPaths = new List<BA_MotionPath>();
            Debug.Log("��� ���� ������ ����: W1 ���� ���� �����մϴ�.");
            int rFaceIndexOffset = 0;

            // 2. W1 ���� ��θ� ���� �����մϴ�.
            if (loadPlaneJson.W_Faces.TryGetValue("W1", out Cube w1Face))
            {
                BA_PathDataManager.Instance.w1Path = pathGenerator.GenerateWeavingPath(w1Face, config);
                if (BA_PathDataManager.Instance.w1Path != null)
                {
                    // 3. ������ W1 ��θ� ���� ����Ʈ�� ù ��°�� �߰��ϰ�, �������� �����մϴ�.
                    orderedPaths.Add(BA_PathDataManager.Instance.w1Path);
                    rFaceIndexOffset = BA_PathDataManager.Instance.w1Path.FaceIndex;
                    Debug.Log($"W1 �� ��ΰ� ����Ʈ�� ù ��°�� �߰��Ǿ����ϴ� (FaceIndex: {BA_PathDataManager.Instance.w1Path.FaceIndex}). R �迭 ���� ������: {rFaceIndexOffset}");
                }
                else
                {
                    Debug.Assert(false);
                }
            }

            // 4. ���� R �迭 ����� ��θ� �����մϴ�.
            Vector3 robotInitPos = Vector3.zero;
            BA_PathDataManager.Instance.rPaths = pathGenerator.GenerateAllPaths(loadPlaneJson.R_Faces, config, robotInitPos, obstacleColliders, rFaceIndexOffset);

            for (int i = 0; i < lineList.Count; i++)
            {
                lineList[i].PointList = BA_PathDataManager.Instance.rPaths[i].PointList;
                lineList[i].gameObject.SetActive(true);
            }

#if false //
            // 5. ������ R �迭 ��ε��� ���� ����Ʈ�� �߰��մϴ�.
            if (rPaths != null && rPaths.Count > 0)
            {
                orderedPaths.AddRange(rPaths);
            }

            // 6. ����������, ������ �����ĵ� ��� ����Ʈ�� `generatedPaths`�� �Ҵ��մϴ�.
            generatedPaths = orderedPaths;

            // [C] ���� ��� �ð�ȭ
            if (generatedPaths == null || generatedPaths.Count == 0)
            {
                Debug.LogError("[MainController] ��� ������ �����߽��ϴ�.");
                return;
            }

            foreach (var path in generatedPaths)
            {
                visualizer.VisualizePath(path);
            }

            Debug.Log($"[MainController] �� {generatedPaths.Count}���� �鿡 ���� ��� ���� �� �ð�ȭ �Ϸ�. (W1 ��ΰ� ���� ���� ���۵˴ϴ�)");
#endif





        }

        protected void Update()
        {
#if DEBUG
            if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                for (int i = 0; i < lineList.Count; i++)
                {
                    lineList[i].GizmoMode = !lineList[i].GizmoMode;
                }
            }
#endif
        }
    }
}