using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// Two axis robot animator. Hangs over TwoAxisRobotController,
// animates a robot through public parameters
[ExecuteInEditMode]
public class TwoAxisRobotAnimator : MonoBehaviour
{
	public TwoAxisRobotController   RobotController;
	public bool                     IsActive;
	
	
	float   SourceAngle;
	float   TargetAngle;
	float   CurrentAngle;
	float   SourcePosition;
	float   TargetPosition;
	float   CurrentPosition;
	float   TurnTime;
	float   MoveTime;
	float   Pause;
	float   Timer;
	bool    InAnimation;



	void Start()
	{
		SourceAngle=0;
		SourcePosition=0;
	}



	void Update()
	{
		// Checks that everything okay
		if (RobotController==null || !IsActive)
			return;

		// If animation has ended - generates new
		if (!InAnimation)
		{
			GenerateAnimation();
			Timer=0;
			InAnimation=true;
		}

		// If animation is active - running it
		if (InAnimation)
		{
			Animation();
			RobotController.Fingers=CurrentPosition;
			RobotController.TurnAngle=CurrentAngle;
		}
	}



	// Generates animation parameters
	void GenerateAnimation()
	{
		TargetAngle=Random.value*360;
		TargetPosition=Random.value/10;
		TurnTime=(Random.value+1)*1;
		MoveTime=(Random.value+1)*1;
		Pause=(Random.value)*1;
	}
	
	
	
	// Animates a few parameters using linear interpolation method
	void Animation()
	{
		float   TurnArg;
		float   MoveArg;
		
		Timer+=Time.deltaTime;
		TurnArg=Mathf.Clamp01(Timer/TurnTime);
		MoveArg=Mathf.Clamp01(Timer/MoveTime);
		
		CurrentAngle=SourceAngle+(TargetAngle-SourceAngle)*TurnArg;
		CurrentPosition=SourcePosition+(TargetPosition-SourcePosition)*MoveArg;
		
		if (TurnArg==1 && MoveArg==1)
		{
			Pause-=Time.deltaTime;
			if (Pause<0)
			{
				SourceAngle=TargetAngle;
				SourcePosition=TargetPosition;
				Timer=0;
				InAnimation=false;
			}
		}
	}
}
