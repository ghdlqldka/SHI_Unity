using UnityEngine;
using System.Collections;



[ExecuteInEditMode]
public class IKMover : IKBase
{
	public enum KeeperType
	{
		Volumetric,
		PlaneXZ,
		PlaneYZ
	};
	
	
	[Header("Control")]
	[Tooltip("Scalar position to match the target, measured or set relative to default local position")]
	public float            TargetPosition=0;
	[Tooltip("TargetPosition, altered by conversion and limiting stages, ReadOnly")]
	public float            AlteredPosition=0;
	[Tooltip("Current value of altered position during animation process, ReadOnly")]
	public float            CurrentPosition=0;
	[Tooltip("Indicates that component movement is finished, ReadOnly")]
	public bool             IsFinished=false;
	[Tooltip("Indicates that target successfully reached, ReadOnly")]
	public bool             IsTargetReached=false;

	[Header("Configuration")]
	[Tooltip("Mode of operation. Inverse follows the target, Forward moves where user wants")]
	public KinematicType    Kinematic;
	[Tooltip("Drag a link to another IKMover script if synchronization is needed. Works in Forward mode only")]
	public IKMover          MasterMover;
	[Tooltip("What source to use to override TargetPosition field if FollowMover is set to another IKMover script")]
	public MasterSource     MasterField;
	
	[Space(10)]
	[Tooltip("GameObject's transform for this script to drive")]
	public Transform        Origin;
	[Tooltip("Target to reach, Origin node will move along its blue axis towards this target")]
	public Transform        Target;
	
	[Space(10)]
	[Tooltip("How distance to target is measured if it needs to be kept")]
	public KeeperType       KeeperMode;
	[Tooltip("Don't come to the target closer than this distance")]
	public float            KeepDistance=0.0f;
	
	[Space(10)]
	[Tooltip("Converts position by following formula: AlteredPosition=(TargetPosition+PreOffset)*Gain+PostOffset")]
	public float            PreOffset=0.0f;
	[Tooltip("Caoverts position by following formula: AlteredPosition=(TargetPosition+PreOffset)*Gain+PostOffset")]
	public float            Gain=1.0f;
	[Tooltip("Converts position by following formula: AlteredPosition=(TargetPosition+PreOffset)*Gain+PostOffset")]
	public float            PostOffset=0.0f;
	
	[Space(10)]
	[Tooltip("Position limiter relative to the start position of the Origin")]
	public bool             Limits=false;
	[Tooltip("Minimum position, can't be greater than maximum")]
	public float            Minimum=0;
	[Tooltip("Maximum position, can't be lesser than minimum ")]
	public float            Maximum=0;

	[Space(10)]
	[Tooltip("Constant or maximum speed, depends on movement type, set to zero for instant operation")]
	public float            Speed;
	[Tooltip("Acceleration = Speed / AccelerationTime, set to zero if constant speed mode required")]
	public float            AccelerationTime;

	[HideInInspector]	public bool     DrawVisual=true;
	[HideInInspector]	public bool     VisualSettings=false;
	[HideInInspector]	public float    VisualSize=1;
	[HideInInspector]	public Color    VisualLineColor=Color.cyan;
	[HideInInspector]	public Color    VisualMarkerColor=Color.red;
	
	
	Matrix4x4           WorldMatrix;
	protected float               CurrentSpeed;
	[HideInInspector] [SerializeField]
	protected Vector3             StartPosition;
	[HideInInspector] [SerializeField]
	protected bool                IsInit=false;
	
	
	
	protected virtual void Start()
	{
		if (!Application.isPlaying)
			return;

		if (!IsInit)
			Init();
	}
	
	
	
	protected virtual void LateUpdate()
	{
		// Update the object automatically if set
		if (AutoUpdate)
			ManualUpdate();
		
		// Draws a debug visual if set
		if (DrawVisual)
			DrawDebug();
	}
	
	

	// Primary initializer used automatically by the system
	public override bool Init()
	{
		if (Origin==null)
		{
			IsInit=false;
			return false;
		}
		
		StartPosition=Origin.localPosition;
		IsInit=true;
		
		return true;
	}
	
	
	
	// Alternative initializer, copies default position from Transform specified,
	// Additionally, if SetAsOrigin is true, sets this transform as origin.
	public bool Init(Transform Transform,bool SetAsOrigin)
	{
		if (Transform==null)
		{
			IsInit=false;
			return false;
		}
		
		if (SetAsOrigin)
			Origin=Transform;
		else 
			if (Origin==null)
				return false;
		
		StartPosition=Transform.localPosition;
		IsInit=true;
		
		return true;
	}
	
	
	
