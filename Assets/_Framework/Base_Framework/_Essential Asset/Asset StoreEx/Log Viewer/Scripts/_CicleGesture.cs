#if UNITY_CHANGE1 || UNITY_CHANGE2 || UNITY_CHANGE3 || UNITY_CHANGE4
#warning UNITY_CHANGE has been set manually
#elif UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#define UNITY_CHANGE1
#elif UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define UNITY_CHANGE2
#else
#define UNITY_CHANGE3
#endif

#if UNITY_2018_3_OR_NEWER
#define UNITY_CHANGE4
#endif
//use UNITY_CHANGE1 for unity older than "unity 5"
//use UNITY_CHANGE2 for unity 5.0 -> 5.3 
//use UNITY_CHANGE3 for unity 5.3 (fix for new SceneManger system)
//use UNITY_CHANGE4 for unity 2018.3 (Networking system)

using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using AYellowpaper.SerializedCollections;

namespace _Base_Framework
{
	public class _CicleGesture : MonoBehaviour
	{
		public int _numOfCircleToShow = 1;

		public delegate void Detect();
		public static event Detect OnDetect;

		[Space(10)]
		[ReadOnly]
		[SerializeField]
		protected List<Vector2> _gestureDetector = new List<Vector2>();
		[ReadOnly]
		[SerializeField]
		protected Vector2 _gestureSum = Vector2.zero;
		[ReadOnly]
		[SerializeField]
		protected float _gestureLength = 0;
		[ReadOnly]
		[SerializeField]
		protected int _gestureCount = 0;

		protected virtual void Update()
		{
			CheckGesture();
		}

		protected virtual void CheckGesture()
		{
			if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
			{
				if (Input.touches.Length != 1)
				{
					_gestureDetector.Clear();
					_gestureCount = 0;
				}
				else
				{
					if (Input.touches[0].phase == TouchPhase.Canceled || Input.touches[0].phase == TouchPhase.Ended)
					{
						_gestureDetector.Clear();
					}
					else if (Input.touches[0].phase == TouchPhase.Moved)
					{
						Vector2 p = Input.touches[0].position;
						if (_gestureDetector.Count == 0 || (p - _gestureDetector[_gestureDetector.Count - 1]).magnitude > 10)
						{
							_gestureDetector.Add(p);
						}
					}
				}
			}
			else
			{
				if (Input.GetMouseButtonUp(0))
				{
					_gestureDetector.Clear();
					_gestureCount = 0;
				}
				else
				{
					if (Input.GetMouseButton(0) == true)
					{
						Vector2 p = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
						if (_gestureDetector.Count == 0 || (p - _gestureDetector[_gestureDetector.Count - 1]).magnitude > 10)
						{
							_gestureDetector.Add(p);
						}
					}
				}
			}

			if (_gestureDetector.Count < 10)
			{
				return;
			}

			_gestureSum = Vector2.zero;
			_gestureLength = 0;
			Vector2 prevDelta = Vector2.zero;
			for (int i = 0; i < _gestureDetector.Count - 2; i++)
			{
				Vector2 delta = _gestureDetector[i + 1] - _gestureDetector[i];
				float deltaLength = delta.magnitude;
				_gestureSum += delta;
				_gestureLength += deltaLength;

				float dot = Vector2.Dot(delta, prevDelta);
				if (dot < 0f)
				{
					_gestureDetector.Clear();
					_gestureCount = 0;
					return;
				}

				prevDelta = delta;
			}

			int gestureBase = (Screen.width + Screen.height) / 4;

			if (_gestureLength > gestureBase && _gestureSum.magnitude < gestureBase / 2)
			{
				_gestureDetector.Clear();
				_gestureCount++;
				if (_gestureCount >= _numOfCircleToShow)
				{
					if (OnDetect != null)
					{
						OnDetect();
					}
					return;
				}
			}

			return;
		}
	}


}