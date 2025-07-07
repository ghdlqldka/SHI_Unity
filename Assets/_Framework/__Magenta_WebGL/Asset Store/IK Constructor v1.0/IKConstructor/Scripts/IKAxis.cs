using UnityEngine;
using System.Collections;



[ExecuteInEditMode]
public class IKAxis : IKBase
{
	[Header("Control")]
	[Tooltip("Angle to the target. Measured or set relative to default local Y axis")]
	public float            TargetAngle=0;
	[Tooltip("TargetAngle passed conversion and limiting stage. Read only")]
	public float            AlteredAngle=0;
	[Tooltip("AlteredAngle passed animation stage")]
	public float            CurrentAngle=0;
	[Tooltip("Set to true when rotation is finished, false if in the middle of animation")]
	public bool             IsFinished=false;
	[Tooltip("Set to true when target is reached (CurrentAngle==AlteredAngle)")]
	public bool             IsTargetReached=false;
	
	[Header("Configuration")]
	[Tooltip("Mode of operation. Inverse turns towards target, Forward turns where user wants")]
	public KinematicType    Kinematic;
	[Tooltip("Drag a link to another IKAxis script if synchronization is needed. Works in Forward mode only")]
	public IKAxis           MasterAxis;
	[Tooltip("What source to use to override TargetAngle field if FollowMover is set to another IKAxis script")]
	public MasterSource     MasterField;
	
	[Space(10)]
	[Tooltip("GameObject's transform for this script to drive")]
	public Transform        Origin;
	[Tooltip("Target to reach, Origin node will turn around its green axis towards this target")]
	public Transform        Target;
	
	[Space(10)]
	[Tooltip("Axis turn as if the target is shifted along red axis by this number of units")]
	public float            SlideCompensation=0;
	
	[Space(10)]
	[Tooltip("Converts angle by formula: AlteredAngle=(TargetAngle+PreOffset)*Gain+PostOffset")]
	public float            PreOffset=0;
	[Tooltip("Converts angle by formula: AlteredAngle=(TargetAngle+PreOffset)*Gain+PostOffset")]
	public float            Gain=1;
	[Tooltip("Converts angle by formula: AlteredAngle=(TargetAngle+PreOffset)*Gain+PostOffset")]
	public float            PostOffset=0;
	
	[Space(10)]
	[Tooltip("Angle Limiter stage. Minimum angle is always to the right, maximum - to the left")]
	public bool             Limits=false;
	[Tooltip("Angle Limiter stage. Minimum angle is always to the right, maximum - to the left")]
	public float            Minimum=0;
	[Tooltip("Angle Limiter stage. Minimum angle is always to the right, maximum - to the left")]
	public float            Maximum=0;
	
	[Space(10)]
	[Tooltip("Constant or maximum speed, depends on animation type, set to zero for instant operation")]
	public float            Speed=0;
	[Tooltip("Time, to accelerate from 0 to specified Speed, set to zero if constant speed mode required")]
	public float            AccelerationTime=0;
	
	[HideInInspector]   public bool     DrawVisual=true;
	[HideInInspector]	public bool     VisualSettings=false;
	[HideInInspector]	public float    VisualRadius=1;
	[HideInInspector]	public int      VisualCircleSteps=16;
	[HideInInspector]	public Color    VisualCircleColor=Color.green;
	[HideInInspector]	public Color    VisualCompensatorColor=Color.yellow;
	
	Matrix4x4           StartMatrix;
	float               CurrentSpeed;
	
	[HideInInspector] [SerializeField]
	Quaternion          StartRotation=Quaternion.identity;
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
		// Updates Axis automatically is set
		if (AutoUpdate)
			ManualUpdate();
		
