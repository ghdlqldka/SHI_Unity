//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2023 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework
{
	/// <summary>
	/// Tween the object's alpha. Works with both UI widgets as well as renderers.
	/// </summary>

	// [AddComponentMenu("UGUI/Tween/Tween Alpha")]
	public class _TweenAlpha : _UITweener
	{
		private static string LOG_FORMAT = "<color=#FFF4D6><b>[_TweenAlpha]</b></color> {0}";

		[Range(0f, 1f)]
		public float from = 1f;
		[Range(0f, 1f)]
		public float to = 1f;

		[Tooltip("If used on a renderer, the material should probably be cleaned up after this script gets destroyed...")]
		public bool autoCleanup = false;

		[Tooltip("Color to adjust")]
		public string colorProperty;

		[System.NonSerialized]
		protected bool mCached = false;
		// [System.NonSerialized]
		// UIRect mRect; // NGUI
		[System.NonSerialized]
		protected Material mShared;
		[System.NonSerialized]
		protected Material mMat;
		[System.NonSerialized]
		protected Light mLight;
		[System.NonSerialized]
		protected SpriteRenderer mSr;
		[System.NonSerialized]
		protected Image mImage; // UGUI
		[System.NonSerialized]
		protected CanvasGroup mCanvasGroup; // UGUI

		[System.NonSerialized]
		protected float mBaseIntensity = 1f;

		[System.Obsolete("Use 'value' instead")]
		public float alpha
		{
			get
			{
				return this.value;
			}
			set
			{
				this.value = value;
			}
		}

		protected virtual void Awake()
		{
			Debug.LogFormat(LOG_FORMAT, "Awake()");
		}

		protected virtual void OnDestroy()
		{
			if (autoCleanup && mMat != null && mShared != mMat) 
			{ 
				Destroy(mMat); mMat = null;
			}
		}

		protected virtual void Cache()
		{
			mCached = true;
			// mRect = GetComponent<UIRect>();
			mSr = GetComponent<SpriteRenderer>();
			mImage = GetComponent<Image>();
			mCanvasGroup = GetComponent<CanvasGroup>();

			if (/*mRect == null &&*/ mSr == null)
			{
				mLight = GetComponent<Light>();

				if (mLight == null)
				{
					var ren = GetComponent<Renderer>();

					if (ren != null)
					{
						mShared = ren.sharedMaterial;
						mMat = ren.material;
					}

					/*
					if (mMat == null)
						mRect = GetComponentInChildren<UIRect>();
					*/
				}
				else mBaseIntensity = mLight.intensity;
			}
		}

		/// <summary>
		/// Tween's current value.
		/// </summary>

		public virtual float value
		{
			get
			{
				if (!mCached)
					Cache();
				// if (mRect != null) return mRect.alpha;
				if (mSr != null) 
					return mSr.color.a;
				if (mCanvasGroup != null)
				{
					return mCanvasGroup.alpha;
				}
				if (mImage != null) 
					return mImage.color.a;
				if (mMat == null) 
					return 1f;
				if (string.IsNullOrEmpty(colorProperty)) 
					return mMat.color.a;

				return mMat.GetColor(colorProperty).a;
			}
			set
			{
				if (mCached == false)
					Cache();

				/*
				if (mRect != null)
				{
					mRect.alpha = value;
				}
				else*/
				if (mSr != null)
				{
					var c = mSr.color;
					c.a = value;
					mSr.color = c;
				}
				else if (mMat != null)
				{
					if (string.IsNullOrEmpty(colorProperty))
					{
						var c = mMat.color;
						c.a = value;
						mMat.color = c;
					}
					else
					{
						var c = mMat.GetColor(colorProperty);
						c.a = value;
						mMat.SetColor(colorProperty, c);
					}
				}
				else if (mLight != null)
				{
					mLight.intensity = mBaseIntensity * value;
				}
				else if (mCanvasGroup != null)
				{
					mCanvasGroup.alpha = value;
				}
				else if (mImage != null)
				{
					var c = mImage.color;
					c.a = value;
					mImage.color = c;
				}
			}
		}

		/// <summary>
		/// Tween the value.
		/// </summary>

		protected override void OnUpdate(float factor, bool isFinished)
		{
			value = Mathf.Lerp(from, to, factor);
		}

		/// <summary>
		/// Start the tweening operation.
		/// </summary>

		static public _TweenAlpha Begin(GameObject go, float duration, float alpha, float delay = 0f)
		{
			var comp = _UITweener.Begin<_TweenAlpha>(go, duration, delay);
			comp.from = comp.value;
			comp.to = alpha;

			if (duration <= 0f)
			{
				comp.Sample(1f, true);
				comp.enabled = false;
			}
			return comp;
		}

		public override void SetStartToCurrentValue()
		{
			from = value;
		}
		public override void SetEndToCurrentValue()
		{
			to = value;
		}
	}
}