using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// Two axis robot controller. Takes control over individual 
// IK-templates, maps prefab parameters, updates pseudo targets
// and main IK-chain. Robot goes to the main target and tries to
// maintain an angle of approach (it tries to align with target's
// Z axis
[ExecuteInEditMode]
public class TwoAxisRobotController : MonoBehaviour
{
	[Header("Controls")]
	public Transform    RobotTarget;        // Target of the robot
	public float        BaseEvasion=0.6f;   // Base of the robot cannot be closer to the target than this distance
	public float        TurnAngle;          // Spindel angle rotation
	public float        Fingers;            // Distance of each finger from spindel's center
	
	[Space(10)]
	public bool         DrawVisuals;        // If true, draws all IK-chain visuals
	
	[Header("Setup")]
	public IKMover      BaseMover;
	public IKAxis       BaseAxis;
	public IKArm        RobotArm;
	public IKAxis       AxisX1;
	public IKAxis       AxisY;
	public IKAxis       AxisX2;
	public IKAxis       AxisZ;
	public IKMover      Finger1;
	public IKMover      Finger2;
	public IKMover      Finger3;
	public IKAxis       BackBoneAxis;
	public Transform    PseudoTarget1;
	public Transform    PseudoTarget2;
	public float        ZOffset;
	public float        YOffset;

	
	
	// Once this script is enabled it turns an AutoUpdate
	// for each IK-template off, and sets DrawVisual as defined
	void OnEnable()
	{
		if (!CheckInit())
			return;
		
		SetAutoUpdate(false);
		if (Application.isPlaying)
			SetDrawVisuals(false);
		else
			SetDrawVisuals(DrawVisuals);
	}
	
	
	
	// Once controller is disabled it returns control to individual
	// IK-template scripts
	void OnDisable()
	{
		if (!CheckInit())
			return;
		
		SetAutoUpdate(true);
	}
	
	
	
	void LateUpdate()
	{
		// Checking components
		if (!CheckInit())
			return;
		
		// Setting visuals as required
		SetDrawVisuals(DrawVisuals);

		// Assigning a target each frame if it will be changed
		AxisY.Target=RobotTarget;
		AxisX2.Target=RobotTarget;
		AxisZ.Target=RobotTarget;
		
		// Transfering control settings to individual IK-components
		BaseMover.KeepDistance=BaseEvasion;
		AxisZ.TargetAngle=TurnAngle;
		Finger1.TargetPosition=Fingers;
		
		// Custom action. Calculates length of the first bone of a robot arm.
		RobotArm.OriginLength=CalculateArmBone1Length();
		
		// Updating IK-chain
		UpdatePseudoTargets();
		UpdateIKChain();
	}
	
	
	
	// Checks if all components in place
	bool CheckInit()
	{
		if (BaseMover==null || BaseAxis==null || RobotArm==null || AxisX1==null || AxisY==null ||
		    AxisX2==null || AxisZ==null || Finger1==null || Finger2==null || Finger3==null ||
			BackBoneAxis==null || PseudoTarget1==null || PseudoTarget2==null || RobotTarget==null)
			return false;
		else
			return true;
	}
	
	
	
	// Calculates length of an arm's first bone
	float CalculateArmBone1Length()
	{
		float   DistanceToTarget;
		float   DistanceToEndpoint;
		float   BoneLength;
		
		DistanceToTarget=Mathf.Clamp((RobotTarget.position-BaseMover.transform.position).magnitude,2,3);
		DistanceToEndpoint=Mathf.Clamp((RobotTarget.position.y-BaseMover.transform.position.y-YOffset)/2,0,1);
		BoneLength=Mathf.Clamp(DistanceToTarget-1+(DistanceToEndpoint),1,1.8f);
		
		return BoneLength;
	}
	
	
	
	// In order to proper function robot have 2 pseudo targets.
	// This routine updates them properly
	void UpdatePseudoTargets()
	{
		if (RobotTarget==null || PseudoTarget1==null || PseudoTarget2==null)
			return;
		
		PseudoTarget1.position=RobotTarget.position-RobotTarget.forward*ZOffset;
		PseudoTarget2.position=PseudoTarget1.position+Vector3.up*YOffset;
	}
	
	
	
	// Updates all IK-templates in proper execution order
	void UpdateIKChain()
	{
		BaseMover.ManualUpdate();
		BaseAxis.ManualUpdate();
		RobotArm.ManualUpdate();
		AxisX1.ManualUpdate();
		AxisY.ManualUpdate();
		AxisX2.ManualUpdate();
		AxisZ.ManualUpdate();
		Finger1.ManualUpdate();
		Finger2.ManualUpdate();
		Finger3.ManualUpdate();
		BackBoneAxis.ManualUpdate();
	}
	
	
	
	// Controls DrawVisual setting of the entire chain
	void SetDrawVisuals(bool State)
	{
		BaseMover.DrawVisual=State;
		BaseAxis.DrawVisual=State;
		RobotArm.DrawVisual=State;
		AxisX1.DrawVisual=State;
		AxisY.DrawVisual=State;
		AxisX2.DrawVisual=State;
		AxisZ.DrawVisual=State;
		Finger1.DrawVisual=State;
		Finger2.DrawVisual=State;
		Finger3.DrawVisual=State;
		BackBoneAxis.DrawVisual=State;
	}
	
	
	
	// Controls AutoUpdate setting of the entire chain
	void SetAutoUpdate(bool State)
	{
		BaseMover.AutoUpdate=State;
		BaseAxis.AutoUpdate=State;
		RobotArm.AutoUpdate=State;
		AxisX1.AutoUpdate=State;
		AxisY.AutoUpdate=State;
		AxisX2.AutoUpdate=State;
		AxisZ.AutoUpdate=State;
		Finger1.AutoUpdate=State;
		Finger2.AutoUpdate=State;
		Finger3.AutoUpdate=State;
		BackBoneAxis.AutoUpdate=State;
	}
}
