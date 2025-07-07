using UnityEngine;
using UnityEditor;

namespace BioIK 
{
    [CustomEditor(typeof(_BioIK))]
    public class _BioIKEditor : BioIKEditor
    {
        protected SerializedProperty m_Script;

        // public BioIK Target;
        protected _BioIK _Target
        {
            get
            {
                return Target as _BioIK;
            }
        }

        protected override void Awake()
        {
            // #pragma warning disable 0618
            // EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            // #pragma warning restore 0618
            Target = (_BioIK)target;
            TargetTransform = _Target.transform;
            _Target.Refresh(false);
            DoF = 0;
            //MakeVisible(TargetTransform);
            //MakeInvisible(TargetTransform);
        }

        protected override void PlaymodeStateChanged()
        {
            throw new System.NotSupportedException("");
        }

        protected void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            // throw new System.NotImplementedException();
            IsPlaying = Application.isPlaying;
        }

        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            // base.OnEnable();
            IsEnabled = true;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            // base.OnInspectorGUI();
            Undo.RecordObject(_Target, _Target.name);

            SetGUIColor(Color3);
            using (new EditorGUILayout.VerticalScope("Button"))
            {
                SetGUIColor(Color5);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.HelpBox("                    Settings                    ", MessageType.None);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                //SetGUIColor(Color1);
                //_Target.SolveInEditMode = EditorGUILayout.Toggle("Solve In Edit Mode", _Target.SolveInEditMode);
                SetGUIColor(Color1);
                // In Unity WebGL, multiple threads cannot be used by default.
                // _Target.SetThreading(EditorGUILayout.Toggle("Use Threading", _Target.GetThreading()));
                SetGUIColor(Color1);
                _Target.SetGenerations(EditorGUILayout.IntField("Generations", _Target.GetGenerations()));
                SetGUIColor(Color1);
                _Target.SetPopulationSize(EditorGUILayout.IntField("Individuals", _Target.GetPopulationSize()));
                SetGUIColor(Color1);
                _Target.SetElites(EditorGUILayout.IntField("Elites", _Target.GetElites()));
                SetGUIColor(Color1);
                _Target.Smoothing = EditorGUILayout.Slider("Smoothing", _Target.Smoothing, 0f, 1f);
                SetGUIColor(Color1);
                _Target.AnimationWeight = EditorGUILayout.Slider("Animation Weight", _Target.AnimationWeight, 0f, 1f);
                SetGUIColor(Color1);
                _Target.AnimationBlend = EditorGUILayout.Slider("Animation Blend", _Target.AnimationBlend, 0f, 1f);
                SetGUIColor(Color1);
                _Target.MotionType = (MotionType)EditorGUILayout.EnumPopup("Motion Type", _Target.MotionType);
                if (_Target.MotionType == MotionType.Realistic)
                {
                    _Target.MaximumVelocity = EditorGUILayout.FloatField("Maximum Velocity", _Target.MaximumVelocity);
                    _Target.MaximumAcceleration = EditorGUILayout.FloatField("Maximum Acceleration", _Target.MaximumAcceleration);
                }
                SetGUIColor(Color1);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("          Reset Posture          "))
                {
                    _Target.ResetPosture(_Target.Root);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            SetGUIColor(Color3);
            using (new EditorGUILayout.VerticalScope("Button"))
            {
                SetGUIColor(Color5);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.HelpBox("                   Character                   ", MessageType.None);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                //SetGUIColor(Color5);
                //EditorGUILayout.HelpBox("Degree of Freedom: " + new Model(_Target).GetDoF() + " / " + DoF, MessageType.None);

                int maxIndent = 0;
                ComputeMaxIndentLevel(_Target.transform, 0, ref maxIndent);

                _Target.Scroll = EditorGUILayout.BeginScrollView(_Target.Scroll, GUILayout.Height(500f));
                InspectBody(_Target.FindSegment(_Target.transform), 0, maxIndent);
                EditorGUILayout.EndScrollView();
            }

            SetGUIColor(Color1);
            if (_Target != null)
            {
                EditorUtility.SetDirty(_Target);
            }
        }

        protected override void MakeInvisible(Transform t)
        {
            Debug.LogError("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            if (t.GetComponent<BioSegment>())
            {
                t.GetComponent<BioSegment>().hideFlags = HideFlags.HideInInspector;
            }
            foreach (BioObjective o in t.GetComponents<BioObjective>())
            {
                o.hideFlags = HideFlags.HideInInspector;
            }
            if (t.GetComponent<BioJoint>())
            {
                t.GetComponent<BioJoint>().hideFlags = HideFlags.HideInInspector;
            }
            for (int i = 0; i < t.childCount; i++)
            {
                MakeInvisible(t.GetChild(i));
            }
        }
    }
}
