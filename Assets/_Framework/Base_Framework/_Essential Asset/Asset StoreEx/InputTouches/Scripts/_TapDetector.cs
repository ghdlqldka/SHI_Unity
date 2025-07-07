using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace _InputTouches
{

	public class _TapDetector : TapDetector
	{
		protected override void Awake()
		{
			instance = null; // Not used!!!!!

			multiTapMouse = new MultiTapTracker[3];
		}

		// Use this for initialization
		protected override void Start()
		{

			if (enableMultiTapFilter == true)
			{
				IT_Gesture.SetMultiTapFilter(enableMultiTapFilter);
				IT_Gesture.SetMaxMultiTapCount(maxMultiTapCount);
				IT_Gesture.SetMaxMultiTapInterval(multiTapInterval);
			}

			for (int i = 0; i < multiTapMouse.Length; i++)
			{
				multiTapMouse[i] = new MultiTapTracker(i);
			}
			for (int i = 0; i < multiTapTouch.Length; i++)
			{
				multiTapTouch[i] = new MultiTapTracker(i);
			}
			for (int i = 0; i < multiTapMFTouch.Length; i++)
			{
				multiTapMFTouch[i] = new MultiTapTracker(i);
			}

			StartCoroutine(CheckMultiTapCount());
			StartCoroutine(MultiFingerRoutine());

			//Debug.Log(IT_Gesture.GetTouch(0).position);
		}

		protected override void Update()
		{
			// Debug.Log("Input.touchCount : " + Input.touchCount + ", Input.touches.Length : " + Input.touches.Length);

			if (Input.touchCount > 0)
			{
				if (fingerIndex.Count < Input.touchCount)
				{
					for (int i = 0; i < Input.touches.Length; i++)
					{
						Touch touch = Input.touches[i];
						if (fingerIndex.Contains(touch.fingerId) == false)
						{
							CheckFingerGroup(touch);
						}
					}
				}

				for (int i = 0; i < Input.touches.Length; i++)
				{
					Touch touch = Input.touches[i];
					if (fingerIndex.Count == 0 || (fingerIndex.Contains(touch.fingerId) == false))
					{
						// Debug.Log("touch.fingerId : " + touch.fingerId);
						StartCoroutine(FingerRoutine(touch.fingerId));
					}
				}
			}
			else if (Input.touchCount == 0)
			{
				if (Input.GetMouseButtonDown(_IT_Gesture.Mouse_Left_Button))
				{
					if (mouseIndex.Contains(_IT_Gesture.Mouse_Left_Button) == false)
						StartCoroutine(MouseRoutine(_IT_Gesture.Mouse_Left_Button));
				}
				else if (Input.GetMouseButtonUp(_IT_Gesture.Mouse_Left_Button))
				{
					if (mouseIndex.Contains(_IT_Gesture.Mouse_Left_Button))
						mouseIndex.Remove(_IT_Gesture.Mouse_Left_Button);
				}

				if (Input.GetMouseButtonDown(_IT_Gesture.Mouse_Right_Button))
				{
					if (mouseIndex.Contains(_IT_Gesture.Mouse_Right_Button) == false)
						StartCoroutine(MouseRoutine(_IT_Gesture.Mouse_Right_Button));
				}
				else if (Input.GetMouseButtonUp(_IT_Gesture.Mouse_Right_Button))
				{
					if (mouseIndex.Contains(_IT_Gesture.Mouse_Right_Button))
						mouseIndex.Remove(_IT_Gesture.Mouse_Right_Button);
				}

				if (Input.GetMouseButtonDown(_IT_Gesture.Mouse_Middle_Button))
				{
					if (mouseIndex.Contains(_IT_Gesture.Mouse_Middle_Button) == false)
						StartCoroutine(MouseRoutine(_IT_Gesture.Mouse_Middle_Button));
				}
				else if (Input.GetMouseButtonUp(_IT_Gesture.Mouse_Middle_Button))
				{
					if (mouseIndex.Contains(_IT_Gesture.Mouse_Middle_Button))
						mouseIndex.Remove(_IT_Gesture.Mouse_Middle_Button);
				}
			}
		}

		protected override IEnumerator CheckMultiTapCount()
		{
			while (true)
			{
				//foreach(MultiTapTracker multiTap in multiTapMouse){
				for (int i = 0; i < multiTapMouse.Length; i++)
				{
					MultiTapTracker multiTap = multiTapMouse[i];
					if (multiTap.count > 0)
					{
						if (multiTap.lastTapTime + multiTapInterval < Time.realtimeSinceStartup)
						{
							multiTap.count = 0;
							multiTap.lastPos = Vector2.zero;
						}
					}
				}
				//foreach(MultiTapTracker multiTap in multiTapTouch){
				for (int i = 0; i < multiTapTouch.Length; i++)
				{
					MultiTapTracker multiTap = multiTapTouch[i];
					if (multiTap.count > 0)
					{
						if (multiTap.lastTapTime + multiTapInterval < Time.realtimeSinceStartup)
						{
							multiTap.count = 0;
							multiTap.lastPos = Vector2.zero;
						}
					}
				}
				//foreach(MultiTapTracker multiTap in multiTapMFTouch){
				for (int i = 0; i < multiTapMFTouch.Length; i++)
				{
					MultiTapTracker multiTap = multiTapMFTouch[i];
					if (multiTap.count > 0)
					{
						if (multiTap.lastTapTime + multiTapInterval < Time.realtimeSinceStartup)
						{
							multiTap.count = 0;
							multiTap.lastPos = Vector2.zero;
							multiTap.fingerCount = 1;
						}
					}
				}

				yield return null;
			}
		}

		protected override void CheckFingerGroup(Touch touch)
		{
			//Debug.Log("Checking "+Time.realtimeSinceStartup);
			bool match = false;
			//foreach(FingerGroup group in fingerGroup){
			for (int i = 0; i < fingerGroup.Count; i++)
			{
				FingerGroup group = fingerGroup[i];
				if (Time.realtimeSinceStartup - group.triggerTime < shortTapTime / 2)
				{
					bool inRange = true;
					float dist = 0;
					//foreach(int index in group.indexes){
					for (int j = 0; j < group.indexes.Count; j++)
					{
						int index = group.indexes[j];
						dist = Vector2.Distance(IT_Gesture.GetTouch(index).position, touch.position);
						if (/*Vector2.Distance(IT_Gesture.GetTouch(index).position, touch.position)*/dist > maxFingerGroupDist * IT_Gesture.GetDPIFactor())
							inRange = false;
					}

					if (inRange)
					{
						group.indexes.Add(touch.fingerId);
						group.positions.Add(touch.position);
						match = true;
						break;
					}
				}
			}

			if (match == false)
			{
				fingerGroup.Add(new FingerGroup(Time.realtimeSinceStartup, touch.fingerId, touch.position));
				StartCoroutine(fingerGroup[fingerGroup.Count - 1].Routine(this));
			}
		}

		protected override IEnumerator FingerRoutine(int index)
		{
			// Debug.Log("FingerRoutine(), index : " + index);
			fingerIndex.Add(index);

			//init tap variables
			Touch touch = IT_Gesture.GetTouch(index);
			float startTime = Time.realtimeSinceStartup;
			Vector2 startPos = touch.position;
			Vector2 lastPos = startPos;
			bool longTap = false;

			//init charge variables
			_ChargeState chargeState = _ChargeState.Clear;
			int chargeDir = 1;
			int chargeConst = 0;
			float startTimeCharge = Time.realtimeSinceStartup;
			Vector2 startPosCharge = touch.position;

			//yield return null;

			bool inGroup = false;

			while (true)
			{
				touch = IT_Gesture.GetTouch(index);
				if (touch.position == Vector2.zero) break;

				Vector2 curPos = touch.position;

				if (Time.realtimeSinceStartup - startTimeCharge > minChargeTime && chargeState == _ChargeState.Clear)
				{
					chargeState = _ChargeState.Charging;
					float chargedValue = Mathf.Clamp(chargeConst + chargeDir * ((Time.realtimeSinceStartup - startTimeCharge) / maxChargeTime), 0, 1);
					ChargedInfo cInfo = new ChargedInfo(curPos, chargedValue, index, false);
					IT_Gesture.ChargeStart(cInfo);

					startPosCharge = curPos;
				}
				else if (chargeState == _ChargeState.Charging)
				{
					if (Vector3.Distance(curPos, startPosCharge) > tapPosDeviation)
					{
						chargeState = _ChargeState.Clear;
						float chargedValue = Mathf.Clamp(chargeConst + chargeDir * ((Time.realtimeSinceStartup - startTimeCharge) / maxChargeTime), 0, 1);
						ChargedInfo cInfo = new ChargedInfo(lastPos, chargedValue, index, false);
						IT_Gesture.ChargeEnd(cInfo);
					}
					else
					{
						float chargedValue = Mathf.Clamp(chargeConst + chargeDir * ((Time.realtimeSinceStartup - startTimeCharge) / maxChargeTime), 0, 1);
						ChargedInfo cInfo = new ChargedInfo(curPos, chargedValue, index, false);

						if (chargeMode == _ChargeMode.PingPong)
						{
							if (chargedValue == 1 || chargedValue == 0)
							{
								chargeDir *= -1;
								if (chargeDir == 1) chargeConst = 0;
								else if (chargeDir == -1) chargeConst = 1;
								startTimeCharge = Time.realtimeSinceStartup;
							}

							IT_Gesture.Charging(cInfo);
						}
						else
						{
							if (chargedValue < 1.0f)
							{
								IT_Gesture.Charging(cInfo);
							}
							else
							{
								cInfo.percent = 1.0f;

								if (chargeMode == _ChargeMode.Once)
								{
									chargeState = _ChargeState.Charged;
									IT_Gesture.ChargeEnd(cInfo);
									startTimeCharge = Mathf.Infinity;
									chargedValue = 0;
								}
								else if (chargeMode == _ChargeMode.Clamp)
								{
									chargeState = _ChargeState.Charged;
									IT_Gesture.Charging(cInfo);
								}
								else if (chargeMode == _ChargeMode.Loop)
								{
									chargeState = _ChargeState.Clear;
									IT_Gesture.ChargeEnd(cInfo);
									startTimeCharge = Time.realtimeSinceStartup;
								}
							}

						}
					}
				}

				if (longTap == false && Time.realtimeSinceStartup - startTime > longTapTime && Vector2.Distance(lastPos, startPos) < maxTapDisplacementAllowance * IT_Gesture.GetDPIFactor())
				{
					//new Tap(multiTapMFTouch[index].count, fCount, posL)
					//IT_Gesture.LongTap(new Tap(multiTapMFTouch[index].count, fCount, posL));
					IT_Gesture.LongTap(new Tap(curPos, 1, index, false));
					//IT_Gesture.LongTap(startPos);
					longTap = true;
				}

				lastPos = curPos;

				if (!inGroup)
					inGroup = IndexInFingerGroup(index);

				yield return null;
			}

			//check for shortTap
			if (inGroup == false)
			{
				if (Time.realtimeSinceStartup - startTime <= shortTapTime && Vector2.Distance(lastPos, startPos) < maxTapDisplacementAllowance * IT_Gesture.GetDPIFactor())
				{
					CheckMultiTapTouch(index, startPos, lastPos);
				}
			}

			//check for charge
			if (chargeState == _ChargeState.Charging || (chargeState == _ChargeState.Charged && chargeMode != _ChargeMode.Once))
			{
				float chargedValue = Mathf.Clamp(chargeConst + chargeDir * ((Time.realtimeSinceStartup - startTimeCharge) / maxChargeTime), 0, 1);
				ChargedInfo cInfo = new ChargedInfo(lastPos, chargedValue, index, false);
				IT_Gesture.ChargeEnd(cInfo);
			}

			fingerIndex.Remove(index);
		}

		protected override IEnumerator MouseRoutine(int index)
		{
			mouseIndex.Add(index);

			//init tap variables
			float startTime = Time.realtimeSinceStartup;
			Vector2 startPos = Input.mousePosition;
			Vector2 lastPos = startPos;
			bool longTap = false;

			//init charge variables
			_ChargeState chargeState = _ChargeState.Clear;
			int chargeDir = 1;
			float chargeConst = 0;
			float startTimeCharge = Time.realtimeSinceStartup;
			Vector2 startPosCharge = Input.mousePosition;

			yield return null;

			while (mouseIndex.Contains(index))
			{

				Vector2 curPos = Input.mousePosition;

				if (Time.realtimeSinceStartup - startTimeCharge > minChargeTime && chargeState == _ChargeState.Clear)
				{
					chargeState = _ChargeState.Charging;
					float chargedValue = Mathf.Clamp(chargeConst + chargeDir * ((Time.realtimeSinceStartup - startTimeCharge) / maxChargeTime), 0, 1);
					ChargedInfo cInfo = new ChargedInfo(curPos, chargedValue, index, true);
					IT_Gesture.ChargeStart(cInfo);

					startPosCharge = curPos;
				}
				else if (chargeState == _ChargeState.Charging)
				{
					if (Vector3.Distance(curPos, startPosCharge) > tapPosDeviation)
					{
						chargeState = _ChargeState.Clear;
						float chargedValue = Mathf.Clamp(chargeConst + chargeDir * ((Time.realtimeSinceStartup - startTimeCharge) / maxChargeTime), 0, 1);
						ChargedInfo cInfo = new ChargedInfo(lastPos, chargedValue, index, true);
						IT_Gesture.ChargeEnd(cInfo);
					}
					else
					{
						float chargedValue = Mathf.Clamp(chargeConst + chargeDir * ((Time.realtimeSinceStartup - startTimeCharge) / maxChargeTime), 0, 1);
						ChargedInfo cInfo = new ChargedInfo(curPos, chargedValue, index, true);

						if (chargeMode == _ChargeMode.PingPong)
						{
							if (chargedValue == 1 || chargedValue == 0)
							{
								chargeDir *= -1;
								if (chargeDir == 1)
									chargeConst = 0;
								else if (chargeDir == -1)
									chargeConst = 1;

								startTimeCharge = Time.realtimeSinceStartup;
							}
							IT_Gesture.Charging(cInfo);
						}
						else
						{
							if (chargedValue < 1.0f)
							{
								IT_Gesture.Charging(cInfo);
							}
							else
							{
								cInfo.percent = 1.0f;

								if (chargeMode == _ChargeMode.Once)
								{
									chargeState = _ChargeState.Charged;
									IT_Gesture.ChargeEnd(cInfo);
									startTimeCharge = Mathf.Infinity;
									chargedValue = 0;
								}
								else if (chargeMode == _ChargeMode.Clamp)
								{
									chargeState = _ChargeState.Charged;
									IT_Gesture.Charging(cInfo);
								}
								else if (chargeMode == _ChargeMode.Loop)
								{
									chargeState = _ChargeState.Clear;
									IT_Gesture.ChargeEnd(cInfo);
									startTimeCharge = Time.realtimeSinceStartup;
								}
							}

						}
					}
				}

				if (!longTap && Time.realtimeSinceStartup - startTime > longTapTime && Vector2.Distance(lastPos, startPos) < maxTapDisplacementAllowance * IT_Gesture.GetDPIFactor())
				{
					IT_Gesture.LongTap(new Tap(curPos, 1, index, true));
					longTap = true;
				}

				lastPos = curPos;

				yield return null;
			}

			//check for shortTap
			if (Time.realtimeSinceStartup - startTime <= shortTapTime && Vector2.Distance(lastPos, startPos) < maxTapDisplacementAllowance * IT_Gesture.GetDPIFactor())
			{
				//IT_Gesture.ShortTap(startPos);
				CheckMultiTapMouse(index, startPos, lastPos);
			}

			//check for charge
			if (chargeState == _ChargeState.Charging || (chargeState == _ChargeState.Charged && chargeMode != _ChargeMode.Once))
			{
				float chargedValue = Mathf.Clamp(chargeConst + chargeDir * ((Time.realtimeSinceStartup - startTimeCharge) / maxChargeTime), 0, 1);
				ChargedInfo cInfo = new ChargedInfo(lastPos, chargedValue, index, true);
				IT_Gesture.ChargeEnd(cInfo);
			}
		}

		protected override void CheckMultiTapMouse(int index, Vector2 startPos, Vector2 lastPos)
		{
			if (multiTapMouse[index].lastTapTime > Time.realtimeSinceStartup - multiTapInterval)
			{
				if (Vector2.Distance(startPos, multiTapMouse[index].lastPos) < multiTapPosSpacing * IT_Gesture.GetDPIFactor())
				{
					multiTapMouse[index].count += 1;
					multiTapMouse[index].lastPos = startPos;
					multiTapMouse[index].lastTapTime = Time.realtimeSinceStartup;

					IT_Gesture.MultiTap(new Tap(startPos, lastPos, multiTapMouse[index].count, index, true));

					if (multiTapMouse[index].count >= maxMultiTapCount)
					{
						multiTapMouse[index].count = 0;
					}
				}
				else
				{
					multiTapMouse[index].count = 1;
					multiTapMouse[index].lastPos = startPos;
					multiTapMouse[index].lastTapTime = Time.realtimeSinceStartup;

					IT_Gesture.MultiTap(new Tap(startPos, lastPos, 1, index, true));
				}
			}
			else
			{
				multiTapMouse[index].count = 1;
				multiTapMouse[index].lastPos = startPos;
				multiTapMouse[index].lastTapTime = Time.realtimeSinceStartup;

				IT_Gesture.MultiTap(new Tap(startPos, lastPos, 1, index, true));
			}
		}

		protected override void CheckMultiTapTouch(int index, Vector2 startPos, Vector2 lastPos)
		{
			// Debug.Log("CheckMultiTapTouch(), index : " + index + ", multiTapTouch.Length : " + multiTapTouch.Length);
			if (index >= multiTapTouch.Length)
				return;

			if (multiTapTouch[index].lastTapTime > Time.realtimeSinceStartup - multiTapInterval)
			{
				if (Vector2.Distance(startPos, multiTapTouch[index].lastPos) < multiTapPosSpacing * IT_Gesture.GetDPIFactor())
				{
					multiTapTouch[index].count += 1;
					multiTapTouch[index].lastPos = startPos;
					multiTapTouch[index].lastTapTime = Time.realtimeSinceStartup;

					IT_Gesture.MultiTap(new Tap(startPos, lastPos, multiTapTouch[index].count, index, false));

					if (multiTapTouch[index].count >= maxMultiTapCount)
						multiTapTouch[index].count = 0;
				}
				else
				{
					multiTapTouch[index].count = 1;
					multiTapTouch[index].lastPos = startPos;
					multiTapTouch[index].lastTapTime = Time.realtimeSinceStartup;

					IT_Gesture.MultiTap(new Tap(startPos, lastPos, 1, index, false));
				}
			}
			else
			{
				multiTapTouch[index].count = 1;
				multiTapTouch[index].lastPos = startPos;
				multiTapTouch[index].lastTapTime = Time.realtimeSinceStartup;

				IT_Gesture.MultiTap(new Tap(startPos, lastPos, 1, index, false));
			}
		}

	}
}