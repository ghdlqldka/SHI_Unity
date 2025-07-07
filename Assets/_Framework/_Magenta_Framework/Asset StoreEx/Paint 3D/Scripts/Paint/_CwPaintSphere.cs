using UnityEngine;
using CW.Common;
using PaintCore;
using System.Collections.Generic;

namespace PaintIn3D
{
	/// <summary>This allows you to paint a sphere at a hit point. Hit points will automatically be sent by any <b>CwHit___</b> component on this GameObject, or its ancestors.</summary>
	// [HelpURL(CwCommon.HelpUrlPrefix + "CwPaintSphere")]
	// [AddComponentMenu(PaintCore.CwCommon.ComponentHitMenuPrefix + "Paint Sphere")]
	public class _CwPaintSphere : CwPaintSphere
    {
        private static string LOG_FORMAT = "<color=#00E1FF><b>[_CwPaintSphere]</b></color> {0}";

        // public CwPaintableTexture TargetTexture { set { targetTexture = value; } get { return targetTexture; } }
        protected new _CwPaintableTexture TargetTexture 
        { 
            set 
            { 
                targetTexture = value;
            } 
            get 
            { 
                return targetTexture as _CwPaintableTexture;
            }
        }

        protected virtual void Awake()
		{
			// base.Awake();
			Debug.LogFormat(LOG_FORMAT, "Awake(), BlendMode : <color=red><b>" + BlendMode.Index + 
                "</b></color>, Color : <color=cyan><b>" + Color +
                "</b></color>, TargetModel : " + TargetModel + ", TargetTexture : " + TargetTexture);
		}

        public override void HandleHitPoint(bool preview, int priority, float pressure, int seed, Vector3 position, Quaternion rotation)
        {
            Debug.LogFormat(LOG_FORMAT, "Handle<b><color=yellow>HitPoint</color></b>()");
            // base.HandleHitPoint(preview, priority, pressure, seed, position, rotation);

            if (Modifiers != null && Modifiers.Count > 0)
            {
                CwHelper.BeginSeed(seed);
                Modifiers.ModifyPosition(ref position, preview, pressure);
                CwHelper.EndSeed();
            }

            _CwCommandSphere.Instance.SetState(preview, priority);
            _CwCommandSphere.Instance.SetLocation(position);
			Debug.Assert(_CwCommandSphere.Instance is _CwCommandSphere);

            Vector3 worldSize = HandleHitCommon(preview, pressure, seed, rotation);
            float worldRadius = PaintCore.CwCommon.GetRadius(worldSize);
            Vector3 worldPosition = position;

            HandleMaskCommon(worldPosition);

			if (TargetModel == null || TargetModel is _CwPaintableMesh)
			{
                _CwPaintableManager.Instance.SubmitAll(_CwCommandSphere.Instance, 
					worldPosition, worldRadius, Layers, Group, 
					TargetModel as _CwPaintableMesh, TargetTexture);
            }
            else if (TargetModel == null || TargetModel is _CwPaintableAtlas)
            {
                _CwPaintableManager.Instance.SubmitAll(_CwCommandSphere.Instance,
                    worldPosition, worldRadius, Layers, Group,
                    TargetModel as _CwPaintableAtlas, TargetTexture);
            }
            else
            {
                Debug.Assert(false);
            }
        }

        public override void HandleHitLine(bool preview, int priority, float pressure, int seed, Vector3 position, Vector3 endPosition, Quaternion rotation, bool clip)
        {
            _CwCommandSphere.Instance.SetState(preview, priority);
            _CwCommandSphere.Instance.SetLocation(position, endPosition, clip: clip);

            var worldSize = HandleHitCommon(preview, pressure, seed, rotation);
            var worldRadius = PaintCore.CwCommon.GetRadius(worldSize, position, endPosition);
            var worldPosition = PaintCore.CwCommon.GetPosition(position, endPosition);

            HandleMaskCommon(worldPosition);

            _CwPaintableManager.Instance.SubmitAll(_CwCommandSphere.Instance, worldPosition, worldRadius, Layers, Group, TargetModel, TargetTexture);
        }

        /// <summary>This method paints all pixels between three points using the shape of a sphere.</summary>
        public override void HandleHitTriangle(bool preview, int priority, float pressure, int seed, Vector3 positionA, Vector3 positionB, Vector3 positionC, Quaternion rotation)
        {
            _CwCommandSphere.Instance.SetState(preview, priority);
            _CwCommandSphere.Instance.SetLocation(positionA, positionB, positionC);

            var worldSize = HandleHitCommon(preview, pressure, seed, rotation);
            var worldRadius = PaintCore.CwCommon.GetRadius(worldSize, positionA, positionB, positionC);
            var worldPosition = PaintCore.CwCommon.GetPosition(positionA, positionB, positionC);

            HandleMaskCommon(worldPosition);

            _CwPaintableManager.Instance.SubmitAll(_CwCommandSphere.Instance, worldPosition, worldRadius, Layers, Group, TargetModel, TargetTexture);
        }