	// Alternative initializer, allows to set default orientation specified by a quaternion.
	// returns true if Origin is set, false otherwise (init has failed)
	public bool Init(Vector3 Position)
	{
		if (Origin==null)
		{
			IsInit=false;
			return false;
		}
		
		StartPosition=Position;
		IsInit=true;
		
		return true;
	}

	
	// Initialise by setting both Origin transform and local position
	// Transform reference must be valid
	public bool Init(Transform Transform,Vector3 Position)
	{
		if (Transform==null)
			return false;

		Origin=Transform;
		StartPosition=Position;
		IsInit=true;

		return true;
	}
	
	
	
	// Update function, called automatically if AutoUpdate is set.
	// Call this from your custom IK script to control update sequence.
	// Dont forget to set AutoUpdate to false in that case, to avoid
	// double computations
	public override void ManualUpdate()
	{
		float   Swap;
		bool    IsLimited;
		
		if (Origin==null)
		{
			IsInit=false;
			return;
		}
		
		if (!IsInit)
		{
			if (!Init())
				return;
		}
		
		if (Minimum>Maximum)
		{
			Swap=Minimum;
			Minimum=Maximum;
			Maximum=Swap;
		}
		
		Speed=Mathf.Clamp(Speed,0,float.PositiveInfinity);
		AccelerationTime=Mathf.Clamp(AccelerationTime,0,float.PositiveInfinity);
		
		// Determines what type of Kinematic in use
		if (Kinematic==KinematicType.Inverse)
		{
			// We're in Inverse mode
			// Doing position measurement
			if (!SolveIKPosition(Origin,Target,ref TargetPosition))
				return;
		}
		else
		{
			// We're in Forward mode
			// Checking if follow sub-mode is active, if so
			// overriding user setting by one of the fields
			// from another IK-script supplied, otherwise letting
			// user to control position directly
			if (MasterMover!=null)
			{
				switch (MasterField)
				{
					case MasterSource.TargetValue:	TargetPosition=MasterMover.TargetPosition;	break;
					case MasterSource.AlteredValue:	TargetPosition=MasterMover.AlteredPosition; break;
					case MasterSource.CurrentValue:	TargetPosition=MasterMover.CurrentPosition; break;
				}
			}
		}

		// Convertion stage
		AlteredPosition=ConvertPosition(TargetPosition);

		// Limit stage, can be inactive
		if (Limits)
			IsLimited=LimitPosition(Minimum,Maximum,ref AlteredPosition);
		else
			IsLimited=false;
		
		// Animation stage
		AnimatePosition(ref CurrentPosition,AlteredPosition,IsLimited);
		
		// Setting position
		SetPosition(CurrentPosition);
	}
	
	
	
	// Measures position to target
	protected bool SolveIKPosition(Transform Origin,Transform Target,ref float TargetPosition)
	{
		Vector3     LocalTargetPos;

		// Checking if measurement is posiible
		if (!IsInit || Origin==null || Target==null || Origin==Target)
			return false;

		// Generating a matrix, using default position, current rotation, and a unit scale
		WorldMatrix.SetTRS(StartPosition,Origin.localRotation,Vector3.one);

		// If this node is a child of someone, multiply our martix by a matrix
		// of our parent to get a full Object-to-world matrix for our node
		if (Origin.parent!=null)
			WorldMatrix=Origin.parent.localToWorldMatrix*WorldMatrix;

		// Projecting world position of a target into local coordinate system
		LocalTargetPos=WorldMatrix.inverse.MultiplyPoint3x4(Target.position);

		// Taking target position from projection
		TargetPosition=LocalTargetPos.z;

		// If we need to keep a specified distance to target - call a keeper
		if (KeepDistance!=0.0f)
			TargetPosition=DistanceKeeper(LocalTargetPos,TargetPosition,KeepDistance,KeeperMode);

		return true;
	}
	
	
	
	// Makes sure that target not closer to the object than [DistanceToKeep] units.
	// If it closer - alters a position so that a distance to the object will be exact
	// [DistanceToKeep] units. 
	
	// Arguments:
	// 1) LocalTargetPosition - current 3D position of a target in local object space
	// 2) Position - scalar position as 3D distance from default to a current local position
	// 3) DistanceToKeep - scalar distance that object should keep away from the target
	// 4) Mode - defines mode of current distance measurement:
	//      4.1) PlanarX - X coorditane doesnt affects a distance
	//      4.2) PlanarY - Y coorditane doesnt affects a distance
	//      4.3) Volumetric - no projections are made, just true 3D distance between object and a target
	// Return value: new scalar position value
	float DistanceKeeper(Vector3 LocalTargetPosition,float Position,float DistanceToKeep,KeeperType Mode)
	{
		float   DistanceSqr;
		float   KeepSqr;
		
		switch (Mode)
		{
			case KeeperType.PlaneYZ:
				LocalTargetPosition.x=0;
				break;
				
			case KeeperType.PlaneXZ:
				LocalTargetPosition.y=0;
				break;
		}
		
		LocalTargetPosition.z=0;
		DistanceSqr=LocalTargetPosition.sqrMagnitude;
		KeepSqr=DistanceToKeep*DistanceToKeep;
		if (DistanceSqr<KeepSqr)
			return Position-Mathf.Sqrt(KeepSqr-DistanceSqr);
		else
			return Position;
	}
	
	
	
