using UnityEngine;

#if UNITY_EDITOR
namespace _Magenta_WebGL
{
	using UnityEditor;
	using TARGET = MWGL_CwFollow;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(MWGL_CwFollow))]
	public class MWGL_CwFollowEditor : CW.Common._CwFollowEditor
    {
        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
        }

        protected override void OnInspector()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            // base.OnInspector();
            MWGL_CwFollow tgt;
            MWGL_CwFollow[] tgts; 
			GetTargets(out tgt, out tgts);

			Draw("follow", "What should this component follow?");
			if (Any(tgts, t => t.Follow == CW.Common.CwFollow.FollowType.TargetTransform))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.Target == null));
						Draw("target", "The transform that will be followed.");
					EndError();
				EndIndent();
			}
			Draw("damping", "How quickly this Transform follows the target.\n\n-1 = instant.");
			Draw("rotate", "Follow the target's rotation too?");
			Draw("ignoreZ", "Ignore Z axis for 2D?");
			Draw("followIn", "Where in the game loop should this component update?");

			Separator();

			Draw("localPosition", "This allows you to specify a positional offset relative to the Target transform.");
			Draw("localRotation", "This allows you to specify a rotational offset relative to the Target transform.");
		}
	}
}
#endif