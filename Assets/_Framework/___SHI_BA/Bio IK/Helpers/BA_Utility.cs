using UnityEngine;
using BioIK;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _SHI_BA 
{

	public static class BA_Utility/* : Utility*/
	{
		public static BA_BioSegment AddBioSegment(BA_BioIK character, Transform t) 
		{
#if UNITY_EDITOR
			if(Application.isPlaying == true) 
			{
				return (t.gameObject.AddComponent(typeof(BA_BioSegment)) as BA_BioSegment).Create(character);
			} 
			else
			{
				return (Undo.AddComponent(t.gameObject, typeof(BA_BioSegment)) as BA_BioSegment).Create(character);
			}
#else
			return (t.gameObject.AddComponent(typeof(BA_BioSegment)) as BA_BioSegment).Create(character);
#endif
		}

		public static BioJoint AddBioJoint(BA_BioSegment segment) 
		{
#if UNITY_EDITOR
			if(Application.isPlaying)
			{
				return (segment.gameObject.AddComponent(typeof(BA_BioJoint)) as BA_BioJoint).Create(segment);
			} 
			else 
			{
				return (Undo.AddComponent(segment.gameObject, typeof(BA_BioJoint)) as BA_BioJoint).Create(segment);
			}
#else
			return (segment.gameObject.AddComponent(typeof(BA_BioJoint)) as BA_BioJoint).Create(segment);
#endif
        }

        public static BioObjective AddObjective(BA_BioSegment segment, ObjectiveType type)
		{
#if UNITY_EDITOR
			if(Application.isPlaying) 
			{
				switch(type) 
				{
					case ObjectiveType.Position:
					return (segment.gameObject.AddComponent(typeof(BA_Position)) as BioObjective).Create(segment);

					case ObjectiveType.Orientation:
					return (segment.gameObject.AddComponent(typeof(BA_Orientation)) as BioObjective).Create(segment);

					case ObjectiveType.LookAt:
					return (segment.gameObject.AddComponent(typeof(BA_LookAt)) as BioObjective).Create(segment);;

					case ObjectiveType.Distance:
					return (segment.gameObject.AddComponent(typeof(BA_Distance)) as BioObjective).Create(segment);

					case ObjectiveType.JointValue:
					return (segment.gameObject.AddComponent(typeof(BA_JointValue)) as BioObjective).Create(segment);

					case ObjectiveType.Displacement:
					return (segment.gameObject.AddComponent(typeof(BA_Displacement)) as BioObjective).Create(segment);

					case ObjectiveType.Projection:
					return (segment.gameObject.AddComponent(typeof(BA_Projection)) as BioObjective).Create(segment);
                    
					case ObjectiveType.ShapeDistance:
					return (segment.gameObject.AddComponent(typeof(ShapeDistance)) as BioObjective).Create(segment);
                    
					default:
						Debug.Assert(false);
						break;
				}
			} 
			else
			{
				switch(type) {
					case ObjectiveType.Position:
					return (Undo.AddComponent(segment.gameObject, typeof(BA_Position)) as BioObjective).Create(segment);

					case ObjectiveType.Orientation:
					return (Undo.AddComponent(segment.gameObject, typeof(BA_Orientation)) as BioObjective).Create(segment);

					case ObjectiveType.LookAt:
					return (Undo.AddComponent(segment.gameObject, typeof(BA_LookAt)) as BioObjective).Create(segment);

					case ObjectiveType.Distance:
					return (Undo.AddComponent(segment.gameObject, typeof(BA_Distance)) as BioObjective).Create(segment);

					case ObjectiveType.JointValue:
					return (Undo.AddComponent(segment.gameObject, typeof(BA_JointValue)) as BioObjective).Create(segment);

					case ObjectiveType.Displacement:
					return (Undo.AddComponent(segment.gameObject, typeof(BA_Displacement)) as BioObjective).Create(segment);

					case ObjectiveType.Projection:
					return (Undo.AddComponent(segment.gameObject, typeof(BA_Projection)) as BioObjective).Create(segment);
                    
					case ObjectiveType.ShapeDistance:
                    return (Undo.AddComponent(segment.gameObject, typeof(ShapeDistance)) as BioObjective).Create(segment);

                    default:
                        Debug.Assert(false);
                        break;
                }
			}
			return null;
#else
			switch(type) {
				case ObjectiveType.Position:
				return (segment.gameObject.AddComponent(typeof(BA_Position)) as BioObjective).Create(segment);

				case ObjectiveType.Orientation:
				return (segment.gameObject.AddComponent(typeof(BA_Orientation)) as BioObjective).Create(segment);

				case ObjectiveType.LookAt:
				return (segment.gameObject.AddComponent(typeof(BA_LookAt)) as BioObjective).Create(segment);;

				case ObjectiveType.Distance:
				return (segment.gameObject.AddComponent(typeof(BA_Distance)) as BioObjective).Create(segment);

				case ObjectiveType.JointValue:
				return (segment.gameObject.AddComponent(typeof(BA_JointValue)) as BioObjective).Create(segment);

				case ObjectiveType.Displacement:
				return (segment.gameObject.AddComponent(typeof(BA_Displacement)) as BioObjective).Create(segment);

				case ObjectiveType.Projection:
				return (segment.gameObject.AddComponent(typeof(BA_Projection)) as BioObjective).Create(segment);

				case ObjectiveType.ShapeDistance:
				return (segment.gameObject.AddComponent(typeof(ShapeDistance)) as BioObjective).Create(segment);

				default:
						Debug.Assert(false);
						break;
			}
			return null;
#endif
        }

    }

}