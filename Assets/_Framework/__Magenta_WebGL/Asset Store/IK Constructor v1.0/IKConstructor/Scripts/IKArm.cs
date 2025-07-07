using UnityEngine;
using System.Collections;



[ExecuteInEditMode]
public class IKArm : IKBase
{
	[Header("Control")]
	[Tooltip("Angle in degrees around X axis between Origin's current rotation and the default one")]
	public float            TargetOriginAngle;
	[Tooltip("Angle in degrees around X axis between Elbow's current rotation and the default one")]
	public float            TargetElbowAngle;
	[Tooltip("True if endpoint of an Arm reached target's projection on a local YZ plane of the Origin, false otherwise")]
	public bool             IsTargetReached;
	
	[Space(10)]
	[Tooltip("Distance between Origin and Elbow objects, Read/Write")]
	public float            OriginLength=1;
	[Tooltip("Distance between Elbow and Endpoint object, Read/Write")]
	public float            ElbowLength=1;
	
	[Header("Configuration")]
	[Tooltip("Mode of operaiton, Inverse to follow the target, Forward for manual control")]
	public KinematicType    Kinematic;
	
	[Space(10)]
	[Tooltip("The base of the Arm")]
	public Transform        Origin;
	[Tooltip("Point at which Arm will bend")]
	public Transform        Elbow;
	[Tooltip("Arm tries to touch the target with this object")]
	public Transform        Endpoint;
	[Tooltip("Object, which Arm always tries to reach")]
	public Transform        Target;
	
	
	
	[HideInInspector]   public bool     DrawVisual=true;
	[HideInInspector]	public float    VisualSize=0.25f;
	[HideInInspector]	public bool     VisualBaseMarker=true;
	[HideInInspector]	public bool     VisualElbowMarker=true;
	[HideInInspector]	public Color    VisualLineColor=Color.cyan;
	[HideInInspector]	public Color    VisualStartOrientationColor=Color.white;
	
	
	Matrix4x4               WorldMatrix;
	[HideInInspector] [SerializeField]
	Quaternion              StartRotation=Quaternion.identity;
	[HideInInspector] [SerializeField]
	protected bool                    IsInit=false;
	
	
	
	protected virtual void Start()
	{
		if (!Application.isPlaying)
			return;
		
		if (!IsInit)
			Init();
	}
	
	
	
	protected virtual void LateUpdate()
	{
		// Updates object automatically if set
		if (AutoUpdate)
			ManualUpdate();
		
		// Draws a debug visual if set
		if (DrawVisual)
			DrawDebug();
	}
	
	
	
	// Primary init function, used automatically by the system
	public override bool Init()
	{
		if (Origin==null || Elbow==null || Endpoint==null)
		{
			IsInit=false;
			return false;
		}
		
		StartRotation=Origin.localRotation;
		IsInit=true;
		
		return true;
	}
	
	
	
	// Primary update function. Called automatically if AutoUpdate is set.
	// You can call this from your custom scripts, but dont forget to set
	// AutoUpdate field to false, to avoid excessive computations
	public override void ManualUpdate()
	{
		bool    IsDistanceLimit=false;
		
		// Tries to initialise on the fly
		if (!IsInit)
		{
			Init();
			if (!IsInit)
				return;
		}
		
		// Makes sure that bones length is positive
		OriginLength=Mathf.Abs(OriginLength);
		ElbowLength=Mathf.Abs(ElbowLength);
		
		// In Inverse mode measures angles, otherwise letting user to set them manually
		if (Kinematic==KinematicType.Inverse)
		{
			if (!SolveIK(ref TargetOriginAngle,ref TargetElbowAngle,ref IsDistanceLimit))
				return;
		}
		
		// Animation stage, current version of animator support only instant mode
		Animate(IsDistanceLimit);
		
		// Setting all arm components in place
		SetArm(TargetOriginAngle,TargetElbowAngle);
	}
	
	
	
