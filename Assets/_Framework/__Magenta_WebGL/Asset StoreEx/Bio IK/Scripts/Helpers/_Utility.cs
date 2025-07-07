using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BioIK 
{

	public static class _Utility/* : Utility*/
	{
		public static _BioSegment AddBioSegment(_BioIK character, Transform t) 
		{
#if UNITY_EDITOR
			if(Application.isPlaying == true) 
			{
				return (t.gameObject.AddComponent(typeof(_BioSegment)) as _BioSegment).Create(character);
			} 
			else
			{
				return (Undo.AddComponent(t.gameObject, typeof(_BioSegment)) as _BioSegment).Create(character);
			}
#else
			return (t.gameObject.AddComponent(typeof(_BioSegment)) as _BioSegment).Create(character);
#endif
		}

		public static BioJoint AddBioJoint(_BioSegment segment) 
		{
#if UNITY_EDITOR
			if(Application.isPlaying)
			{
				return (segment.gameObject.AddComponent(typeof(_BioJoint)) as _BioJoint).Create(segment);
			} 
			else 
			{
				return (Undo.AddComponent(segment.gameObject, typeof(_BioJoint)) as _BioJoint).Create(segment);
			}
#else
			return (segment.gameObject.AddComponent(typeof(_BioJoint)) as _BioJoint).Create(segment);
#endif
		}

		public static BioObjective AddObjective(_BioSegment segment, ObjectiveType type)
		{
#if UNITY_EDITOR
			if(Application.isPlaying) 
			{
				switch(type) 
				{
					case ObjectiveType.Position:
					return (segment.gameObject.AddComponent(typeof(_Position)) as BioObjective).Create(segment);

					case ObjectiveType.Orientation:
					return (segment.gameObject.AddComponent(typeof(_Orientation)) as BioObjective).Create(segment);

					case ObjectiveType.LookAt:
					return (segment.gameObject.AddComponent(typeof(_LookAt)) as BioObjective).Create(segment);;

					case ObjectiveType.Distance:
					return (segment.gameObject.AddComponent(typeof(_Distance)) as BioObjective).Create(segment);

					case ObjectiveType.JointValue:
					return (segment.gameObject.AddComponent(typeof(_JointValue)) as BioObjective).Create(segment);

					case ObjectiveType.Displacement:
					return (segment.gameObject.AddComponent(typeof(_Displacement)) as BioObjective).Create(segment);

					case ObjectiveType.Projection:
					return (segment.gameObject.AddComponent(typeof(_Projection)) as BioObjective).Create(segment);

					default:
						Debug.Assert(false);
						break;
				}
			} 
			else
			{
				switch(type) 
				{
					case ObjectiveType.Position:
					return (Undo.AddComponent(segment.gameObject, typeof(_Position)) as BioObjective).Create(segment);

					case ObjectiveType.Orientation:
					return (Undo.AddComponent(segment.gameObject, typeof(_Orientation)) as BioObjective).Create(segment);

					case ObjectiveType.LookAt:
					return (Undo.AddComponent(segment.gameObject, typeof(_LookAt)) as BioObjective).Create(segment);

					case ObjectiveType.Distance:
					return (Undo.AddComponent(segment.gameObject, typeof(_Distance)) as BioObjective).Create(segment);

					case ObjectiveType.JointValue:
					return (Undo.AddComponent(segment.gameObject, typeof(_JointValue)) as BioObjective).Create(segment);

					case ObjectiveType.Displacement:
					return (Undo.AddComponent(segment.gameObject, typeof(_Displacement)) as BioObjective).Create(segment);

					case ObjectiveType.Projection:
					return (Undo.AddComponent(segment.gameObject, typeof(_Projection)) as BioObjective).Create(segment);

                    default:
                        Debug.Assert(false);
                        break;
                }
			}
			return null;
#else
			switch(type) {
				case ObjectiveType.Position:
				return (segment.gameObject.AddComponent(typeof(_Position)) as BioObjective).Create(segment);

				case ObjectiveType.Orientation:
				return (segment.gameObject.AddComponent(typeof(_Orientation)) as BioObjective).Create(segment);

				case ObjectiveType.LookAt:
				return (segment.gameObject.AddComponent(typeof(_LookAt)) as BioObjective).Create(segment);;

				case ObjectiveType.Distance:
				return (segment.gameObject.AddComponent(typeof(_Distance)) as BioObjective).Create(segment);

				case ObjectiveType.JointValue:
				return (segment.gameObject.AddComponent(typeof(_JointValue)) as BioObjective).Create(segment);

				case ObjectiveType.Displacement:
				return (segment.gameObject.AddComponent(typeof(_Displacement)) as BioObjective).Create(segment);

				case ObjectiveType.Projection:
				return (segment.gameObject.AddComponent(typeof(_Projection)) as BioObjective).Create(segment);

				default:
						Debug.Assert(false);
						break;
			}
			return null;
#endif
        }

    }

}