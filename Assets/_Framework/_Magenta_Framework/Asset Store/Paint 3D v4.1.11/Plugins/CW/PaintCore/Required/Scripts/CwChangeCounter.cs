﻿using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component will check all pixels in the specified paintable texture, compare them to the reference state defined in this component, and tell you how many of them differ by more than the threshold value.</summary>
	[ExecuteInEditMode]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwChangeCounter")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Change Counter")]
	public class CwChangeCounter : CwPaintableTextureMonitorMask
	{
		/// <summary>This stores all active and enabled instances.</summary>
		public static LinkedList<CwChangeCounter> Instances = new LinkedList<CwChangeCounter>(); private LinkedListNode<CwChangeCounter> instancesNode;

		/// <summary>The RGBA values must be within this range of a color for it to be counted.</summary>
		public float Threshold { set { if (threshold != value) { threshold = value; MarkChangeReaderAsDirty(); } } get { return threshold; } } [Range(0.0f, 1.0f)] [SerializeField] private float threshold = 0.1f;

		/// <summary>The texture we want to compare change to.
		/// None/null = white.
		/// NOTE: All pixels in this texture will be tinted by the current <b>Color</b>.</summary>
		public Texture Texture { set { if (texture != value) { texture = value; MarkChangeReaderAsDirty(); } } get { return texture; } } [SerializeField] private Texture texture;

		/// <summary>The color we want to compare change to.
		/// NOTE: All pixels in the <b>Texture</b> will be tinted by this.</summary>
		public Color Color { set { if (color != value) { color = value; MarkChangeReaderAsDirty(); } } get { return color; } } [SerializeField] private Color color = Color.white;

		/// <summary>The previously counted amount of pixels with a RGBA value difference above the threshold.</summary>
		public int Count { get { return count; } } [SerializeField] private int count;

		/// <summary>The <b>Count / Total</b> value.</summary>
		public float Ratio { get { return total > 0 ? count / (float)total : 0.0f; } }

		[System.NonSerialized]
		private CwReader changeReader;

		[SerializeField]
		protected NativeArray<Color32> changePixels;

		public CwReader ChangeReader
		{
			get
			{
				return changeReader;
			}
		}
		
		/// <summary>This will return true once this counter has updated at least once.</summary>
		public bool HasRead
		{
			get
			{
				return MaskReader != null && MaskReader.ReadCount > 0 && CurrentReader != null && CurrentReader.ReadCount > 0 && changeReader != null && changeReader.ReadCount > 0;
			}
		}

		public void MarkChangeReaderAsDirty()
		{
			if (changeReader != null)
			{
				changeReader.MarkAsDirty();
			}
		}

		/// <summary>The <b>Total</b> of the specified counters.</summary>
		public static long GetTotal(ICollection<CwChangeCounter> counters = null)
		{
			var total = 0L; foreach (var counter in counters ?? Instances) { if (counter != null) total += counter.total; } return total;
		}

		/// <summary>The <b>Count</b> of the specified counters.</summary>
		public static long GetCount(ICollection<CwChangeCounter> counters = null)
		{
			var solid = 0L; foreach (var counter in counters ?? Instances) { if (counter != null) solid += counter.count; } return solid;
		}

		/// <summary>The <b>Ratio</b> of the specified counters.</summary>
		public static float GetRatio(ICollection<CwChangeCounter> counters = null)
		{
			return CwHelper.Divide(GetCount(counters), GetTotal(counters));
		}

		public static bool GetReady(ICollection<CwChangeCounter> counters = null)
		{
			foreach (var counter in counters ?? Instances)
			{
				if (counter != null)
				{
					if (counter.HasRead == false) return false;
				}
			}
			
			return true;
		}

		private void HandleCompleteChange(NativeArray<Color32> pixels)
		{
			if (changePixels.IsCreated == true && changePixels.Length != pixels.Length)
			{
				changePixels.Dispose();
			}

			if (changePixels.IsCreated == false)
			{
				changePixels = new NativeArray<Color32>(pixels.Length, Allocator.Persistent);
			}

			if (changePixels.IsCreated == true)
			{
				NativeArray<Color32>.Copy(pixels, changePixels);
			}
			else
			{
				changePixels = new NativeArray<Color32>(pixels, Allocator.Persistent);
			}

			HandleComplete(changeReader.DownsampleBoost);
		}

		protected override void HandleComplete(int boost)
		{
			if (currentPixels.IsCreated == false || maskPixels.IsCreated == false || changePixels.IsCreated == false || currentPixels.Length != maskPixels.Length || currentPixels.Length != changePixels.Length)
			{
				return;
			}

			var threshold32 = (byte)(threshold * 255.0f);
			var oldTotal    = total;

			count = 0;
			total = 0;

			for (var i = 0; i < currentPixels.Length; i++)
			{
				if (maskPixels[i] > 127)
				{
					total++;

					var currentPixel = currentPixels[i];
					var changePixel  = changePixels[i];
					var distance     = 0;

					distance += System.Math.Abs(changePixel.r - currentPixel.r);
					distance += System.Math.Abs(changePixel.g - currentPixel.g);
					distance += System.Math.Abs(changePixel.b - currentPixel.b);
					distance += System.Math.Abs(changePixel.a - currentPixel.a);

					if (distance > threshold32)
					{
						count++;
					}
				}
			}

			total *= boost;
			count *= boost;

			if (CalculateTotal == false)
			{
				total = oldTotal;
			}

			InvokeOnUpdated();
		}

		protected override void OnEnable()
		{
			instancesNode = Instances.AddLast(this);

			base.OnEnable();

			if (changeReader == null)
			{
				changeReader = new CwReader();

				changeReader.OnComplete += HandleCompleteChange;
			}
		}

		protected override void OnDisable()
		{
			Instances.Remove(instancesNode); instancesNode = null;

			base.OnDisable();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (changeReader != null)
			{
				changeReader.OnComplete -= HandleCompleteChange;

				changeReader.Release();
			}

			if (changePixels.IsCreated == true)
			{
				changePixels.Dispose();
			}
		}

		protected override void Start()
		{
			base.Start();

			if (MaskReader.Dirty == true)
			{
				changeReader.MarkAsDirty();
			}
		}

		protected override void Update()
		{
			base.Update();

			if (changeReader.Requested == false && registeredPaintableTexture != null && registeredPaintableTexture.Activated == true)
			{
				if (CwReader.NeedsUpdating(changeReader, changePixels, registeredPaintableTexture.Current, downsampleSteps) == true)
				{
					var desc          = registeredPaintableTexture.Current.descriptor; desc.useMipMap = false;
					var renderTexture = CwCommon.GetRenderTexture(desc);

					CwCommandReplace.Blit(renderTexture, texture, color);

					// Request new change
					changeReader.Request(renderTexture, DownsampleSteps, Async);

					CwCommon.ReleaseRenderTexture(renderTexture);
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwChangeCounter;

	[CustomEditor(typeof(TARGET))]
	public class CwChangeCounter_Editor : CwPaintableTextureMonitorMask_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var markAsDirty = false;

			base.OnInspector();

			Separator();

			Draw("threshold", ref markAsDirty, "The RGBA value must be higher than this for it to be counted.");
			DrawTexture(tgts, markAsDirty);
			DrawColor(tgts, markAsDirty);

			Separator();

			Draw("total");

			EditorGUILayout.BeginHorizontal();
				Draw("count");
				EditorGUI.ProgressBar(Reserve(), tgt.Ratio, "Ratio");
			EditorGUILayout.EndHorizontal();

			if (markAsDirty == true)
			{
				Each(tgts, t => t.MarkChangeReaderAsDirty(), true);
			}
		}

		private void DrawTexture(TARGET[] tgts, bool dirtyChange)
		{
			EditorGUILayout.BeginHorizontal();
				Draw("texture", ref dirtyChange, "The texture we want to compare change to.\n\nNone/null = white.\n\nNOTE: All pixels in this texture will be tinted by the current Color.");
				BeginDisabled(All(tgts, t => t.PaintableTexture == null || t.PaintableTexture.Texture == t.Texture));
					if (GUILayout.Button("Copy", EditorStyles.miniButton, GUILayout.ExpandWidth(false)) == true)
					{
						Undo.RecordObjects(targets, "Copy Texture"); Each(tgts, t => { if (t.PaintableTexture != null) { t.Texture = t.PaintableTexture.Texture; EditorUtility.SetDirty(t); } });
					}
				EndDisabled();
			EditorGUILayout.EndHorizontal();
		}

		private void DrawColor(TARGET[] tgts, bool dirtyChange)
		{
			EditorGUILayout.BeginHorizontal();
				Draw("color", ref dirtyChange, "The color we want to compare change to.\n\nNOTE: All pixels in the Texture will be tinted by this.");
				BeginDisabled(All(tgts, t => t.PaintableTexture == null || t.PaintableTexture.Color == t.Color));
					if (GUILayout.Button("Copy", EditorStyles.miniButton, GUILayout.ExpandWidth(false)) == true)
					{
						Undo.RecordObjects(targets, "Copy Color"); Each(tgts, t => { if (t.PaintableTexture != null) { t.Color = t.PaintableTexture.Color; EditorUtility.SetDirty(t); } });
					}
				EndDisabled();
			EditorGUILayout.EndHorizontal();
		}
	}
}
#endif