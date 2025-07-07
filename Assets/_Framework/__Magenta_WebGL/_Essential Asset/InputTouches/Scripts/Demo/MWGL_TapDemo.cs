using UnityEngine;
using System.Collections;

namespace _Magenta_WebGL
{
	public class MWGL_TapDemo : _Magenta_Framework.TapDemoEx
    {
		private static string LOG_FORMAT = "<color=#AB3030><b>[MWGL_TapDemo]</b></color> {0}";

        protected override void Awake()
		{
			base.Awake();
		}

		protected override void OnEnable()
		{
			//these events are obsolete, replaced by onMultiTapE, but it's still usable
			//IT_Gesture.onShortTapE += OnShortTap;
			//IT_Gesture.onDoubleTapE += OnDoubleTap;

			MWGL_IT_Gesture.onMultiTapE += OnMultiTap;
            MWGL_IT_Gesture.onLongTapE += OnLongTap;

            MWGL_IT_Gesture.onChargingE += OnCharging;
            MWGL_IT_Gesture.onChargeEndE += OnChargeEnd;

            MWGL_IT_Gesture.onDraggingStartE += OnDraggingStart;
            MWGL_IT_Gesture.onDraggingE += OnDragging;
            MWGL_IT_Gesture.onDraggingEndE += OnDraggingEnd;
		}

		protected override void OnDisable()
		{
            // these events are obsolete, replaced by onMultiTapE, but it's still usable
            // IT_GestureEx.onShortTapE -= OnShortTap;
            // IT_GestureEx.onDoubleTapE -= OnDoubleTap;

            MWGL_IT_Gesture.onMultiTapE -= OnMultiTap;
            MWGL_IT_Gesture.onLongTapE -= OnLongTap;

            MWGL_IT_Gesture.onChargingE -= OnCharging;
            MWGL_IT_Gesture.onChargeEndE -= OnChargeEnd;

            MWGL_IT_Gesture.onDraggingStartE -= OnDraggingStart;
            MWGL_IT_Gesture.onDraggingE -= OnDragging;
            MWGL_IT_Gesture.onDraggingEndE -= OnDraggingEnd;
		}