        /// <summary>This method paints all pixels between two pairs of points using the shape of a sphere.</summary>
        public override void HandleHitQuad(bool preview, int priority, float pressure, int seed, Vector3 position, Vector3 endPosition, Vector3 position2, Vector3 endPosition2, Quaternion rotation, bool clip)
        {
            _CwCommandSphere.Instance.SetState(preview, priority);
            _CwCommandSphere.Instance.SetLocation(position, endPosition, position2, endPosition2, clip: clip);

            var worldSize = HandleHitCommon(preview, pressure, seed, rotation);
            var worldRadius = PaintCore.CwCommon.GetRadius(worldSize, position, endPosition, position2, endPosition2);
            var worldPosition = PaintCore.CwCommon.GetPosition(position, endPosition, position2, endPosition2);

            HandleMaskCommon(worldPosition);

            _CwPaintableManager.Instance.SubmitAll(_CwCommandSphere.Instance, worldPosition, worldRadius, Layers, Group, TargetModel, TargetTexture);
        }

        public override void HandleHitCoord(bool preview, int priority, float pressure, int seed, CwHit hit, Quaternion rotation)
        {
			Debug.LogFormat(LOG_FORMAT, "HandleHitCoord()");
            // base.HandleHitCoord(preview, priority, pressure, seed, hit, rotation);

            _CwPaintableAtlas model = hit.Transform.GetComponent<_CwPaintableAtlas>();

            if (model != null)
            {
                // List<CwPaintableTexture> paintableTextures = model.FindPaintableTextures(Group);
                List<_CwPaintableTexture> paintableTextures = model._FindPaintableTextures(Group);

                for (int i = paintableTextures.Count - 1; i >= 0; i--)
                {
                    _CwPaintableTexture paintableTexture = paintableTextures[i];
                    Vector2 coord = paintableTexture.GetCoord(ref hit);

                    if (Modifiers != null && Modifiers.Count > 0)
                    {
                        Vector3 position = (Vector3)coord;

                        CwHelper.BeginSeed(seed);
                        Modifiers.ModifyPosition(ref position, preview, pressure);
                        CwHelper.EndSeed();

                        coord = position;
                    }

                    _CwCommandSphere.Instance.SetState(preview, priority);
                    _CwCommandSphere.Instance.SetLocation(coord, in3D: false);

                    HandleHitCommon(preview, pressure, seed, rotation);

                    _CwCommandSphere.Instance.ClearMask();

                    _CwCommandSphere.Instance.ApplyAspect(paintableTexture.Current);

                    _CwPaintableManager.Instance.Submit(_CwCommandSphere.Instance, model, paintableTexture);
                }
            }
        }

        protected override void HandleMaskCommon(Vector3 worldPosition)
        {
            // base.HandleMaskCommon(worldPosition);

            if (FindMask == true)
            {
                CwMask mask = _CwMask.Find(worldPosition, Layers);

                if (mask != null)
                {
                    _CwCommandSphere.Instance.SetMask(mask.Matrix, mask.Texture, mask.Channel, mask.Invert, mask.Stretch);
                }
                else
                {
                    _CwCommandSphere.Instance.ClearMask();
                }
            }
            else
            {
                _CwCommandSphere.Instance.ClearMask();
            }

            if (FindDepthMask == true)
            {
                _CwCommandSphere.Instance.DepthMask = CwRenderDepth.Find();
            }
            else
            {
                _CwCommandSphere.Instance.DepthMask = null;
            }
        }
    }
}

#if false //
#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = CwPaintSphere;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwClickToPaintSphere_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Layers == 0 && t.TargetModel == null));
				Draw("layers", "Only the CwPaintable___ GameObjects whose layers are within this mask will be eligible for painting.");
			EndError();
			Draw("group", "Only the CwPaintableTexture components with a matching group will be painted by this component.");

			Separator();

			Draw("blendMode", "This allows you to choose how the paint from this component will combine with the existing pixels of the textures you paint.\n\nNOTE: See the Blend Mode section of the documentation for more information.");
			Draw("color", "The color of the paint.");
			Draw("opacity", "The opacity of the brush.");

			Separator();

			Draw("angle", "The angle of the paint in degrees.\n\nNOTE: This is only useful if you change the Scale.x/y values.");
			Draw("scale", "By default this component paints using a sphere shape, but you can override this here to paint an ellipsoid.\n\nNOTE: When painting an ellipsoid, the orientation of the sphere matters. This can be controlled from the CwHit__ component settings.");
			Draw("radius", "The radius of the paint brush.");
			Draw("hardness", "This allows you to control the sharpness of the near+far depth cut-off point.");

			Separator();

			if (DrawFoldout("Advanced", "Show advanced settings?") == true)
			{
				BeginIndent();
					Draw("targetModel", "If this is set, then only the specified model will be painted, regardless of the layer setting.");
					Draw("targetTexture", "If this is set, then only the specified CwPaintableTexture will be painted, regardless of the layer or group setting.");
					Draw("findMask", "If your scene contains a <b>CwMask</b>, should this paint component use it?");
					Draw("findDepthMask", "If your scene contains a <b>CwRenderDepth</b>, should this paint component use it?");

					Separator();

					Draw("tileTexture", "This allows you to apply a tiled detail texture to your decals. This tiling will be applied in world space using triplanar mapping.");
					Draw("tileTransform", "This allows you to adjust the tiling position + rotation + scale using a Transform.");
					Draw("tileOpacity", "This allows you to control the triplanar influence.\n\n0 = No influence.\n\n1 = Full influence.");
					Draw("tileTransition", "This allows you to control how quickly the triplanar mapping transitions between the X/Y/Z planes.");
				EndIndent();
			}

			Separator();

			tgt.Modifiers.DrawEditorLayout(serializedObject, target, "Color", "Opacity", "Radius", "Scale", "Hardness", "Position");
		}
	}
}
#endif
#endif