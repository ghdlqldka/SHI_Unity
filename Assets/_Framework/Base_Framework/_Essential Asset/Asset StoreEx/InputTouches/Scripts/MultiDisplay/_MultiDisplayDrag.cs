using UnityEngine;
using System.Collections;

namespace _InputTouches
{
	public class _MultiDisplayDrag : MonoBehaviour
	{
		private static string LOG_FORMAT = "<b><color=#0DEC90>[_MultiDisplay</color><color=red>Drag</color><color=#0DEC90>]</color></b> {0}";

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

		protected Vector3 dragOffset;

		protected int currentDragIndex = -1;

		protected virtual void Awake()
		{
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
			_MultiDisplayInputManager.OnDraggingStart += OnDraggingStart;
			_MultiDisplayInputManager.OnDragging += OnDragging;
			_MultiDisplayInputManager.OnDraggingEnd += OnDraggingEnd;
		}

		protected virtual void OnDisable()
		{
			_MultiDisplayInputManager.OnDraggingStart -= OnDraggingStart;
			_MultiDisplayInputManager.OnDragging -= OnDragging;
			_MultiDisplayInputManager.OnDraggingEnd -= OnDraggingEnd;
		}

		protected virtual void OnDraggingStart(DragInfo dragInfo, _MultiDisplayInputManager.DisplayIndex display)
		{
			// Debug.LogFormat(LOG_FORMAT, "OnDraggingStart(), this.gameObject : <color=cyan>" + this.gameObject.name + "</color>, display : <b><color=yellow>" + display + "</color></b>");
			//currentDragIndex=dragInfo.index;

			//if(currentDragIndex==-1){
			Camera camera = MultiDisplayInputManager.GetDiplayCamera(display);
			Ray ray = camera.ScreenPointToRay(dragInfo.pos);
			RaycastHit hit;
			//use raycast at the cursor position to detect the object
			if (Physics.Raycast(ray, out hit, Mathf.Infinity) == true)
			{
				//if the drag started on dragObj1
				if (hit.collider.transform == this.transform)
				{
					Vector3 p = camera.ScreenToWorldPoint(new Vector3(dragInfo.pos.x, dragInfo.pos.y, 30));
					dragOffset = this.transform.position - p;

					//change the scale of dragObj1, give the user some visual feedback
					this.transform.localScale *= 1.1f;
					this.transform.position = p + dragOffset;
					currentDragIndex = dragInfo.index;
				}
			}
			//}
		}

#if DEBUG
		protected bool DEBUG_flag = false;
#endif
		//triggered on a single-finger/mouse dragging event is on-going
		protected virtual void OnDragging(DragInfo dragInfo, _MultiDisplayInputManager.DisplayIndex display)
		{
			//if the dragInfo index matches dragIndex1, call function to position dragObj1 accordingly
			if (dragInfo.index == currentDragIndex)
			{
#if DEBUG
				if (DEBUG_flag == false)
				{
					Debug.LogFormat(LOG_FORMAT, "OnDragging(), this.gameObject : <color=cyan>" + this.gameObject.name + "</color>, display : <b><color=yellow>" + display + "</color></b>");
					DEBUG_flag = true;
				}
#endif

				Camera camera = MultiDisplayInputManager.GetDiplayCamera(display);
				Vector3 p = camera.ScreenToWorldPoint(new Vector3(dragInfo.pos.x, dragInfo.pos.y, 30));
				this.transform.position = p + dragOffset;
			}
		}

		protected virtual void OnDraggingEnd(DragInfo dragInfo, _MultiDisplayInputManager.DisplayIndex display)
		{
			Camera camera = MultiDisplayInputManager.GetDiplayCamera(display);
			//drop the dragObj being drag by this particular cursor
			if (dragInfo.index == currentDragIndex)
			{
				currentDragIndex = -1;
				this.transform.localScale *= 10f / 11f;

				Vector3 p = camera.ScreenToWorldPoint(new Vector3(dragInfo.pos.x, dragInfo.pos.y, 30));
				this.transform.position = p + dragOffset;
			}
#if DEBUG
			DEBUG_flag = false;
#endif
		}
	}
}