	// Converter stage inmplementation
	protected float ConvertPosition(float Position)
	{
		return (Position+PreOffset)*Gain+PostOffset;
	}
	
	
	
	// Limiter stage implementation, returns true, if position was limited,
	// and false otherwise
	protected bool LimitPosition(float Minimum,float Maximum,ref float Position)
	{
		float   Swap;
		bool    IsLimited=false;
		
		if (Minimum>Maximum)
		{
			Swap=Minimum;
			Minimum=Maximum;
			Maximum=Swap;
		}
		
		if (Position<Minimum)
		{
			Position=Minimum;
			IsLimited=true;
		}
		
		if (Position>Maximum)
		{
			Position=Maximum;
			IsLimited=true;
		}
		
		return IsLimited;
	}
	
	
	
	// Animation stage
	protected virtual void AnimatePosition(ref float DstPosition,float SrcPosition,bool IsLimited)
	{
		AnimationType   AnimType;
		float           MoveDelta;
		
		// Now evaluate type of movement
		AnimType=AnimationType.Instant;
		if (Speed>0)
		{
			AnimType=AnimationType.ConstSpeed;
			if (AccelerationTime>0)
				AnimType=AnimationType.Accelerated;
		}

		// Move, according to movement type
		switch (AnimType)
		{
			// Performing instant move
			case AnimationType.Instant:
				DstPosition=SrcPosition;
				SetFinishState(true,IsLimited);
				break;

			// Moving towards AlteredPosition with constant speed
			case AnimationType.ConstSpeed:
				MoveDelta=0;
				if (SrcPosition!=DstPosition)
					MoveDelta=Speed*Mathf.Sign(DstPosition-SrcPosition)*Time.deltaTime;

				if (Mathf.Abs(SrcPosition-DstPosition)<=MoveDelta || MoveDelta==0)
				{
					DstPosition=SrcPosition;
					SetFinishState(true,IsLimited);
				}
				else
				{
					DstPosition+=MoveDelta;
					SetFinishState(false,IsLimited);
				}
				break;

			// Moving to AlteredPosition with acceleration and start push brakes before we reach it
			case AnimationType.Accelerated:
				DstPosition=IKUtility.IKRamp(DstPosition,SrcPosition,Speed,AccelerationTime,ref CurrentSpeed,Time.deltaTime);

				if (DstPosition==SrcPosition)
					SetFinishState(true,IsLimited);
				else
					SetFinishState(false,IsLimited);
				break;
		}
	}
	
	

	// Just setting position of our object
	protected void SetPosition(float Position)
	{
		Origin.localPosition=StartPosition+Origin.localRotation*(Vector3.forward*Position);
	}
	
	
	
	// Setting flags of "animation ended" and "target is reached" to their states
	protected virtual void SetFinishState(bool State,bool IsLimited)
	{
		IsFinished=State;
		
		if (State)
			IsTargetReached=!IsLimited;
		else
			IsTargetReached=false;
	}
	
	
	
	// Drawing debug visual if needed
	protected virtual void DrawDebug()
	{
		Vector3     StartPos;
		Vector3     v1,v2;
		Vector3     v3,v4;
		
		if (Origin==null)
			return;
		
		if (Origin.parent!=null)
			StartPos=Origin.parent.transform.TransformPoint(StartPosition);
		else
			StartPos=StartPosition;
			
		// Drawing a marker of default position
		v1=StartPos-Origin.right*VisualSize/2;
		v2=StartPos+Origin.right*VisualSize/2;
		Debug.DrawLine(v1,v2,Color.white);

		// Drawing main axis
		v1=StartPos;
		v2=StartPos+Origin.forward*VisualSize/2;
		Debug.DrawLine(v1,v2,Color.white);
		
		v2=Origin.position;
		Debug.DrawLine(v1,v2,VisualLineColor);
		
		// Drawing a marker of current position
		v1=Origin.position-Origin.forward*VisualSize/2-Origin.right*VisualSize/2;
		v2=Origin.position+Origin.forward*VisualSize/2-Origin.right*VisualSize/2;
		v3=Origin.position-Origin.forward*VisualSize/2+Origin.right*VisualSize/2;
		v4=Origin.position+Origin.forward*VisualSize/2+Origin.right*VisualSize/2;
		
		Debug.DrawLine(v1,v2,VisualMarkerColor);
		Debug.DrawLine(v2,v4,VisualMarkerColor);
		Debug.DrawLine(v4,v3,VisualMarkerColor);
		Debug.DrawLine(v3,v1,VisualMarkerColor);
		Debug.DrawLine(v1,v4,VisualMarkerColor);
		Debug.DrawLine(v2,v3,VisualMarkerColor);
	}
}
