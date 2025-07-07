using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[ExecuteInEditMode]
public class IKTextControl : MonoBehaviour
{
	[Header("Controls")]
	public float    Speed;
	public bool     AutoUpdate;
	public bool     DrawVisual;
	
	[Space(10)]
	[Header("Components")]
	public IKAxis   Axis;
	public IKArm    Arm;
	
	
	
	void Start()
	{
		if (Axis==null || Arm==null)
			return;
		
		Axis.Kinematic=IKBase.KinematicType.Forward;
		Axis.TargetAngle=0;
		
		Arm.Kinematic=IKBase.KinematicType.Inverse;
	}
	
	
	
	void Update()
	{
		if (Axis==null || Arm==null)
			return;

		Axis.AutoUpdate=AutoUpdate;
		Arm.AutoUpdate=AutoUpdate;

		Axis.DrawVisual=DrawVisual;
		Arm.DrawVisual=DrawVisual;
		
		Axis.TargetAngle+=Speed*Time.deltaTime;
		
		if (!AutoUpdate)
		{
			Axis.ManualUpdate();
			Arm.ManualUpdate();
		}
	}
}
