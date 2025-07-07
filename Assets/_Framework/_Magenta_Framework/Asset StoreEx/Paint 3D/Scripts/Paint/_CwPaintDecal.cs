using UnityEngine;
using CW.Common;
using PaintCore;
using System.Collections.Generic;

namespace PaintIn3D
{
	/// <summary>This allows you to paint a decal at a hit point. Hit points will automatically be sent by any <b>CwHit___</b> component on this GameObject, or its ancestors.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwPaintDecal")]
	// [AddComponentMenu(PaintCore.CwCommon.ComponentHitMenuPrefix + "Paint Decal")]
	public class _CwPaintDecal : CwPaintDecal
    {
        private static string LOG_FORMAT = "<color=#00E1FF><b>[_CwPaintDecal]</b></color> {0}";

        // public Color Color { set { color = value; } get { return color; } }
        public Color _Color { set { color = value; } get { return color; } }

        // public Texture Texture { set { texture = value; } get { return texture; } }
        protected Texture _Texture { set { texture = value; } get { return texture; } }

        // public CwPaintableTexture TargetTexture { set { targetTexture = value; } get { return targetTexture; } }
        protected new _CwPaintableTexture TargetTexture 
        { 
            set { targetTexture = value; } 
            get { return targetTexture as _CwPaintableTexture; } 
        }
        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");
        }

        public override void HandleHitPoint(bool preview, int priority, float pressure, int seed, 
            Vector3 position, Quaternion rotation)
        {
            // base.HandleHitPoint(preview, priority, pressure, seed, position, rotation);

            if (Modifiers != null && Modifiers.Count > 0)
            {
                CwHelper.BeginSeed(seed);
                Modifiers.ModifyPosition(ref position, preview, pressure);
                CwHelper.EndSeed();
            }

            _CwCommandDecal.Instance.SetState(preview, priority);
            _CwCommandDecal.Instance.SetLocation(position);

            var worldSize = HandleHitCommon(preview, pressure, seed, rotation);
            var worldRadius = PaintCore.CwCommon.GetRadius(worldSize);
            var worldPosition = position;

            HandleMaskCommon(worldPosition);

            _CwPaintableManager.Instance.SubmitAll(_CwCommandDecal.Instance, worldPosition, worldRadius, 
                Layers, Group, TargetModel, TargetTexture);
        }

        /// <summary>This method paints all pixels between the two specified points using the shape of a decal.</summary>
        public override void HandleHitLine(bool preview, int priority, float pressure, int seed, 
            Vector3 position, Vector3 endPosition, Quaternion rotation, bool clip)
        {
            // base.HandleHitLine(preview, priority, pressure, seed, position, endPosition, rotation, clip);

            _CwCommandDecal.Instance.SetState(preview, priority);
            _CwCommandDecal.Instance.SetLocation(position, endPosition, clip: clip);

            var worldSize = HandleHitCommon(preview, pressure, seed, rotation);
            var worldRadius = PaintCore.CwCommon.GetRadius(worldSize, position, endPosition);
            var worldPosition = PaintCore.CwCommon.GetPosition(position, endPosition);

            HandleMaskCommon(worldPosition);

            _CwPaintableManager.Instance.SubmitAll(_CwCommandDecal.Instance, worldPosition, worldRadius,
                Layers, Group, TargetModel, TargetTexture);
        }

        /// <summary>This method paints all pixels between three points using the shape of a decal.</summary>
        public override void HandleHitTriangle(bool preview, int priority, float pressure, int seed,
            Vector3 positionA, Vector3 positionB, Vector3 positionC, Quaternion rotation)
        {
            // base.HandleHitTriangle(preview, priority, pressure, seed, positionA, positionB, positionC, rotation);

            _CwCommandDecal.Instance.SetState(preview, priority);
            _CwCommandDecal.Instance.SetLocation(positionA, positionB, positionC);

            var worldSize = HandleHitCommon(preview, pressure, seed, rotation);
            var worldRadius = PaintCore.CwCommon.GetRadius(worldSize, positionA, positionB, positionC);
            var worldPosition = PaintCore.CwCommon.GetPosition(positionA, positionB, positionC);

            HandleMaskCommon(worldPosition);

            _CwPaintableManager.Instance.SubmitAll(_CwCommandDecal.Instance, worldPosition, worldRadius,
                Layers, Group, TargetModel, TargetTexture);
        }

