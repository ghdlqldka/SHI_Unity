﻿using UnityEngine;
using CW.Common;
using PaintCore;

namespace PaintIn3D
{
	/// <summary>This allows you to paint a sphere at a hit point. Hit points will automatically be sent by any <b>CwHit___</b> component on this GameObject, or its ancestors.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwPaintSphere")]
	[AddComponentMenu(PaintCore.CwCommon.ComponentHitMenuPrefix + "Paint Sphere")]
	public class CwPaintSphere : MonoBehaviour, IHitPoint, IHitLine, IHitTriangle, IHitQuad, IHitCoord
	{
		/// <summary>Only the CwPaintable___ GameObjects whose layers are within this mask will be eligible for painting.</summary>
		public LayerMask Layers { set { layers = value; } get { return layers; } } [SerializeField] private LayerMask layers = -1;

		/// <summary>Only the <b>CwPaintableTexture</b> components with a matching group will be painted by this component.</summary>
		public CwGroup Group { set { group = value; } get { return group; } } [SerializeField] private CwGroup group;

		/// <summary>If this is set, then only the specified model will be painted, regardless of the layer setting.</summary>
		public CwMeshModel TargetModel { set { targetModel = value; } get { return targetModel; } } [SerializeField] private CwMeshModel targetModel;

		/// <summary>If this is set, then only the specified CwPaintableTexture will be painted, regardless of the layer or group setting.</summary>
		public CwPaintableTexture TargetTexture { set { targetTexture = value; } get { return targetTexture; } } [SerializeField] protected CwPaintableTexture targetTexture;

		/// <summary>This allows you to choose how the paint from this component will combine with the existing pixels of the textures you paint.
		/// NOTE: See the <b>Blend Mode</b> section of the documentation for more information.</summary>
		public CwBlendMode BlendMode { set { blendMode = value; } get { return blendMode; } } [SerializeField] private CwBlendMode blendMode = CwBlendMode.AlphaBlend(Vector4.one);

		/// <summary>The color of the paint.</summary>
		public Color Color { set { color = value; } get { return color; } } [SerializeField] private Color color = Color.white;

		/// <summary>The opacity of the brush.</summary>
		public float Opacity { set { opacity = value; } get { return opacity; } } [Range(0.0f, 1.0f)] [SerializeField] private float opacity = 1.0f;

		/// <summary>The angle of the paint in degrees.
		/// NOTE: This is only useful if you change the <b>Scale.x/y</b> values.</summary>
		public float Angle { set { angle = value; } get { return angle; } } [Range(-180.0f, 180.0f)] [SerializeField] private float angle;

		/// <summary>By default this component paints using a sphere shape, but you can override this here to paint an ellipsoid.
		/// NOTE: When painting an ellipsoid, the orientation of the sphere matters. This can be controlled from the <b>CwHit__</b> component settings.</summary>
		public Vector3 Scale { set { scale = value; } get { return scale; } } [SerializeField] private Vector3 scale = Vector3.one;

		/// <summary>The radius of the paint brush.</summary>
		public float Radius { set { radius = value; } get { return radius; } } [SerializeField] private float radius = 0.1f;

		/// <summary>The hardness of the paint brush.</summary>
		public float Hardness { set { hardness = value; } get { return hardness; } } [SerializeField] private float hardness = 5.0f;

		/// <summary>This allows you to apply a tiled detail texture to your decals. This tiling will be applied in world space using triplanar mapping.</summary>
		public Texture TileTexture { set { tileTexture = value; } get { return tileTexture; } } [SerializeField] private Texture tileTexture;

		/// <summary>This allows you to adjust the tiling position + rotation + scale using a <b>Transform</b>.</summary>
		public Transform TileTransform { set { tileTransform = value; } get { return tileTransform; } } [SerializeField] private Transform tileTransform;

		/// <summary>This allows you to control the triplanar influence.
		/// 0 = No influence.
		/// 1 = Full influence.</summary>
		public float TileOpacity { set { tileOpacity = value; } get { return tileOpacity; } } [UnityEngine.Serialization.FormerlySerializedAs("tileBlend")] [Range(0.0f, 1.0f)] [SerializeField] private float tileOpacity = 1.0f;

		/// <summary>This allows you to control how quickly the triplanar mapping transitions between the X/Y/Z planes.</summary>
		public float TileTransition { set { tileTransition = value; } get { return tileTransition; } } [Range(1.0f, 200.0f)] [SerializeField] private float tileTransition = 4.0f;

		/// <summary>If your scene contains a <b>CwMask</b>, should this paint component use it?</summary>
		public bool FindMask { set { findMask = value; } get { return findMask; } } [SerializeField] private bool findMask = true;

