//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2023 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;

namespace _Base_Framework
{

	/// <summary>
	/// Tween the object's rotation.
	/// </summary>

	// [AddComponentMenu("NGUI/Tween/Tween Rotation")]
	public class _TweenRotation : _UITweener
	{
		private static string LOG_FORMAT = "<color=#FFF4D6><b>[_TweenRotation]</b></color> {0}";

		public Vector3 from;
		public Vector3 to;
		public bool quaternionLerp = false;

		protected Transform mTrans;

		public Transform cachedTransform
		{
			get
			{
				if (mTrans == null) 
					mTrans = transform; return mTrans;
			}
		}

		[System.Obsolete("Use 'value' instead")]
		public Quaternion rotation
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

		public Quaternion value
		{
			get
			{
				return cachedTransform.localRotation;
			}
			set
			{
				cachedTransform.localRotation = value;
			}
		}

		protected virtual void Awake()
		{
			Debug.LogFormat(LOG_FORMAT, "Awake()");
		}

		/// <summary>
		/// Tween the value.
		/// </summary>

		protected override void OnUpdate(float factor, bool isFinished)
		{
			value = quaternionLerp ? Quaternion.Slerp(Quaternion.Euler(from), Quaternion.Euler(to), factor) :
				Quaternion.Euler(new Vector3(
				Mathf.Lerp(from.x, to.x, factor),
				Mathf.Lerp(from.y, to.y, factor),
				Mathf.Lerp(from.z, to.z, factor)));
		}

		/// <summary>
		/// Start the tweening operation.
		/// </summary>

		static public _TweenRotation Begin(GameObject go, float duration, Quaternion rot)
		{
			_TweenRotation comp = _UITweener.Begin<_TweenRotation>(go, duration);
			comp.from = comp.value.eulerAngles;
			comp.to = rot.eulerAngles;

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
			from = value.eulerAngles;
		}

		[ContextMenu("Set 'To' to current value")]
		public override void SetEndToCurrentValue()
		{
			to = value.eulerAngles;
		}

		[ContextMenu("Assume value of 'From'")]
		void SetCurrentValueToStart()
		{
			value = Quaternion.Euler(from);
		}

		[ContextMenu("Assume value of 'To'")]
		void SetCurrentValueToEnd()
		{
			value = Quaternion.Euler(to);
		}
	}
}