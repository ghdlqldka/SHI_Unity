using UnityEngine;
using System.Collections;



// Abstract base class of an IKConstructor primitive,
// from which every IK-component must be derived from
public abstract class IKBase : MonoBehaviour
{
	// Type of kinematic
	public enum KinematicType
	{
		Inverse,        // Automatic target following
		Forward         // Manual user control
	};
	
	// From what field a slave will read data to its TargetValue
	public enum MasterSource
	{
		TargetValue,    // Primary position/angle
		AlteredValue,   // Primary position/angle after conversion and limiting stage
		CurrentValue    // Animated position/angle
	};
	
	// Type of animation in use. Determined automatically by animation stage
	protected enum AnimationType
	{
		Instant,        // "Teleport" to the target within a single update
		ConstSpeed,     // Move to the target with constant speed
		Accelerated     // Accelerate to the target, then move with a constant speed, then decelerate
	};
	
	// True, if script is working in Edit mode
	[HideInInspector]
	public bool AutoUpdate=true;
	
	// Common method prototypes
	public abstract bool Init();
	public abstract void ManualUpdate();
}