        /// <summary>This method paints all pixels between two pairs of points using the shape of a decal.</summary>
        public override void HandleHitQuad(bool preview, int priority, float pressure, int seed, 
            Vector3 position, Vector3 endPosition, Vector3 position2, Vector3 endPosition2, Quaternion rotation, bool clip)
        {
            // base.HandleHitQuad(preview, priority, pressure, seed, position, endPosition, position2, endPosition2, rotation, clip);

            _CwCommandDecal.Instance.SetState(preview, priority);
            _CwCommandDecal.Instance.SetLocation(position, endPosition, position2, endPosition2, clip: clip);

            var worldSize = HandleHitCommon(preview, pressure, seed, rotation);
            var worldRadius = PaintCore.CwCommon.GetRadius(worldSize, position, endPosition, position2, endPosition2);
            var worldPosition = PaintCore.CwCommon.GetPosition(position, endPosition, position2, endPosition2);

            HandleMaskCommon(worldPosition);

            _CwPaintableManager.Instance.SubmitAll(_CwCommandDecal.Instance, worldPosition, worldRadius, 
                Layers, Group, TargetModel, TargetTexture);
        }

        /// <summary>This method paints the scene using the current component settings at the specified <b>CwHit</b>.</summary>
        public override void HandleHitCoord(bool preview, int priority, float pressure, int seed, CwHit hit, Quaternion rotation)
        {
            Debug.LogFormat(LOG_FORMAT, "$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$");
            // base.HandleHitCoord(preview, priority, pressure, seed, hit, rotation);

            _CwPaintableAtlas model = hit.Transform.GetComponent<_CwPaintableAtlas>();

            if (model != null)
            {
                List<_CwPaintableTexture> paintableTextures = model._FindPaintableTextures(Group);

                for (var i = paintableTextures.Count - 1; i >= 0; i--)
                {
                    _CwPaintableTexture paintableTexture = paintableTextures[i];
                    var coord = paintableTexture.GetCoord(ref hit);

                    if (Modifiers != null && Modifiers.Count > 0)
                    {
                        var position = (Vector3)coord;

                        CwHelper.BeginSeed(seed);
                        Modifiers.ModifyPosition(ref position, preview, pressure);
                        CwHelper.EndSeed();

                        coord = position;
                    }

                    _CwCommandDecal.Instance.SetState(preview, priority);
                    _CwCommandDecal.Instance.SetLocation(coord, false);

                    HandleHitCommon(preview, pressure, seed, rotation);

                    _CwCommandDecal.Instance.ClearMask();

                    _CwCommandDecal.Instance.ApplyAspect(paintableTexture.Current);

                    _CwPaintableManager.Instance.Submit(_CwCommandDecal.Instance, model, paintableTexture);
                }
            }
        }

        protected override Vector3 HandleHitCommon(bool preview, float pressure, int seed, Quaternion rotation)
        {
            // base.HandleHitCommon(preview, pressure, seed, rotation, false);

            var finalOpacity = Opacity;
            var finalRadius = Radius;
            var finalScale = Scale;
            var finalHardness = Hardness;
            var finalColor = _Color;
            var finalAngle = Angle;
            var finalTexture = _Texture;
            // Matrix4x4 finalMatrix = TileTransform != null ? TileTransform.localToWorldMatrix : Matrix4x4.identity;
            Matrix4x4 finalMatrix = Matrix4x4.identity;
            if (TileTransform != null)
            {
                finalMatrix = TileTransform.localToWorldMatrix;
            }

            if (Modifiers != null && Modifiers.Count > 0)
            {
                CwHelper.BeginSeed(seed);
                Modifiers.ModifyColor(ref finalColor, preview, pressure);
                Modifiers.ModifyAngle(ref finalAngle, preview, pressure);
                Modifiers.ModifyOpacity(ref finalOpacity, preview, pressure);
                Modifiers.ModifyRadius(ref finalRadius, preview, pressure);
                Modifiers.ModifyScale(ref finalScale, preview, pressure);
                Modifiers.ModifyHardness(ref finalHardness, preview, pressure);
                Modifiers.ModifyTexture(ref finalTexture, preview, pressure);
                CwHelper.EndSeed();
            }

            var finalAspect = PaintCore.CwCommon.GetAspect(Shape, finalTexture);
            var finalSize = PaintCore.CwCommon.ScaleAspect(finalScale * finalRadius, finalAspect);

            _CwCommandDecal.Instance.SetShape(rotation, finalSize, finalAngle);

            _CwCommandDecal.Instance.SetMaterial(BlendMode, finalTexture, Shape,
                ShapeChannel, finalHardness, Wrapping, NormalBack, NormalFront, NormalFade, 
                finalColor, finalOpacity, TileTexture, finalMatrix, TileOpacity, TileTransition);

            return finalSize;
        }