        protected override void OnMultiTap(Tap tap)
		{
			Debug.LogFormat(LOG_FORMAT, "OnMultiTap(), tap : " + tap.count);

			Ray ray = Cam.ScreenPointToRay(tap.pos);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, Mathf.Infinity))
			{
				//if the tap lands on the shortTapObj, then shows the effect.
				if (hit.collider.transform == shortTapObj)
				{
                    //place the indicator at the object position and assign a random color to it
                    IndicatorPS.transform.position = shortTapObj.position;

					var main = IndicatorPS.main;
					main.startColor = GetRandomColor(); //Indicator.startColor=GetRandomColor();

                    //emit a set number of particle
                    IndicatorPS.Emit(30);
				}
				else if (hit.collider.transform == doubleTapObj)
				{
					if (tap.count == 2)
					{
                        //place the indicator at the object position and assign a random color to it
                        IndicatorPS.transform.position = doubleTapObj.position;

						var main = IndicatorPS.main;
						main.startColor = GetRandomColor(); //Indicator.startColor=GetRandomColor();

                        //emit a set number of particle
                        IndicatorPS.Emit(30);
					}
				}
			}
		}

		//called when a long tap event is ended
		protected override void OnLongTap(Tap tap)
		{
			Debug.LogFormat(LOG_FORMAT, "OnLongTap()");

			//do a raycast base on the position of the tap
			Ray ray = Cam.ScreenPointToRay(tap.pos);
			RaycastHit hit;
			//if the tap lands on the longTapObj
			if (Physics.Raycast(ray, out hit, Mathf.Infinity))
			{
				if (hit.collider.transform == longTapObj)
				{
                    //place the indicator at the object position and assign a random color to it
                    IndicatorPS.transform.position = longTapObj.position;

					var main = IndicatorPS.main;
					main.startColor = GetRandomColor(); //Indicator.startColor=GetRandomColor();

                    //emit a set number of particle
                    IndicatorPS.Emit(30);
				}
			}
		}

		protected override void OnCharging(ChargedInfo cInfo)
		{
			// Ray ray = Camera.main.ScreenPointToRay(cInfo.pos);
			Ray ray = Cam.ScreenPointToRay(cInfo.pos);
			RaycastHit hit;
			//use raycast at the cursor position to detect the object
			if (Physics.Raycast(ray, out hit, Mathf.Infinity))
			{
				if (hit.collider.transform == chargeObj)
				{
					//display the charged percentage on screen
					chargeTextMesh.text = "Charging " + (cInfo.percent * 100).ToString("f1") + "%";
				}
			}
		}

		protected override void OnChargeEnd(ChargedInfo cInfo)
		{
			// Ray ray = Camera.main.ScreenPointToRay(cInfo.pos);
			Ray ray = Cam.ScreenPointToRay(cInfo.pos);
			RaycastHit hit;
			//use raycast at the cursor position to detect the object
			if (Physics.Raycast(ray, out hit, Mathf.Infinity))
			{
				if (hit.collider.transform == chargeObj)
				{
                    //place the indicator at the object position and assign a random color to it
                    IndicatorPS.transform.position = chargeObj.position;

					var main = IndicatorPS.main;
					main.startColor = GetRandomColor(); //Indicator.startColor=GetRandomColor();

					//adjust the indicator speed with respect to the charged percent
					main.startSpeed = 1 + 3 * cInfo.percent;    //Indicator.startSpeed=1+3*cInfo.percent;

                    //emit a set number of particles with respect to the charged percent
                    IndicatorPS.Emit((int)(10 + cInfo.percent * 75f));

					//reset the particle speed, since it's shared by other event
					StartCoroutine(ResumeSpeed());
				}
			}
			chargeTextMesh.text = "HoldToCharge";
		}

		protected override void OnDraggingStart(DragInfo dragInfo)
		{
			//currentDragIndex=dragInfo.index;

			//if(currentDragIndex==-1){
			// Ray ray = Camera.main.ScreenPointToRay(dragInfo.pos);
			Ray ray = Cam.ScreenPointToRay(dragInfo.pos);
			RaycastHit hit;
			//use raycast at the cursor position to detect the object
			if (Physics.Raycast(ray, out hit, Mathf.Infinity) == true)
			{
				//if the drag started on dragObj1
				if (hit.collider.transform == dragObj1)
				{
					Vector3 p = Cam.ScreenToWorldPoint(new Vector3(dragInfo.pos.x, dragInfo.pos.y, 30));
					dragOffset1 = dragObj1.position - p;

					//change the scale of dragObj1, give the user some visual feedback
					dragObj1.localScale *= 1.1f;
					//latch dragObj1 to the cursor, based on the index
					Obj1ToCursor(dragInfo);
					currentDragIndex1 = dragInfo.index;
				}
				//if the drag started on dragObj2
				else if (hit.collider.transform == dragObj2)
				{
					Vector3 p = Cam.ScreenToWorldPoint(new Vector3(dragInfo.pos.x, dragInfo.pos.y, 30));
					dragOffset2 = dragObj2.position - p;

					//change the scale of dragObj2, give the user some visual feedback
					dragObj2.localScale *= 1.1f;
					//latch dragObj2 to the cursor, based on the index
					Obj2ToCursor(dragInfo);
					currentDragIndex2 = dragInfo.index;
				}
			}
			//}
		}

		protected override void Obj1ToCursor(DragInfo dragInfo)
		{
			Vector3 p = Cam.ScreenToWorldPoint(new Vector3(dragInfo.pos.x, dragInfo.pos.y, 30));
			dragObj1.position = p + dragOffset1;

			if (dragInfo.isMouse == true)
			{
				dragTextMesh1.text = "Dragging with mouse" + (dragInfo.index + 1);
			}
			else
			{
				dragTextMesh1.text = "Dragging with finger" + (dragInfo.index + 1);
			}
		}

		protected override void Obj2ToCursor(DragInfo dragInfo)
		{
			Vector3 p = Cam.ScreenToWorldPoint(new Vector3(dragInfo.pos.x, dragInfo.pos.y, 30));
			dragObj2.position = p + dragOffset2;

			if (dragInfo.isMouse == true)
			{
				dragTextMesh2.text = "Dragging with mouse" + (dragInfo.index + 1);
			}
			else
			{
				dragTextMesh2.text = "Dragging with finger" + (dragInfo.index + 1);
			}
		}

		protected override void OnDraggingEnd(DragInfo dragInfo)
		{
			//drop the dragObj being drag by this particular cursor
			if (dragInfo.index == currentDragIndex1)
			{
				currentDragIndex1 = -1;
				dragObj1.localScale *= 10f / 11f;

				Vector3 p = Cam.ScreenToWorldPoint(new Vector3(dragInfo.pos.x, dragInfo.pos.y, 30));
				dragObj1.position = p + dragOffset1;
				dragTextMesh1.text = "DragMe";
			}
			else if (dragInfo.index == currentDragIndex2)
			{
				currentDragIndex2 = -1;
				dragObj2.localScale *= 10f / 11f;

				Vector3 p = Cam.ScreenToWorldPoint(new Vector3(dragInfo.pos.x, dragInfo.pos.y, 30));
				dragObj2.position = p + dragOffset2;
				dragTextMesh2.text = "DragMe";
			}

		}
	}
}