using UnityEngine;
using System.Collections;

namespace UnityChan
{
	public class _FaceUpdate : FaceUpdate
    {
		private static string LOG_FORMAT = "<color=#9EFF00><b>[_FaceUpdate]</b></color> {0}";

		[ReadOnly]
		[SerializeField]
		protected Animator _animator;

		protected virtual void Awake()
		{
			Debug.LogFormat(LOG_FORMAT, "Awake()");
		}

		protected override void Start()
		{
			_animator = GetComponent<Animator>();
		}

		protected override void Update()
		{

			if (Input.GetMouseButton(0))
			{
				current = 1;
			}
			else if (!isKeepFace)
			{
				current = Mathf.Lerp(current, 0, delayWeight);
			}
			_animator.SetLayerWeight(1, current);
		}

		public override void OnCallChangeFace(string str)
		{
			int ichecked = 0;
			foreach (var animation in animations)
			{
				if (str == animation.name)
				{
					ChangeFace(str);
					break;
				}
				else if (ichecked <= animations.Length)
				{
					ichecked++;
				}
				else
				{
					//str指定が間違っている時にはデフォルトで
					str = "default@unitychan";
					ChangeFace(str);
				}
			}
		}

		protected override void ChangeFace(string str)
		{
			isKeepFace = true;
			current = 1;
			_animator.CrossFade(str, 0);
		}

		protected override void OnGUI()
		{
			GUILayout.Box("Face Update", GUILayout.Width(170), GUILayout.Height(25 * (animations.Length + 2)));
			Rect screenRect = new Rect(10, 25, 150, 25 * (animations.Length + 1));
			GUILayout.BeginArea(screenRect);
			foreach (var animation in animations)
			{
				if (GUILayout.RepeatButton(animation.name))
				{
					_animator.CrossFade(animation.name, 0);
				}
			}
			isKeepFace = GUILayout.Toggle(isKeepFace, " Keep Face");
			GUILayout.EndArea();
		}
	}
}