		/// <summary>If your scene contains a <b>CwRenderDepth</b>, should this paint component use it?</summary>
		public bool FindDepthMask { set { findDepthMask = value; } get { return findDepthMask; } } [SerializeField] private bool findDepthMask = true;

		/// <summary>This stores a list of all modifiers used to change the way this component applies paint (e.g. <b>CwModifyColorRandom</b>).</summary>
		public CwModifierList Modifiers { get { if (modifiers == null) modifiers = new CwModifierList(); return modifiers; } } [SerializeField] private CwModifierList modifiers;

		/// <summary>This method increments the angle by the specified amount of degrees, and wraps it to the -180..180 range.</summary>
		public void IncrementAngle(float degrees)
		{
			angle = Mathf.Repeat(angle + 180.0f + degrees, 360.0f) - 180.0f;
		}

		/// <summary>This method multiplies the <b>Opacity</b> by the specified value.</summary>
		public void MultiplyOpacity(float multiplier)
		{
			opacity = Mathf.Clamp01(opacity * multiplier);
		}

		/// <summary>This method increments the <b>Opacity</b> by the specified value.</summary>
		public void IncrementOpacity(float delta)
		{
			opacity = Mathf.Clamp01(opacity + delta);
		}

		/// <summary>This method multiplies the <b>Radius</b> by the specified value.</summary>
		public void MultiplyRadius(float multiplier)
		{
			radius *= multiplier;
		}

		/// <summary>This method increases the <b>Radius</b> by the specified value.</summary>
		public void IncrementRadius(float delta)
		{
			radius += delta;
		}

		/// <summary>This method multiplies the <b>Scale</b> by the specified value.</summary>
		public void MultiplyScale(float multiplier)
		{
			scale *= multiplier;
		}

		/// <summary>This method increases the <b>Scale</b> by the specified value.</summary>
		public void IncrementScale(float multiplier)
		{
			scale += Vector3.one * multiplier;
		}

		/// <summary>This method paints all pixels at the specified point using the shape of a sphere.</summary>
		public virtual void HandleHitPoint(bool preview, int priority, float pressure, int seed, Vector3 position, Quaternion rotation)
		{
			if (modifiers != null && modifiers.Count > 0)
			{
				CwHelper.BeginSeed(seed);
					modifiers.ModifyPosition(ref position, preview, pressure);
				CwHelper.EndSeed();
			}

			CwCommandSphere.Instance.SetState(preview, priority);
			CwCommandSphere.Instance.SetLocation(position);

			var worldSize     = HandleHitCommon(preview, pressure, seed, rotation);
			var worldRadius   = PaintCore.CwCommon.GetRadius(worldSize);
			var worldPosition = position;

			HandleMaskCommon(worldPosition);

			CwPaintableManager.SubmitAll(CwCommandSphere.Instance, worldPosition, worldRadius, layers, group, targetModel, targetTexture);
		}

		/// <summary>This method paints all pixels between the two specified points using the shape of a sphere.</summary>
		public virtual void HandleHitLine(bool preview, int priority, float pressure, int seed, Vector3 position, Vector3 endPosition, Quaternion rotation, bool clip)
		{
			CwCommandSphere.Instance.SetState(preview, priority);
			CwCommandSphere.Instance.SetLocation(position, endPosition, clip: clip);

			var worldSize     = HandleHitCommon(preview, pressure, seed, rotation);
			var worldRadius   = PaintCore.CwCommon.GetRadius(worldSize, position, endPosition);
			var worldPosition = PaintCore.CwCommon.GetPosition(position, endPosition);

			HandleMaskCommon(worldPosition);

			CwPaintableManager.SubmitAll(CwCommandSphere.Instance, worldPosition, worldRadius, layers, group, targetModel, targetTexture);
		}

		/// <summary>This method paints all pixels between three points using the shape of a sphere.</summary>
		public virtual void HandleHitTriangle(bool preview, int priority, float pressure, int seed, Vector3 positionA, Vector3 positionB, Vector3 positionC, Quaternion rotation)
		{
			CwCommandSphere.Instance.SetState(preview, priority);
			CwCommandSphere.Instance.SetLocation(positionA, positionB, positionC);

			var worldSize     = HandleHitCommon(preview, pressure, seed, rotation);
			var worldRadius   = PaintCore.CwCommon.GetRadius(worldSize, positionA, positionB, positionC);
			var worldPosition = PaintCore.CwCommon.GetPosition(positionA, positionB, positionC);

			HandleMaskCommon(worldPosition);

			CwPaintableManager.SubmitAll(CwCommandSphere.Instance, worldPosition, worldRadius, layers, group, targetModel, targetTexture);
		}

