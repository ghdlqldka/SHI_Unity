using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// Controls and animates gearbox
[ExecuteInEditMode]
public class GearBox : MonoBehaviour
{
	[Header("Controls")]
	public bool     TakeControl=false;  // If true animates a gearbox, if false turns to inverse kinematic mode
	
	[Space(10)]
	public float    TargetSpeed;
	public float    CurrentSpeed;
	public float    Acceleration;

	[Space(10)]
	[Header("Components")]
	public IKAxis   MainGear;
	public IKAxis   SmallGear;
	public IKAxis   TopGear;
	public IKAxis   SideGear1;
	public IKAxis   SideGear2;
	public IKMover  Piston1;
	public IKMover  Piston2;
	public IKAxis   Rod1;
	public IKAxis   Rod2;
	
	
	
	void LateUpdate()
	{
		float   dSpeed;
		
		if (!CheckInit())
			return;
		
		// Mode of operation
		if (TakeControl)
		{
			MainGear.Kinematic=IKBase.KinematicType.Forward;
			SetAutoUpdate(false);
		}
		else
		{
			MainGear.Kinematic=IKBase.KinematicType.Inverse;
			SetAutoUpdate(true);
			return;
		}
		
		// Interpolate animation speed
		if (TargetSpeed!=CurrentSpeed)
		{
			dSpeed=Mathf.Abs(Acceleration)*Mathf.Sign(TargetSpeed-CurrentSpeed)*Time.deltaTime;
			if (Mathf.Abs(dSpeed)>Mathf.Abs(TargetSpeed-CurrentSpeed))
				CurrentSpeed=TargetSpeed;
			else
				CurrentSpeed+=dSpeed;
		}
		
		// Interpolate angle
		MainGear.TargetAngle+=CurrentSpeed*Time.deltaTime;
		
		// Update kinematic chain in proper execution order
		MainGear.ManualUpdate();
		SmallGear.ManualUpdate();
		TopGear.ManualUpdate();
		SideGear1.ManualUpdate();
		SideGear2.ManualUpdate();
		Piston1.ManualUpdate();
		Piston2.ManualUpdate();
		Rod1.ManualUpdate();
		Rod2.ManualUpdate();
	}
	
	
	
	bool CheckInit()
	{
		if (MainGear==null || SmallGear==null || TopGear==null || SideGear1==null ||
		    SideGear2==null || Piston1==null || Piston2==null || Rod1==null || Rod2==null)
			return false;
		
		return true;
	}
	
	
	
	// Controls autoupdate of the whole chain
	void SetAutoUpdate(bool State)
	{
		MainGear.AutoUpdate=State;
		SmallGear.AutoUpdate=State;
		TopGear.AutoUpdate=State;
		SideGear1.AutoUpdate=State;
		SideGear2.AutoUpdate=State;
		Piston1.AutoUpdate=State;
		Piston2.AutoUpdate=State;
		Rod1.AutoUpdate=State;
		Rod2.AutoUpdate=State;
	}
}
