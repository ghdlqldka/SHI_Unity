using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[ExecuteInEditMode]
public class ScrewDriverRobot : MonoBehaviour
{
	[Header("Controls")]
	public float        PlatformEvasion;
	public float        HandAngle;
	public float        FingerPosition;
	
	[Space(10)]
	[Header("Components")]
	public IKMover      RobotMover;
	public IKAxis       RobotAxis;
	public IKArm        RobotArm;
	public IKAxis       HandAxisX;
	public IKAxis       HandAxisY;
	public IKAxis       HandAxisZ;
	public IKMover      HandFinger1;
	public IKMover      HandFinger2;
	public IKMover      HandFinger3;
	
	
	
	void LateUpdate()
	{
		if (!CheckInit())
			return;
		
		FingerPosition=Mathf.Clamp(FingerPosition,0,100);
		RobotMover.KeepDistance=PlatformEvasion;
		HandAxisZ.TargetAngle=HandAngle;
		HandFinger1.TargetPosition=FingerPosition/100;
		
		UpdateIK();
	}
	
	
	
	bool CheckInit()
	{
		if (RobotMover==null || RobotAxis==null || RobotArm==null || HandAxisX==null || HandAxisY==null ||
			HandAxisZ==null || HandFinger1==null || HandFinger2==null || HandFinger3==null)
			return false;
		
		return true;
	}
	
	
	
	void UpdateIK()
	{
		RobotMover.ManualUpdate();
		RobotAxis.ManualUpdate();
		RobotArm.ManualUpdate();
		HandAxisX.ManualUpdate();
		HandAxisY.ManualUpdate();
		HandAxisZ.ManualUpdate();
		HandFinger1.ManualUpdate();
		HandFinger2.ManualUpdate();
		HandFinger3.ManualUpdate();
	}
}
