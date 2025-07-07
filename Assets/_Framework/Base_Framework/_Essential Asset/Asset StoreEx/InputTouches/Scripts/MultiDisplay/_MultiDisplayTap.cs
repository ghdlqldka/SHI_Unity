using UnityEngine;
using System.Collections;

namespace _InputTouches
{
	public enum TapType
	{
		None,

		ShortTap,
		LongTap,
		DoubleTap,
	}

	public class _MultiDisplayTap : MonoBehaviour
	{
		private static string LOG_FORMAT = "<b><color=#AB3030>[_MultiDisplay</color><color=red>Tap</color><color=#AB3030>]</color></b> {0}";

		[ReadOnly]
		[SerializeField]
		protected _MultiDisplayInputManager _multiDisplayInputManager;
		public _MultiDisplayInputManager MultiDisplayInputManager
		{
			get
			{
				return _multiDisplayInputManager;
			}
		}

		[Space(10)]
		[SerializeField]
		protected TapType _tapType;

		protected virtual void Awake()
		{
			Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : " + this.gameObject.name + ", _tapType : <b><color=yellow>" + _tapType + "</color></b>");
			StartCoroutine(PostAwake());
		}

		protected virtual IEnumerator PostAwake()
		{
			while (_MultiDisplayInputManager.Instance == null)
			{
				Debug.LogFormat(LOG_FORMAT, "_MultiDisplayInputManager.Instance == null");
				yield return null;
			}

			_multiDisplayInputManager = _MultiDisplayInputManager.Instance;
		}

		// Use this for initialization
		protected virtual void Start()
		{
			//
		}

		protected virtual void OnEnable()
		{
			_MultiDisplayInputManager.OnMultiTap += OnMultiTap;
			_MultiDisplayInputManager.OnLongTap += OnLongTap;
		}

		protected virtual void OnDisable()
		{
			_MultiDisplayInputManager.OnMultiTap -= OnMultiTap;
			_MultiDisplayInputManager.OnLongTap -= OnLongTap;
		}

		//called when a multi-Tap event is detected
		protected virtual void OnMultiTap(Tap tap, _MultiDisplayInputManager.DisplayIndex display)
		{
			Debug.LogFormat(LOG_FORMAT, "OnMultiTap(), tap.count : <b>" + tap.count + "</b>, display : <color=yellow>" + display + "</color>");

			//do a raycast base on the position of the tap
			Camera camera = MultiDisplayInputManager.GetDiplayCamera(display);
			Ray ray = camera.ScreenPointToRay(tap.pos);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, Mathf.Infinity) == true)
			{
				//if the tap lands on the shortTapObj, then shows the effect.
				if (hit.collider.transform == this.transform && _tapType == TapType.ShortTap)
				{
					Debug.LogWarningFormat(LOG_FORMAT, "this.gameObject : <color=cyan>" + this.gameObject.name + "</color>, <b><color=yellow>ShortTap</color></b>");
				}
				//if the tap lands on the doubleTapObj
				else if (hit.collider.transform == this.transform && _tapType == TapType.DoubleTap)
				{
					//check to make sure if the tap count matches
					if (tap.count == 2)
					{
						Debug.LogWarningFormat(LOG_FORMAT, "this.gameObject : <color=cyan>" + this.gameObject.name + "</color>, <b><color=yellow>DoubleTap</color></b>");
					}
				}
			}
		}

		//called when a long tap event is ended
		protected virtual void OnLongTap(Tap tap, _MultiDisplayInputManager.DisplayIndex display)
		{
			Debug.LogFormat(LOG_FORMAT, "OnLongTap(), display : <b><color=yellow>" + display + "</color></b>");

			//do a raycast base on the position of the tap
			Camera camera = MultiDisplayInputManager.GetDiplayCamera(display);
			Ray ray = camera.ScreenPointToRay(tap.pos);
			RaycastHit hit;
			//if the tap lands on the longTapObj
			if (Physics.Raycast(ray, out hit, Mathf.Infinity))
			{
				if (hit.collider.transform == this.transform && _tapType == TapType.LongTap)
				{
					Debug.LogWarningFormat(LOG_FORMAT, "this.gameObject : <color=cyan>" + this.gameObject.name + "</color>, <b><color=yellow>LongTap</color></b>");
				}
			}
		}
	}
}