	// Does measurements of angles, sets IsDistanceLimit flag true if distance form Arm's base to
	// the local target projection overcomes sum of bones lengths, or if its smaller that their difference,
	// false otherwise.
	// returns false if measurement not possible
	bool SolveIK(ref float BaseAngle,ref float ElbowAngle,ref bool IsDistanceLimit)
	{
		Vector3     LocalTargetPos;
		Vector3     LocalElbowPos;
		Vector3     TargetNormal;
		Vector3     TargetTangent;
		float       Distance;
		float       SinA;
		float       CosA;
		
		// Checks if measurement is posiible
		if (!IsInit || Origin==null || Elbow==null || Endpoint==null || Target==null)
			return false;
		
		// Calculate local-to-parent matrix
		WorldMatrix.SetTRS(Origin.localPosition,StartRotation,Vector3.one);
		
		// If there is a parent - multiply by its matrix in order to get
		// full local-to-world matrix
		if (Origin.parent!=null)
			WorldMatrix=Origin.parent.localToWorldMatrix*WorldMatrix;
		
		// Now project the target into local YZ plane, in which arm is functioning
		LocalTargetPos=WorldMatrix.inverse.MultiplyPoint3x4(Target.position);
		LocalTargetPos.x=0;
		
		// Making a distance to projected target
		Distance=Mathf.Sqrt(LocalTargetPos.y*LocalTargetPos.y+LocalTargetPos.z*LocalTargetPos.z);
		if (Distance<0.0001)
			return false;
		
		// Direction to the target
		TargetTangent=LocalTargetPos/Distance;

		// Now consider distance to the target relative to the Arm's base.
		// Option 1: Distance is lesser than sum of bones lengths, and is greater than 
		// module of ther difference. In this case arm can reach the target's projection point
		if (Distance<OriginLength+ElbowLength && Distance>Mathf.Abs(OriginLength-ElbowLength))
		{
			// Найдем синус и косинус угла между вектором Forward и первой костью
			
			// Lets find sine and cosine between forward vector and first bone
			CosA=(OriginLength*OriginLength+Distance*Distance-ElbowLength*ElbowLength)/(2*OriginLength*Distance);
			SinA=Mathf.Sqrt(1-CosA*CosA);
			
			// Make normal to that, by turning +90 degrees around local X axis from our tangent
			TargetNormal.x=0.0f;
			TargetNormal.y=TargetTangent.z;
			TargetNormal.z=-TargetTangent.y;
			
			// Lets find local Elbow object position
			LocalElbowPos=TargetTangent*OriginLength*CosA+TargetNormal*OriginLength*SinA;
			
			// Mark that there in no distance limit, so target can be reached
			IsDistanceLimit=false;
		}
		// Option 2: Distance to target's projection is greater that full arm's length
		// Option 3: Distance to target's projection is lesser that module of difference between bones
		else
		{
			// In both cases target cannot be reached, so we just align Elbow object to the tangent
			LocalElbowPos=TargetTangent*OriginLength;
			
			// Marking that target cannot be reached
			IsDistanceLimit=true;
		}

		// Now lets find angle in degrees from base to elbow.
		TargetTangent=LocalElbowPos.normalized;
		SinA=TargetTangent.y;
		CosA=TargetTangent.z;
		BaseAngle=SinCosToAngle(SinA,CosA);
		if (BaseAngle>180)		BaseAngle-=360;
		if (BaseAngle<-180)		BaseAngle+=360;
		
		// Now find direction from elbow to target and measure an angle in degrees
		TargetTangent=(LocalTargetPos-LocalElbowPos).normalized;
		SinA=TargetTangent.y;
		CosA=TargetTangent.z;
		ElbowAngle=SinCosToAngle(SinA,CosA)-BaseAngle;
		if (ElbowAngle>180)		ElbowAngle-=360;
		if (ElbowAngle<-180)	ElbowAngle+=360;
		
		return true;
	}
	
	
	
	// Animation stage doesnt support animation so far, just works in instant mode only
	bool Animate(bool IsDistanceLimit)
	{
		// Setting both states
		SetFinishState(true,IsDistanceLimit);
		
		return true;
	}
	
	
	
	// Takes 2 angles in degrees and sets all arm's objects in current pose
	void SetArm(float BaseAngle,float ElbowAngle)
	{
		Quaternion  QBase;
		Quaternion  QElbow;
		
		QBase=Quaternion.AngleAxis(-BaseAngle,Vector3.right);
		QElbow=Quaternion.AngleAxis(-ElbowAngle,Vector3.right);
		
		Elbow.localPosition=Vector3.forward*OriginLength;
		Endpoint.localPosition=Vector3.forward*ElbowLength;
		
		Origin.localRotation=StartRotation*QBase;
		Elbow.localRotation=QElbow;
	}
	
	
	
	// Takes sine and cosine of an angle and returns it in degrees
	float SinCosToAngle(float SinA,float CosA)
	{
		float   Angle=Mathf.Acos(CosA)*Mathf.Rad2Deg;
		
		if (SinA<0.0f)
			Angle=360-Angle;
		
		return Angle;
	}
	
	
	
	// Setting flags of animation-ended and target-is-reached
	void SetFinishState(bool State,bool IsDistanceLimit)
	{
		if (State)
			IsTargetReached=!IsDistanceLimit;
		else
			IsTargetReached=false;
	}
	
	

	// Draws a debug-purpose visual
	protected virtual void DrawDebug()
	{
		Quaternion  QStart;
		Vector3     v1,v2;
		
		// Checking if everything is okay
		if (!IsInit || Origin==null || Elbow==null || Endpoint==null)
			return;

		if (Origin.parent!=null)
			QStart=Origin.parent.rotation*StartRotation;
		else
			QStart=StartRotation;
		
		// Draws a first bone
		v1=Origin.position;
		v2=Elbow.position;
		Debug.DrawLine(v1,v2,VisualLineColor);
		
		// Draws a second bone
		v1=Elbow.position;
		v2=Endpoint.position;
		Debug.DrawLine(v1,v2,VisualLineColor);
		
		// Draws a base default orientation marker if set
		if (VisualBaseMarker)
		{
			v1=Origin.position;
			v2=v1+QStart*Vector3.forward*VisualSize;
			Debug.DrawLine(v1,v2,VisualStartOrientationColor);
		}
		
		// Draws an elbow orientation marker if set
		if (VisualElbowMarker)
		{
			v1=Elbow.position;
			v2=v1+Origin.forward*VisualSize;
			Debug.DrawLine(v1,v2,VisualStartOrientationColor);
		}
	}
}
