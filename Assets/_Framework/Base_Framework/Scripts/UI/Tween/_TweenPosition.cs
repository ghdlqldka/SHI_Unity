//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2023 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;

namespace _Base_Framework
{
	/// <summary>
	/// Tween the object's position.
	/// </summary>

	// [AddComponentMenu("NGUI/Tween/Tween Position")]
	public class _TweenPosition : _UITweener
	{
		private static string LOG_FORMAT = "<color=#FFF4D6><b>[_TweenPosition]</b></color> {0}";

		public Vector3 from;
		public Vector3 to;

		[HideInInspector]
		public bool worldSpace = false;

		protected Transform mTrans;
		// UIRect mRect;

		public Transform cachedTransform
		{
			get
			{
				if (mTrans == null) mTrans = transform; return mTrans;
			}
		}

		[System.Obsolete("Use 'value' instead")]
		public Vector3 position
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

		/// <summary>
		/// Tween's current value.
		/// </summary>

		public virtual Vector3 value
		{
			get
			{
				return worldSpace ? cachedTransform.position : cachedTransform.localPosition;
			}
			set
			{
#if false
				if (mRect == null || !mRect.isAnchored || worldSpace)
#endif
				{
					if (worldSpace)
						cachedTransform.position = value;
					else
						cachedTransform.localPosition = value;
				}
				#if false
				else
				{
					value -= cachedTransform.localPosition;
					NGUIMath.MoveRect(mRect, value.x, value.y);
				}
#endif
			}
		}

		protected virtual void Awake()
		{
			Debug.LogFormat(LOG_FORMAT, "Awake()");

			// mRect = GetComponent<UIRect>();
		}

		/// <summary>
		/// Tween the value.
		/// </summary>

		protected override void OnUpdate(float factor, bool isFinished)
		{
			value = from * (1f - factor) + to * factor;
		}

		/// <summary>
		/// Start the tweening operation.
		/// </summary>

		static public _TweenPosition Begin(GameObject go, float duration, Vector3 pos)
		{
			_TweenPosition comp = _UITweener.Begin<_TweenPosition>(go, duration);
			comp.from = comp.value;
			comp.to = pos;

			if (duration <= 0f)
			{
				comp.Sample(1f, true);
				comp.enabled = false;
			}
			return comp;
		}

		/// <summary>
		/// Start the tweening operation.
		/// </summary>

		static public _TweenPosition Begin(GameObject go, float duration, Vector3 pos, bool worldSpace)
		{
			_TweenPosition comp = _UITweener.Begin<_TweenPosition>(go, duration);
			comp.worldSpace = worldSpace;
			comp.from = comp.value;
			comp.to = pos;

			if (duration <= 0f)
			{
				comp.Sample(1f, true);
				comp.enabled = false;
			}
			return comp;
		}

		[ContextMenu("Set 'From' to current value")]
		public override void SetStartToCurrentValue()
		{
			from = value;
		}

		[ContextMenu("Set 'To' to current value")]
		public override void SetEndToCurrentValue()
		{
			to = value;
		}

		[ContextMenu("Assume value of 'From'")]
		void SetCurrentValueToStart()
		{
			value = from;
		}

		[ContextMenu("Assume value of 'To'")]
		void SetCurrentValueToEnd()
		{
			value = to;
		}
	}
}