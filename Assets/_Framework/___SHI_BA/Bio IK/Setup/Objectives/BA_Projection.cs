using BioIK;
using UnityEngine;

namespace _SHI_BA
{

    //This objective aims to minimise both the translational and rotational distance between
    //the projected transform with respect to the normal of the game object to other collideable objects in the scene.
    // [AddComponentMenu("")]
    public class BA_Projection : BioIK._Projection
    {

        private static string LOG_FORMAT = "<color=#E0BA4B><b>[BA_Projection]</b></color> {0}";

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