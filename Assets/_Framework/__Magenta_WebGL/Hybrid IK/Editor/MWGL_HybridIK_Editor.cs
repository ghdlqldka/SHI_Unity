﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace _Magenta_WebGL
{
    [CustomEditor(typeof(MWGL_HybridIK))]
    public class MWGL_HybridIK_Editor : _HybridIK_Editor
    {
        private static string LOG_FORMAT = "<color=#F85AA7><b>[MWGL_HybridIK_Editor]</b></color> {0}";

        // protected SerializedProperty m_Script;

        protected new MWGL_HybridIK hybridIKNode
        {
            get
            {
                return target as MWGL_HybridIK;
            }
        }

        protected override void OnEnable()
        {
            // m_Script = serializedObject.FindProperty("m_Script");
        }

        protected override void AddJointsPoseKeyframe()
        {
            List<int> keyframeState = new List<int>();
            foreach (HybridIKJoint j in hybridIKNode.nodes)
            {
                // keyframeState.Add(j.enableKeyframeConstraints ? 0 : j.overrideConstraint ? 1 : 2);
                if (j.enableKeyframeConstraints == true)
                {
                    keyframeState.Add(0);
                }
                else
                {
                    if (j.overrideConstraint == true)
                    {
                        keyframeState.Add(1);
                    }
                    else
                    {
                        keyframeState.Add(2);
                    }
                }
                j.enableKeyframeConstraints = true; j.overrideConstraint = false;
            }

            hybridIKNode.ReprocessJoints();

            HybridIKJoint root = hybridIKNode.nodes[0];
            Transform parent = root.jointTransform.parent;

            foreach (HybridIKJoint node in hybridIKNode.nodes)
            {
                // encode rotation
                if (node.keyedRotations == null)
                    node.keyedRotations = new List<Quaternion>();
                node.keyedRotations.Add(node.jointTransform.localRotation);

                //encode local positions used for stretch
                if (node.keyedLocalPositions == null)
                    node.keyedLocalPositions = new List<Vector3>();
                node.keyedLocalPositions.Add(node.jointTransform.localPosition);

                // encode position in parent space
                if (node.keyedPositions == null)
                    node.keyedPositions = new List<Vector3>();
                node.keyedPositions.Add(parent != null ? parent.InverseTransformPoint(node.GetCurrentPositionWorld()) : node.GetCurrentPositionWorld());

                HybridIKConstraint constraint = hybridIKNode.constraints.Find(c => c.jointTransform == node.jointTransform);
                if (constraint == null)
                {
                    constraint = new HybridIKConstraint();
                    constraint.jointTransform = node.jointTransform;
                    constraint.constrainPosition = true;
                    hybridIKNode.constraints.Add(constraint);
                }
            }

            foreach (HybridIKConstraint c in hybridIKNode.constraints)
            {
                ConstraintPositionKey key = new ConstraintPositionKey();
                if (c.positionKeys == null)
                    c.positionKeys = new List<ConstraintPositionKey>();
                key.constraintPositionValue = c.jointTransform.position;

                Vector3 endPos = parent != null ? parent.InverseTransformPoint(hybridIKNode.endNode.position) : hybridIKNode.endNode.position;
                key.SetEndTargetPosition(hybridIKNode.endNode.position, parent);
                c.positionKeys.Add(key);
            }
            hybridIKNode.ResetToZeroPose();
        }

        protected override void HandleSelectedIKNodeIndex()
        {
            GUIStyle richTextLabelStyle = new GUIStyle(GUI.skin.label);
            richTextLabelStyle.richText = true;

            HybridIKJoint _selectedNode = hybridIKNode.nodes[selectedIKNodeIndex];
            Debug.Assert(_selectedNode is _HybridIKJoint);
            _HybridIKJoint selectedNode = _selectedNode as _HybridIKJoint;

            GUILayout.BeginHorizontal();
            if (selectedNode.jointTransform != null)
            {
                if (GUILayout.Button("Select Joint GameObject"))
                {
                    Selection.activeGameObject = selectedNode.jointTransform.gameObject;
                }
            }
            if (GUILayout.Button("Select Full Chain"))
            {
                selectedIKNodeIndex = -1;
            }
            GUILayout.EndHorizontal();

            if (selectedIKNodeIndex >= 0)
            {
                GUILayout.Label("IK Node <color=red>" + selectedIKNodeIndex.ToString() + "</color> selected", richTextLabelStyle);
                selectedNode.jointRadius = EditorGUILayout.FloatField("Node Radius", selectedNode.jointRadius);
                var oldColor = GUI.backgroundColor;
                GUILayout.Label("CONSTRAINTS MODE");
                GUILayout.BeginHorizontal();
                if (selectedNode.enableKeyframeConstraints && !selectedNode.overrideConstraint)
                    GUI.backgroundColor = Color.green;
                if (GUILayout.Button("KEYFRAMED"))
                {
                    selectedNode.enableKeyframeConstraints = true;
                    selectedNode.overrideConstraint = false;
                    hybridIKNode.ReprocessJoints();
                }
                GUI.backgroundColor = oldColor;
                if (selectedNode.overrideConstraint)
                    GUI.backgroundColor = Color.green;
                if (GUILayout.Button("MANUAL CONSTRAINT"))
                {
                    Debug.LogFormat(LOG_FORMAT, "MANUAL CONSTRAINT pressed!!!!!");

                    selectedNode.enableKeyframeConstraints = false;
                    selectedNode.overrideConstraint = true;
                    hybridIKNode.ReprocessJoints();
                }
                GUI.backgroundColor = oldColor;
                if (!selectedNode.enableKeyframeConstraints && !selectedNode.overrideConstraint)
                    GUI.backgroundColor = Color.green;
                if (GUILayout.Button("NONE"))
                {
                    selectedNode.enableKeyframeConstraints = false;
                    selectedNode.overrideConstraint = false;
                    hybridIKNode.ReprocessJoints();
                }
                GUILayout.EndHorizontal();
                GUI.backgroundColor = oldColor;

                if (selectedNode.overrideConstraint)
                {
                    //n.position = EditorGUILayout.Vector3Field("Target Position", node.position);
                    selectedNode.constraint.jointTransform = selectedNode.jointTransform;
                    selectedNode.constraint.targetTransform = EditorGUILayout.ObjectField("Target Transform", selectedNode.constraint.targetTransform, typeof(Transform), true) as Transform;

                    selectedNode.constraint.constrainPosition = EditorGUILayout.Toggle("Constrain Position", selectedNode.constraint.constrainPosition);
                    selectedNode.constraint.constrainRotation = EditorGUILayout.Toggle("Constrain Orientation", selectedNode.constraint.constrainRotation);
                }

                selectedNode.enableStretch = EditorGUILayout.Toggle("Enable Stretch", selectedNode.enableStretch);
                if (selectedNode.enableStretch)
                {
                    if (selectedNode.stretchLimits == null)
                    {
                        selectedNode.stretchLimits = new HybridIKNodeStretchLimits();
                    }

                    selectedNode.stretchLimits.targetSpace = EditorGUILayout.ObjectField("Target Transform", selectedNode.stretchLimits.targetSpace, typeof(Transform), true) as Transform;

                    selectedNode.stretchLimits.minStretchLimits = EditorGUILayout.Vector3Field("Min Stretch Limits", selectedNode.stretchLimits.minStretchLimits);
                    selectedNode.stretchLimits.maxStretchLimits = EditorGUILayout.Vector3Field("Max Stretch Limits", selectedNode.stretchLimits.maxStretchLimits);
                }

                if (GUI.changed)
                    EditorUtility.SetDirty(hybridIKNode);
                return;
            }
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            /*
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();
            */

            // base.OnInspectorGUI();

            // base.OnInspectorGUI();
            DrawDefaultInspector();

            if (hybridIKNode.nodes == null)
                return;

            if (hybridIKNode.constraints == null || hybridIKNode.constraints.Count == 0)
            {
                hybridIKNode.constraints = new List<HybridIKConstraint>();
                foreach (HybridIKJoint node in hybridIKNode.nodes)
                {
                    node.keyedLocalPositions = new List<Vector3>();
                    node.keyedPositions = new List<Vector3>();
                    node.keyedRotations = new List<Quaternion>();
                }
            }

            if (hybridIKNode.constraints != null)// && hybridIKNode.constraints.Count > 0)
            {
                //EditorGUILayout.LabelField("Keyframe Constraints", EditorStyles.boldLabel);
                if (GUILayout.Button("Add Joints Pose Keyframe"))
                {
                    AddJointsPoseKeyframe();

                }

                if (hybridIKNode.nodes != null && hybridIKNode.nodes.Count > 0 && hybridIKNode.nodes[0] != null && hybridIKNode.nodes[0].keyedPositions != null)
                {
                    for (int p = 0; p < hybridIKNode.nodes[0].keyedPositions.Count; ++p)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Key Pose " + (p + 1).ToString());
                        if (GUILayout.Button("Select"))
                        {
                            //selectedIKKeyframe = p;
                            hybridIKNode.PoseToKeyframe(p);
                            Repaint();
                        }
                        if (GUILayout.Button("Delete Key"))
                        {
                            DeletePoseKeyframe(p);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }

            if (selectedIKKeyframe >= 0)
            {
                if (GUILayout.Button("Select Full Chain"))
                {
                    selectedIKKeyframe = -1;
                }

                if (selectedIKKeyframe >= 0)
                {
                    GUILayout.Label("IK Keyframe " + selectedIKKeyframe.ToString() + " selected");
                    if (GUILayout.Button("Delete Pose Keyframe"))
                    {
                        DeletePoseKeyframe(selectedIKKeyframe);
                    }
                    return;
                }
            }

            if (selectedIKNodeIndex >= 0)
            {
                HandleSelectedIKNodeIndex();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Process IK Joint Chain"))
            {
                Debug.LogFormat(LOG_FORMAT, "Process IK Joint Chain Button Clicked!!!!!!!!!");

                hybridIKNode.ResetAll();
                hybridIKNode.ProcessChain();
            }

            if (hybridIKNode.nodes != null)
            {
                if (GUILayout.Button("Set Zero Rotations to Current Pose"))
                {
                    foreach (HybridIKJoint node in hybridIKNode.nodes)
                    {
                        node.zeroRotation = node.jointTransform.localRotation;
                        node.zeroPosition = node.jointTransform.localPosition;
                    }
                }
            }

            if (GUI.changed)
                EditorUtility.SetDirty(hybridIKNode);
        }

        public override void OnSceneGUI()
        {
            // Debug.LogFormat(LOG_FORMAT, "OnSceneGUI()");

            if (hybridIKNode.nodes == null || hybridIKNode.nodes.Count == 0)
                return;

            Color originalColor = Handles.color;
            if (hybridIKNode.IsInitialized())
            {
                for (int i = 0; i < hybridIKNode.nodes.Count; ++i)
                //foreach (SweepIKNode n in hybridIKNode.nodes)
                {
                    HybridIKJoint n = hybridIKNode.nodes[i];
                    Color c = n.enableKeyframeConstraints ? Color.blue : n.overrideConstraint ? Color.cyan : originalColor;
                    Handles.color = selectedIKNodeIndex == i ? Color.green : c;
                    float nRadius = Mathf.Max(n.jointRadius, 0.05f, n.jointRadius);
                    if (Handles.Button(n.GetCurrentPositionWorld(), Quaternion.identity, nRadius, nRadius, Handles.SphereHandleCap))
                    {
                        selectedIKNodeIndex = i;
                        selectedIKKeyframe = -1;
                        if (!Application.isPlaying)
                            hybridIKNode.ResetToZeroPose();
                        Repaint();
                    }
                }

                HybridIKJoint root = hybridIKNode.GetRootIKNode();
                HybridIKJoint endIKNode = hybridIKNode.GetEndIKNode();

                Transform parentSpace = root.jointTransform.parent;

                foreach (HybridIKConstraint c in hybridIKNode.constraints)
                {
                    if (c.positionKeys.Count > 0)
                    {
                        for (int i = 0; i < c.positionKeys.Count; ++i)
                        {
                            ConstraintPositionKey k = c.positionKeys[i];
                            Handles.color = selectedIKKeyframe == i ? Color.green : originalColor;
                            if (i % 4 == 0) Handles.color = Color.green;
                            if (i % 4 == 1) Handles.color = Color.blue;
                            if (i % 4 == 2) Handles.color = Color.yellow;
                            if (i % 4 == 3) Handles.color = Color.magenta;

                            Vector3 buttonPos = k.GetEndTargetPosition(endIKNode.targetIKSpace != null ? endIKNode.targetIKSpace : parentSpace);

                            if (Handles.Button(buttonPos, Quaternion.identity, endIKNode.jointRadius, endIKNode.jointRadius, Handles.SphereHandleCap))
                            {
                                selectedIKKeyframe = i;
                                selectedIKNodeIndex = -1;

                                hybridIKNode.PoseToKeyframe(selectedIKKeyframe);
                                Repaint();
                            }
                        }
                    }
                }
            }
            Handles.color = originalColor;
        }
    }
}