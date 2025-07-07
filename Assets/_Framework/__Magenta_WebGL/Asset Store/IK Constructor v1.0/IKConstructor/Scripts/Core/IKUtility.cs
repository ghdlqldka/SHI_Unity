using UnityEngine;
using System.Collections;



// Miscellaneous underlying functions, used by the rest of the IKConstructor system
public static class IKUtility
{
	// Calculates a moving ramp, interpolates a [Position] towards [Target] using speed [MaxSpeed],
	// with acceleration, defined as A=[MaxSpeed]/[AccTime]
	// Parameters:
	//  Position - just any value needs to be animated
	//  Target - position at the end of animation
	//  MaxSpeed - maximum velocity to move
	//  AccTime - time at the beginning and end of animation, during which acceleration will be used
	//  CurrentSpeed - Ramp will interpolate a movement velocity itself
	//  dTime - Time delta of a current frame
	//  Return value: new position value
	public static float IKRamp(float Position,float Target,float MaxSpeed,float AccTime,ref float CurrentSpeed,float dTime)
	{
		float   Sign=(Position>Target)?-1:1;    // Sign of path to go
		float   S,V,A;                          // Path, Speed and Acceleration
		float   t1,t2;                          // Time to move with acceleration and constant speed
		
		// If position matched the target or velocity is too low - just staying where we are
		if (Position==Target || Mathf.Abs(MaxSpeed)<0.001f)
			return Position;

		// If current speed is opposite to the target's direction - zeroing current speed.
		// We're doing this because if target is not stationary there is a situation when
		// we can't apply deceleration time (if target moves towards current position itself)
		if (Sign*CurrentSpeed<0)
			CurrentSpeed=0;

		// Making sure that Speed, Acceleration, and Path ("S" variable)
		// are all positiove
		V=Mathf.Abs(CurrentSpeed);
		A=Mathf.Abs(MaxSpeed/AccTime);
		S=Mathf.Abs(Target-Position);
		
		
		t1=IKRampMakeTime(S,V,A*dTime,A);   // How long we need to move with acceleration
		t2=IKRampMakeTime(S,V,0,A);         // How long for constant speed
		
		// If we need aceleration - move like that
		if (t1>0)
		{
			IKRampIntegrator(ref Position,ref CurrentSpeed,Target,MaxSpeed,A*Sign,dTime);
			return Position;
		}
		
		// If there is still time left - move with constant speed
		if (t2>0)
		{
			IKRampIntegrator(ref Position,ref CurrentSpeed,Target,MaxSpeed,0,dTime);
			return Position;
		}
		
		// Moving within brakes turned on
		IKRampIntegrator(ref Position,ref CurrentSpeed,Target,MaxSpeed,-A*Sign,dTime);
		return Position;
	}
	
	
	
	// Returns time to move path [S] with start speed [V], stop speed [V]+[dV],
	// and acceleration [A]. Used by IKRamp function
	static float IKRampMakeTime(float S,float V,float dV,float A)
	{
		float   Result;
		
		if (A==0 || V+dV<0)
			return -1;

		Result=S-V*(dV*dV+2*V*dV+(V+dV)*(V+dV))/(2*A*(V+dV));
		return Result;
	}
	
	
	
	// Integrates trajectory, used by IKRamp function
	static void IKRampIntegrator(ref float Position,ref float Speed,float Target,float MaxSpeed,float Acc,float dTime)
	{
		float   Sign=(Position>Target)?-1:1;
		
		// Integrating speed
		Speed+=Acc*dTime;
		if (Mathf.Abs(Speed)>Mathf.Abs(MaxSpeed))
			Speed=Mathf.Abs(MaxSpeed)*Sign;
		
		// And position
		Position+=Speed*dTime;
		if (Mathf.Abs(Position-Target)<=Mathf.Abs(Speed*dTime))
		{
			Position=Target;
			Speed=0;
		}
	}
}
