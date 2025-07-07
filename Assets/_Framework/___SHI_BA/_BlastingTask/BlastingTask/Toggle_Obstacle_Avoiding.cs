using UnityEngine;
using System.Collections.Generic;
using _SHI_BA;

namespace BioIK
{
    /// <summary>
    /// 씬에 있는 모든 ShapeDistance 타입의 BioObjective의 활성화 상태를
    /// true 또는 false 값으로 설정하는 최적화된 스크립트입니다.
    /// </summary>
    public class Toggle_Obstacle_Avoiding : MonoBehaviour
    {
        private List<ShapeDistance> allShapeDistanceObjectives;

        void Awake()
        {
            Debug.Log("[Toggle_Obstacle_Avoiding].Awake(), this.gameObject : " + this.gameObject.name);

            // 성능을 위해 시작 시 씬의 모든 ShapeDistance 컴포넌트를 한 번만 찾아 저장합니다.
            allShapeDistanceObjectives = new List<ShapeDistance>(FindObjectsByType<ShapeDistance>(FindObjectsSortMode.None));

            if (allShapeDistanceObjectives.Count == 0)
            {
                Debug.LogWarning("씬에서 ShapeDistance Objective를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// 모든 ShapeDistance BioObjective의 활성화 상태를 설정합니다.
        /// </summary>
        /// <param name="enable">true일 경우 활성화, false일 경우 비활성화합니다.</param>
        public void SetShapeDistanceObjectives(bool enable)
        {
            if (allShapeDistanceObjectives == null || allShapeDistanceObjectives.Count == 0)
            {
                return;
            }

            foreach (ShapeDistance objective in allShapeDistanceObjectives)
            {
                if (objective != null)
                {
                    objective.IsEnabled = enable;
                }
            }
            
            string status = enable ? "활성화" : "비활성화";
            Debug.Log($"모든 ShapeDistance Objective가 {status}되었습니다.");
        }

        //--- Context Menu 추가 ---
        // 아래 두 함수는 인스펙터의 컨텍스트 메뉴에서 직접 호출할 수 있습니다.

        [ContextMenu("모든 충돌 회피 기능 활성화 (Enable All Obstacles)")]
        private void EnableAllFromMenu()
        {
            Debug.Log("컨텍스트 메뉴를 통해 모든 충돌 회피 기능을 활성화합니다.");
            SetShapeDistanceObjectives(true);
        }

        [ContextMenu("모든 충돌 회피 기능 비활성화 (Disable All Obstacles)")]
        private void DisableAllFromMenu()
        {
            Debug.Log("컨텍스트 메뉴를 통해 모든 충돌 회피 기능을 비활성화합니다.");
            SetShapeDistanceObjectives(false);
        }
    }
}