		/// <summary>This method paints all pixels between two pairs of points using the shape of a sphere.</summary>
		public virtual void HandleHitQuad(bool preview, int priority, float pressure, int seed, Vector3 position, Vector3 endPosition, Vector3 position2, Vector3 endPosition2, Quaternion rotation, bool clip)
		{
			CwCommandSphere.Instance.SetState(preview, priority);
			CwCommandSphere.Instance.SetLocation(position, endPosition, position2, endPosition2, clip: clip);

			var worldSize     = HandleHitCommon(preview, pressure, seed, rotation);
			var worldRadius   = PaintCore.CwCommon.GetRadius(worldSize, position, endPosition, position2, endPosition2);
			var worldPosition = PaintCore.CwCommon.GetPosition(position, endPosition, position2, endPosition2);

			HandleMaskCommon(worldPosition);

			CwPaintableManager.SubmitAll(CwCommandSphere.Instance, worldPosition, worldRadius, layers, group, targetModel, targetTexture);
		}

		/// <summary>This method paints the scene using the current component settings at the specified <b>CwHit</b>.
		/// NOTE: The <b>rotation</b> argument is in world space, where <b>Quaternion.identity</b> means the paint faces forward on the +Z axis, and up is +Y.</summary>
		public virtual void HandleHitCoord(bool preview, int priority, float pressure, int seed, CwHit hit, Quaternion rotation)
		{
			var model = hit.Transform.GetComponent<CwPaintableMeshAtlas>();

			if (model != null)
			{
				var paintableTextures = model.FindPaintableTextures(group);

				for (var i = paintableTextures.Count - 1; i >= 0; i--)
				{
					var paintableTexture = paintableTextures[i];
					var coord            = paintableTexture.GetCoord(ref hit);

					if (modifiers != null && modifiers.Count > 0)
					{
						var position = (Vector3)coord;

						CwHelper.BeginSeed(seed);
							modifiers.ModifyPosition(ref position, preview, pressure);
						CwHelper.EndSeed();

						coord = position;
					}

					CwCommandSphere.Instance.SetState(preview, priority);
					CwCommandSphere.Instance.SetLocation(coord, in3D: false);

					HandleHitCommon(preview, pressure, seed, rotation);

					CwCommandSphere.Instance.ClearMask();

					CwCommandSphere.Instance.ApplyAspect(paintableTexture.Current);

					CwPaintableManager.Submit(CwCommandSphere.Instance, model, paintableTexture);
				}
			}
		}

		protected Vector3 HandleHitCommon(bool preview, float pressure, int seed, Quaternion rotation)
		{
			var finalOpacity    = opacity;
			var finalRadius     = radius;
			var finalScale      = scale;
			var finalHardness   = hardness;
			var finalColor      = color;
			var finalAngle      = angle;
			var finalTileMatrix = tileTransform != null ? tileTransform.localToWorldMatrix : Matrix4x4.identity;

			if (modifiers != null && modifiers.Count > 0)
			{
				CwHelper.BeginSeed(seed);
					modifiers.ModifyColor(ref finalColor, preview, pressure);
					modifiers.ModifyAngle(ref finalAngle, preview, pressure);
					modifiers.ModifyOpacity(ref finalOpacity, preview, pressure);
					modifiers.ModifyRadius(ref finalRadius, preview, pressure);
					modifiers.ModifyScale(ref finalScale, preview, pressure);
					modifiers.ModifyHardness(ref finalHardness, preview, pressure);
				CwHelper.EndSeed();
			}

			var finalSize = finalScale * finalRadius;

			CwCommandSphere.Instance.SetShape(rotation, finalSize, finalAngle);

			CwCommandSphere.Instance.SetMaterial(BlendMode, finalHardness, finalColor, finalOpacity, tileTexture, finalTileMatrix, tileOpacity, tileTransition);

			return finalSize;
		}

		protected virtual void HandleMaskCommon(Vector3 worldPosition)
		{
			if (findMask == true)
			{
				var mask = CwMask.Find(worldPosition, layers);

				if (mask != null)
				{
					CwCommandSphere.Instance.SetMask(mask.Matrix, mask.Texture, mask.Channel, mask.Invert, mask.Stretch);
				}
				else
				{
					CwCommandSphere.Instance.ClearMask();
				}
			}
			else
			{
				CwCommandSphere.Instance.ClearMask();
			}

			if (findDepthMask == true)
			{
				CwCommandSphere.Instance.DepthMask = CwRenderDepth.Find();
			}
			else
			{
				CwCommandSphere.Instance.DepthMask = null;
			}
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);

			Gizmos.DrawWireSphere(Vector3.zero, radius);
		}
#endif
	}
}

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