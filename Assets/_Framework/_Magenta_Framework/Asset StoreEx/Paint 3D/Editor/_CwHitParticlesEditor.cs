using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using PaintCore;

#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = _CwHitParticles;

	[CustomEditor(typeof(_CwHitParticles))]
	public class _CwHitParticlesEditor : CwHitParticles_Editor
    {
        protected SerializedProperty m_Script;

        protected virtual void OnEnable()
        {
			// base.OnEnable();
            m_Script = serializedObject.FindProperty("m_Script");
        }

        protected override void OnInspector()
		{
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            _CwHitParticles tgt;
            _CwHitParticles[] tgts; 
			GetTargets(out tgt, out tgts);

			Draw("emit", "This allows you to control the hit data this component sends out.\n\nPoints = Point drawing in 3D.\n\nPointsOnUV = Point drawing on UV (requires non-convex MeshCollider).\n\nTrianglesIn3D = Triangle drawing in 3D.");
			if (Any(tgts, t => t.Emit != CwHitParticles.EmitType.PointsIn3D))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.RaycastDistance <= 0.0f));
						Draw("raycastDistance", "When emitting PointsOnUV or TrianglesIn3D, this setting allows you to specify the world space distance from the hit point a raycast will be fired. This is necessary because particles by themselves don't provide the necessary information.\n\nNOTE: Performing this raycast has a slight performance penalty.");
					EndError();
				EndIndent();
			}
			Draw("layers", "This allows you to filter collisions to specific layers.");

			Separator();

			Draw("orientation", "How should the hit point be oriented?\n\nNone = It will be treated as a point with no rotation.\n\nWorldUp = It will be rotated to the normal, where the up vector is world up.\n\nCameraUp = It will be rotated to the normal, where the up vector is world up.");
			BeginIndent();
				if (Any(tgts, t => t.Orientation == CwHitParticles.OrientationType.CameraUp))
				{
					Draw("_camera", "Orient to a specific camera?\n\nNone = MainCamera.");
				}
			EndIndent();
			Draw("normal", "Which normal should the hit point rotation be based on?");

			Separator();
			
			Draw("preview", "Should the particles paint preview paint?");
			Draw("pressureMode", "This allows you to set how the pressure value will be calculated.\n\nConstant = The PressureConstant value will be directly used.\n\nDistance = A value will be calculated based on the distance between this emitter and the particle hit point.\n\nVelocity = A value will be calculated based on the hit velocity of the particle.");
			BeginIndent();
				if (Any(tgts, t => t.PressureMode == CwHitParticles.PressureType.Constant))
				{
					Draw("pressureConstant", "The pressure value used when PressureMode is set to Constant.", "Constant");
				}
				if (Any(tgts, t => t.PressureMode == CwHitParticles.PressureType.Distance))
				{
					Draw("pressureMin", "This allows you to set the world space distance from this emitter where the particle hit point will register as having 0.0 pressure.", "Min");
					Draw("pressureMax", "This allows you to set the world space distance from this emitter where the particle hit point will register as having 1.0 pressure.", "Max");
				}
				else if (Any(tgts, t => t.PressureMode == CwHitParticles.PressureType.Speed))
				{
					Draw("pressureMin", "This allows you to set the particle speed where the hit will register as having 0.0 pressure.", "Min");
					Draw("pressureMax", "This allows you to set the particle speed where the hit will register as having 1.0 pressure.", "Max");
				}
				Draw("pressureMultiplier", "The calculated pressure value will be multiplied by this.", "Multiplier");
			EndIndent();

			Separator();

			if (DrawFoldout("Advanced", "Show advanced settings?") == true)
			{
				BeginIndent();
					Draw("skip", "If you have too many particles, then painting can slow down. This setting allows you to reduce the amount of particles that actually cause hits.\n\n0 = Every particle will hit.\n\n5 = Skip 5 particles, then hit using the 6th.");
					Draw("offset", "If you want the raycast hit point to be offset from the surface a bit, this allows you to set by how much in world space.");
					Draw("priority", "This allows you to override the order this paint gets applied to the object during the current frame.");
					Draw("root", "Hit events are normally sent to all components attached to the current GameObject, but this setting allows you to override that. This is useful if you want to use multiple CwHitCollisions components with different settings and results.");
				EndIndent();
			}

			Separator();

			var point    = tgt.Emit == CwHitParticles.EmitType.PointsIn3D;
			var triangle = tgt.Emit == CwHitParticles.EmitType.TrianglesIn3D;
			var coord    = tgt.Emit == CwHitParticles.EmitType.PointsOnUV;
			GameObject _obj = tgt.gameObject;
			if (tgt.Root != null)
			{
                _obj = tgt.Root;
            }
            // tgt.HitCache.Inspector(tgt.Root != null ? tgt.Root : tgt.gameObject, point: point, triangle: triangle, coord: coord);
            tgt.HitCache.Inspector(_obj, point: point, triangle: triangle, coord: coord);
        }
	}
}
#endif