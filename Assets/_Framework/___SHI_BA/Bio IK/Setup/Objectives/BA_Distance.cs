using BioIK;
using UnityEngine;

//!!!!!!
//This objective type is still under development
//!!!!!!

namespace _SHI_BA 
{

	//This objective aims to keep particular distances to the defined transform positions. This can be used to integrate
	//real-time collision avoidance. However, note that collision avoidance is typically a very challenging thing for motion generation,
	//so please do not expect any wonders or some sort of black magic. It works well for typical scenarios, but it will not solve the world for you.
	//Note that you should use preferably small weights in order to get good-looking results. Best thing is to play around with it and see what happens.
	//It is not generally clear how to chose those weights.
	// [AddComponentMenu("")]
	public class BA_Distance : BioIK._Distance
    {
        private static string LOG_FORMAT = "<color=#B604FF><b>[BA_Distance]</b></color> {0}";

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