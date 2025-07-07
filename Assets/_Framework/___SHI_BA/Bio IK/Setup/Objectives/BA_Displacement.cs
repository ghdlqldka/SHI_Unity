using BioIK;
using UnityEngine;

//!!!!!!
//This objective type is still under development
//!!!!!!

namespace _SHI_BA 
{
	//This objective aims to minimise the joint configuration changes between consecutive solutions.
	//It should only be used once as it acts as a global objective for the whole body posture.
	//Preferably add it to the root of your character.
	// [AddComponentMenu("")]
	public class BA_Displacement : BioIK._Displacement
    {
        private static string LOG_FORMAT = "<color=#B604FF><b>[BA_Displacement]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
        }

        protected override void OnDestroy()
        {
            if (Segment != null && Segment.Character != null)
            {
                // Segment의 Objectives 배열에서 자기 자신을 찾아 제거합니다.
                for (int i = 0; i < Segment.Objectives.Length; i++)
                {
                    if (Segment.Objectives[i] == this)
                    {
                        // 배열에서 해당 요소를 제거하기 위한 간단한 리스트 변환 방식
                        var objectiveList = new System.Collections.Generic.List<BioObjective>(Segment.Objectives);
                        objectiveList.RemoveAt(i);
                        Segment.Objectives = objectiveList.ToArray();
                        break;
                    }
                }

                // 가장 중요한 단계: IK 시스템 전체를 갱신하여 변경사항을 반영합니다.
                if (Application.isPlaying)
                { // 플레이 중일 때만 Refresh를 호출하여 에디터 오류 방지
                    Segment.Character.Refresh();
                }
            }
        }
    }
	
}