		// Draw a debug visual if set
		if (DrawVisual)
			DrawDebug();
	}
	
	
	
	// Primary initializer, used automatically by the system
	public override bool Init()
	{
		if (Origin==null)
		{
			IsInit=false;
			return false;
		}
		
		StartRotation=Origin.localRotation;
		IsInit=true;
		
		return true;
	}
	
	
	
	// Alternative initializer, copies default orientation from Transform specified,
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
		
		StartRotation=Transform.localRotation;
		IsInit=true;
		
		return true;
	}
	
	
	
	// Alternative initializer, allows to set default orientation specified by a quaternion.
	// returns true if Origin is set, false otherwise (init has failed)
	public bool Init(Quaternion Rotation)
	{
		if (Origin==null)
		{
			IsInit=false;
			return false;
		}
		
		StartRotation=Rotation;
		IsInit=true;
		
		return true;
	}
	
	
	
	// Initialise by setting both Origin transform and Rotation quaternion
	// Transform reference must be valid
	public bool Init(Transform Transform,Quaternion Rotation)
	{
		if (Transform==null)
			return false;
		
		Origin=Transform;
		StartRotation=Rotation;
		IsInit=true;
		
		return true;
	}
	
	
	
	// Update function, called automatically if AutoUpdate is set.
	// Call this from your custom IK script to control update sequence.
	// Dont forget to set AutoUpdate to false in that case, to avoid 
	// double computations
	public override void ManualUpdate()
	{
		bool IsLimited;
		
		if (Origin==null)
		{
			IsInit=false;
			return;
		}
		
		if (!IsInit)
			Init();
		
		// Determines what type of Kinematic in use
		if (Kinematic==KinematicType.Inverse)
		{
			// We're in Inverse mode
			// Doing an Angle-to-target measurement
			if (!SolveIKAngle(ref TargetAngle))
				return;
		}
		else
		{
			// We're in Forward mode
			// Checking if follow sub-mode is active, if so
			// overriding user setting by one of the fields
			// from another IK-script supplied, otherwise letting
			// user to control the angle
			if (MasterAxis!=null)
			{
				switch (MasterField)
				{
					case MasterSource.TargetValue:  TargetAngle=MasterAxis.TargetAngle;		break;
					case MasterSource.AlteredValue: TargetAngle=MasterAxis.AlteredAngle;	break;
					case MasterSource.CurrentValue: TargetAngle=MasterAxis.CurrentAngle;	break;
				}
			}
		}
		
		// Applying converter stage
		AlteredAngle=ConvertAngle(TargetAngle);

		// Applying limiter stage if active
		if (Limits)
			IsLimited=LimitAngle(ref AlteredAngle,Minimum,Maximum);
		else
			IsLimited=false;

		// Applying animation stage
		Animate(IsLimited);

		// Setting rotation
		SetRotation(CurrentAngle);
	}
	
	
	
	// Measuring angle to target
	bool SolveIKAngle(ref float TrgAngle)
	{
		Vector3     LocalTargetPos;
		float       Length;
		float       SinA;
		float       CosA;
		float       SlideAngle;
	
		// Checking if work is possible
		if (Kinematic!=KinematicType.Inverse || Target==null || Target==Origin)
			return false;
		
		// Generating a matrix, using current position, default rotation, and a unit scale
		StartMatrix.SetTRS(Origin.localPosition,StartRotation,Vector3.one);
		
		// If this node is a child of someone, multiply our martix by a matrix
		// of our parent to get a full Object-to-world matrix for our node
		if (Origin.parent!=null)
			StartMatrix=Origin.parent.localToWorldMatrix*StartMatrix;
		
		// Projecting world position of a target into local coordinate system
		LocalTargetPos=StartMatrix.inverse.MultiplyPoint3x4(Target.position);
		
		// Finding projection to XZ plane
		LocalTargetPos.y=0.0f;
		
		// Finding distance to projected target, if too small - no turns required
		TrgAngle=0;
		Length=LocalTargetPos.magnitude;
		if (Length>0.0001f)
		{
			// Making an angle in degrees
			CosA=LocalTargetPos.z/Length;
			SinA=-LocalTargetPos.x/Length;
			TrgAngle=Mathf.Acos(CosA)*Mathf.Rad2Deg;
			if (SinA<0.0f) TrgAngle=-TrgAngle;
		}
		
		// If some compensation if set - will take into account
		if (SlideCompensation!=0)
		{
			// Two cases: If distance is greater that compensation shift, then
			// calculate throughout rectangular triangle, otherwise we need to 
			// orient axis by +-90 degrees (depends on sign of compensation value)
			if (Mathf.Abs(SlideCompensation)<=Length)
			{
				SinA=SlideCompensation/Length;
				SlideAngle=Mathf.Asin(SinA)*Mathf.Rad2Deg;

				TrgAngle+=SlideAngle;
			}
			else
			{
				if (Mathf.Sign(SlideCompensation)>0)
					TrgAngle+=90;
				else
					TrgAngle-=90;
			}
		}
		
		return true;
	}
	
	

	// Convertion stage implementation. Plain and simple :)
	float ConvertAngle(float Angle)
	{
		return (Angle+PreOffset)*Gain+PostOffset;
	}
	
	
	
	// Limiting stage implementation. Will limit an Angle, then return true, if
	// limit is occured, and false otherwise
	bool LimitAngle(ref float Angle,float Minimum,float Maximum)
	{
		float   MinAngle;
		float   MaxAngle;
		float   NewMaxAngle;
		float   NewAngle;

		MinAngle=Minimum;
		MaxAngle=Maximum;

		// Limiting Angle is a tricky business. At first,
		// lets wrap everything into acceptable range
		if (MinAngle>=360.0f)	MinAngle-=360.0f;
		if (MinAngle<=-360.0f)	MinAngle+=360.0f;
		if (MaxAngle>=360.0f)	MaxAngle-=360.0f;
		if (MaxAngle<=-360.0f)	MaxAngle+=360.0f;

		// Turning everything by minus minimal angle to move into alternative
		// "coordinate system"
		NewMaxAngle=MaxAngle-MinAngle;
		NewAngle=Angle-MinAngle;

		// Flipping maximum and angle if they become negative
		if (NewMaxAngle<0.0f)	NewMaxAngle+=360;
		if (NewAngle<0.0f)		NewAngle+=360;

		// Now, if Angle has gone beyond then it is always greater that maximum
		if (NewAngle>NewMaxAngle)
		{
			// Now we need to check which distance closer: 360 or a maximum value
			// a 360 choise teleports us into 0
			if (NewAngle-NewMaxAngle<360-NewAngle)
				Angle=MaxAngle;
			else
				Angle=MinAngle;

			// Target angle is not reachable as its gone past limiter, so
			// we returning true
			return true;
		}
		
		// Target angle wasnt limited, so we returning false
		return false;
	}
	
	
	
	// Анимирует угол вращения, на вход принимает флаг срабатывания ограничения угла,
	// также устанавливает состояния флагов достижения позиции
	
	// Animation stage implementation. Does animation and flags setting.
	void Animate(bool IsLimited)
	{
		AnimationType   AnimType;
		float           TurnDir;
		float           DeltaAngle;
		
		// Lets define type of animation using Speed and AccelerationTime settings
		AnimType=AnimationType.Instant;
		if (Speed>0)
		{
			AnimType=AnimationType.ConstSpeed;
			if (AccelerationTime>0)
				AnimType=AnimationType.Accelerated;
		}

		// Depends on animation type
		switch (AnimType)
		{
			// Instant turn towards target
			case AnimationType.Instant:
				CurrentAngle=AlteredAngle;
				SetFinishState(true,IsLimited);
				break;

			// Turn to a target with constant speed
			case AnimationType.ConstSpeed:
				TurnDir=0.0f;
				if (AlteredAngle!=CurrentAngle)
					TurnDir=Mathf.Sign(AlteredAngle-CurrentAngle);
				
				DeltaAngle=TurnDir*Speed*Time.deltaTime;
				if (Mathf.Abs(AlteredAngle-CurrentAngle)<DeltaAngle || DeltaAngle==0)
				{
					CurrentAngle=AlteredAngle;
					SetFinishState(true,IsLimited);
				}
				else
				{
					CurrentAngle+=DeltaAngle;
					SetFinishState(false,IsLimited);
				}
				break;

			// Turn to target with acceleration, Speed parameter is used as maximum speed
			case AnimationType.Accelerated:
				CurrentAngle=IKUtility.IKRamp(CurrentAngle,AlteredAngle,Speed,AccelerationTime,ref CurrentSpeed,Time.deltaTime);

				if (CurrentAngle==AlteredAngle)
					SetFinishState(true,IsLimited);
				else
					SetFinishState(false,IsLimited);
				break;
		}
	}
	
	
	
	// Final stage. Just setting rotation to an object we drive.
	void SetRotation(float Angle)
	{
		Quaternion  LocalRotation;
		float       RadAngle;
		
		// Coverts angle into radians and divides by 2
		RadAngle=-Angle*Mathf.Deg2Rad/2;
		
		// Makes quaternion around Y axis
		LocalRotation.x=0.0f;
		LocalRotation.y=Mathf.Sin(RadAngle);
		LocalRotation.z=0.0f;
		LocalRotation.w=Mathf.Cos(RadAngle);

		// Setting full local orientation
		Origin.localRotation=StartRotation*LocalRotation;
	}
	
	
	
	// Sets flags of animation and target-reaching states
	void SetFinishState(bool State,bool IsLimited)
	{
		IsFinished=State;
		
		// If angle wasnt limited then target can be reached, otherwise - not
		if (State)
			IsTargetReached=!IsLimited;
		else
			IsTargetReached=false;
	}
	
	
	
	// Draws a debug-purpose visual
	public void DrawDebug()
	{
		Vector3     v1,v2;
		Vector3     VPos;
		Vector3     VForward;
		Vector3     VRight;
		Vector3     VUp;
		Quaternion  QRot;
		Quaternion  QStart;
		int         i;
		
		if (Origin==null)
			return;
		
		if (Origin.parent!=null)
			QStart=Origin.parent.rotation*StartRotation;
		else
			QStart=StartRotation;
		
		VPos=Origin.position;
		VForward=Origin.forward;
		VRight=Origin.right;
		VUp=Origin.up;
		
		if (VisualCircleSteps<5)
			VisualCircleSteps=5;

		// Default orientation marker
		v1=VPos;
		v2=v1+QStart*Vector3.forward*VisualRadius;
		Debug.DrawLine(v1,v2,Color.white);

		// Circle
		v1=VPos+VForward*VisualRadius;
		for (i=0;i<VisualCircleSteps;i++)
		{
			QRot=Quaternion.AngleAxis((i+1)*360/VisualCircleSteps,VUp);
			v2=QRot*VForward*VisualRadius+VPos;
			
			Debug.DrawLine(v1,v2,VisualCircleColor);
			v1=v2;
		}
		
		// Current orientation marker and a compensator
		v1=VPos;
		v2=v1+VRight*SlideCompensation;
		Debug.DrawLine(v1,v2,VisualCompensatorColor);
		
		v1=v2;
		v2=v1+VForward*VisualRadius;
		Debug.DrawLine(v1,v2,VisualCompensatorColor);
	}
}
