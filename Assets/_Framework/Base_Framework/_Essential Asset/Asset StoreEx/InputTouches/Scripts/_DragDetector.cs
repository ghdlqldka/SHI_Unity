using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//using InputTouches;

namespace _InputTouches
{

	public class _DragDetector : DragDetector
	{
		protected virtual void Awake()
		{
			//
		}

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
		{
			if (Input.touchCount <= 1)
			{
				multiDragCount = 1;
			}

			//drag event detection goes here
			if (Input.touchCount > 0)
			{
				if (enableMultiDrag == true)
				{
					//foreach(Touch touch in Input.touches){
					for (int i = 0; i < Input.touches.Length; i++)
					{
						Touch touch = Input.touches[i];
						if (fingerIndex.Count == 0 || fingerIndex.Contains(touch.fingerId) == false)
						{
							StartCoroutine(TouchRoutine(touch.fingerId));
						}
					}
				}
				else
				{
					if (Input.touchCount == 1)
					{
						if (fingerIndex.Count == 0)
						{
							StartCoroutine(TouchRoutine(Input.touches[0].fingerId));
						}
					}
				}

				if (Input.touchCount > 1 && Input.touchCount != multiDragCount)
				{
					multiDragCount = Input.touchCount;
					StartCoroutine(MultiDragRoutine(Input.touchCount));
				}
			}
			else if (Input.touchCount == 0)
			{
				if (Input.GetMouseButtonDown(_IT_Gesture.Mouse_Left_Button))
				{
					if (mouseIndex.Contains(_IT_Gesture.Mouse_Left_Button) == false)
					{
						StartCoroutine(MouseRoutine(_IT_Gesture.Mouse_Left_Button));
					}
				}
				else if (Input.GetMouseButtonUp(_IT_Gesture.Mouse_Left_Button))
				{
					if (mouseIndex.Contains(_IT_Gesture.Mouse_Left_Button))
					{
						mouseIndex.Remove(_IT_Gesture.Mouse_Left_Button);
					}
				}

				if (Input.GetMouseButtonDown(_IT_Gesture.Mouse_Right_Button))
				{
					if (mouseIndex.Contains(_IT_Gesture.Mouse_Right_Button) == false)
					{
						StartCoroutine(MouseRoutine(_IT_Gesture.Mouse_Right_Button));
					}
				}
				else if (Input.GetMouseButtonUp(_IT_Gesture.Mouse_Right_Button))
				{
					if (mouseIndex.Contains(_IT_Gesture.Mouse_Right_Button))
					{
						mouseIndex.Remove(_IT_Gesture.Mouse_Right_Button);
					}
				}

				if (Input.GetMouseButtonDown(_IT_Gesture.Mouse_Middle_Button))
				{
					if (mouseIndex.Contains(_IT_Gesture.Mouse_Middle_Button) == false)
					{
						StartCoroutine(MouseRoutine(_IT_Gesture.Mouse_Middle_Button));
					}
				}
				else if (Input.GetMouseButtonUp(_IT_Gesture.Mouse_Middle_Button))
				{
					if (mouseIndex.Contains(_IT_Gesture.Mouse_Middle_Button))
					{
						mouseIndex.Remove(_IT_Gesture.Mouse_Middle_Button);
					}
				}

				if (Input.GetMouseButtonDown(3))
				{
					if (mouseIndex.Contains(3) == false)
					{
						StartCoroutine(MouseRoutine(3));
					}
				}
				else if (Input.GetMouseButtonUp(3))
				{
					if (mouseIndex.Contains(3))
					{
						mouseIndex.Remove(3);
					}
				}
			}
		}