        protected override void HandleMaskCommon(Vector3 worldPosition)
        {
            // base.HandleMaskCommon(worldPosition);

            if (FindMask == true)
            {
                var mask = CwMask.Find(worldPosition, Layers);

                if (mask != null)
                {
                    _CwCommandDecal.Instance.SetMask(mask.Matrix, mask.Texture, mask.Channel, mask.Invert, mask.Stretch);
                }
                else
                {
                    _CwCommandDecal.Instance.ClearMask();
                }
            }
            else
            {
                _CwCommandDecal.Instance.ClearMask();
            }

            if (FindDepthMask == true)
            {
                _CwCommandDecal.Instance.DepthMask = CwRenderDepth.Find();
            }
            else
            {
                _CwCommandDecal.Instance.DepthMask = null;
            }
        }
    }
}

#if false //
#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = CwPaintDecal;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwPaintDecal_Editor : CwEditor
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
			BeginError(Any(tgts, t => t.Texture == null && t.Shape == null));
				Draw("texture", "The decal that will be painted.");
			EndError();
			EditorGUILayout.BeginHorizontal();
				BeginError(Any(tgts, t => t.BlendMode.Index == CwBlendMode.REPLACE && t.Shape == null));
					Draw("shape", "This allows you to specify the shape of the decal. This is optional for most blending modes, because they usually derive their shape from the RGB or A values. However, if you're using the Replace blending mode, then you must manually specify the shape.");
				EndError();
				EditorGUILayout.PropertyField(serializedObject.FindProperty("shapeChannel"), GUIContent.none, GUILayout.Width(50));
			EditorGUILayout.EndHorizontal();
			Draw("color", "The color of the paint.");
			Draw("opacity", "The opacity of the brush.");

			Separator();

			Draw("angle", "The angle of the decal in degrees.");
			Draw("scale", "This allows you to control the mirroring and aspect ratio of the decal.\n\n1, 1 = No scaling.\n-1, 1 = Horizontal Flip.");
			BeginError(Any(tgts, t => t.Radius <= 0.0f));
				Draw("radius", "The radius of the paint brush.");
			EndError();
			BeginError(Any(tgts, t => t.Hardness <= 0.0f));
				Draw("hardness", "This allows you to control the sharpness of the near+far depth cut-off point.");
			EndError();
			Draw("wrapping", "This allows you to control how much the decal can wrap around uneven paint surfaces.");

			Separator();

			if (DrawFoldout("Advanced", "Show advanced settings?") == true)
			{
				BeginIndent();
					Draw("targetModel", "If this is set, then only the specified CwPaintable___ will be painted, regardless of the layer setting.");
					Draw("targetTexture", "If this is set, then only the specified CwPaintableTexture will be painted, regardless of the layer or group setting.");

					Separator();

					Draw("normalFront", "This allows you to control how much the paint can wrap around the front of surfaces (e.g. if you want paint to wrap around curved surfaces then set this to a higher value).\n\nNOTE: If you set this to 0 then paint will not be applied to front facing surfaces.");
					Draw("normalBack", "This works just like Normal Front, except for back facing surfaces.\n\nNOTE: If you set this to 0 then paint will not be applied to back facing surfaces.");
					Draw("normalFade", "This allows you to control the smoothness of the depth cut-off point.");

					Separator();

					Draw("tileTexture", "This allows you to apply a tiled detail texture to your decals. This tiling will be applied in world space using triplanar mapping.");
					Draw("tileTransform", "This allows you to adjust the tiling position + rotation + scale using a Transform.");
					Draw("tileOpacity", "This allows you to control the triplanar influence.\n\n0 = No influence.\n\n1 = Full influence.");
					Draw("tileTransition", "This allows you to control how quickly the triplanar mapping transitions between the X/Y/Z planes.");
					Draw("findMask", "If your scene contains a <b>CwMask</b>, should this paint component use it?");
					Draw("findDepthMask", "If your scene contains a <b>CwRenderDepth</b>, should this paint component use it?");
				EndIndent();
			}

			Separator();

			tgt.Modifiers.DrawEditorLayout(serializedObject, target, "Color", "Angle", "Opacity", "Radius", "Scale", "Hardness", "Texture", "Position");
		}
	}
}
#endif
#endif