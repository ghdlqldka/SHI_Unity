using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[ExecuteInEditMode]
public class ScrewAnim : MonoBehaviour
{
	public ScrewDriverRobot     Robot;
	public bool                 IsActive=false;

	
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
		if (Robot==null || !IsActive)
			return;
		
		if (!InAnimation)
		{
			GenerateAnimation();
			Timer=0;
			InAnimation=true;
		}
		
		if (InAnimation)
		{
			Animation();
			Robot.FingerPosition=CurrentPosition;
			Robot.HandAngle=CurrentAngle;
		}
	}
	
	
	
	void GenerateAnimation()
	{
		TargetAngle=Random.value*360;
		TargetPosition=Random.value*10;
		TurnTime=(Random.value+1)*1;
		MoveTime=(Random.value+1)*1;
		Pause=(Random.value)*1;
	}
	
	
	
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