		protected override IEnumerator MultiDragRoutine(int count)
		{
			if (count <= 1)
			{
				yield break;
			}

			bool dragStarted = false;

			Vector2 startPos = Vector2.zero;
			for (int i = 0; i < Input.touchCount; i++)
			{
				startPos += Input.touches[i].position;
			}
			startPos /= Input.touchCount;
			Vector2 lastPos = startPos;

			float timeStart = Mathf.Infinity;

#if UNITY_ANDROID || UNITY_IOS
		Vector2[] lastPoss=new Vector2[count];
		for(int i=0; i<count; i++){
			lastPoss[i]=Input.touches[i].position;
		}
#endif

			while (Input.touchCount == count)
			{
				Vector2 curPos = Vector2.zero;
				Vector2[] allPos = new Vector2[count];
				bool moving = true;
				for (int i = 0; i < count; i++)
				{
					Touch touch = Input.touches[i];
					curPos += touch.position;
					allPos[i] = touch.position;
					if (touch.phase != TouchPhase.Moved) moving = false;
				}
				curPos /= count;

				bool sync = true;
				if (moving)
				{
					for (int i = 0; i < count - 1; i++)
					{
#if UNITY_ANDROID || UNITY_IOS
					Vector2 v1=(Input.touches[i].position+lastPoss[i]).normalized;
					Vector2 v2=(Input.touches[i+1].position+lastPoss[i+1]).normalized;
#else
						Vector2 v1 = Input.touches[i].deltaPosition.normalized;
						Vector2 v2 = Input.touches[i + 1].deltaPosition.normalized;
#endif
						if (Vector2.Dot(v1, v2) < 0.85f) sync = false;
					}
				}

				if (moving && sync)
				{
					if (!dragStarted)
					{
						if (Vector2.Distance(curPos, startPos) > minDragDistance * IT_Gesture.GetDPIFactor())
						{
							dragStarted = true;
							Vector2 delta = curPos - startPos;
							DragInfo dragInfo = new DragInfo(curPos, delta, count);
							IT_Gesture.DraggingStart(dragInfo);

							timeStart = Time.realtimeSinceStartup;
						}
					}
					else
					{
						if (curPos != lastPos)
						{
							Vector2 delta = curPos - lastPos;
							DragInfo dragInfo = new DragInfo(curPos, delta, count);
							IT_Gesture.Dragging(dragInfo);
						}
					}
				}
				else if (dragStarted && fireOnDraggingWhenNotMoving)
				{
					DragInfo dragInfo = new DragInfo(curPos, Vector2.zero, count);
					IT_Gesture.Dragging(dragInfo);
				}

				lastPos = curPos;
#if UNITY_ANDROID || UNITY_IOS
			for(int i=0; i<count; i++){
				lastPoss[i]=Input.touches[i].position;
			}
#endif

				yield return null;
			}

			if (dragStarted)
			{
				bool isFlick = false;
				if (Time.realtimeSinceStartup - timeStart < 0.5f)
					isFlick = true;

				Vector2 delta = lastPos - startPos;
				DragInfo dragInfo = new DragInfo(lastPos, delta, count, isFlick);
				IT_Gesture.DraggingEnd(dragInfo);
			}

		}

		protected override IEnumerator MouseRoutine(int index)
		{
			mouseIndex.Add(index);

			bool dragStarted = false;

			Vector2 startPos = Input.mousePosition;
			Vector2 lastPos = startPos;

			float timeStart = Mathf.Infinity;

			int fingerCount = 1;

			while (mouseIndex.Contains(index))
			{
				Vector2 curPos = Input.mousePosition;

				if (dragStarted == false)
				{
					if (Vector3.Distance(curPos, startPos) > minDragDistance * IT_Gesture.GetDPIFactor())
					{
						dragStarted = true;
						Vector2 delta = curPos - startPos;
						DragInfo dragInfo = new DragInfo(curPos, delta, fingerCount, index, true);
						IT_Gesture.DraggingStart(dragInfo);

						timeStart = Time.realtimeSinceStartup;
					}
				}
				else
				{
					if (curPos != lastPos)
					{
						Vector2 delta = curPos - lastPos;
						DragInfo dragInfo = new DragInfo(curPos, delta, fingerCount, index, true);
						IT_Gesture.Dragging(dragInfo);
					}
					else if (fireOnDraggingWhenNotMoving)
					{
						DragInfo dragInfo = new DragInfo(curPos, Vector2.zero, fingerCount, index, true);
						IT_Gesture.Dragging(dragInfo);
					}
				}

				lastPos = curPos;

				yield return null;
			}

			if (dragStarted == true)
			{
				bool isFlick = false;
				if (Time.realtimeSinceStartup - timeStart < 0.5f)
					isFlick = true;

				Vector2 delta = lastPos - startPos;
				DragInfo dragInfo = new DragInfo(lastPos, delta, fingerCount, index, isFlick, true);

				IT_Gesture.DraggingEnd(dragInfo);
			}
		}

	}
}