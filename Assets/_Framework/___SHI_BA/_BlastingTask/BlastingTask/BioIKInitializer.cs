// BioIKInitializer.cs
using UnityEngine;
using BioIK; // BioIK 네임스페이스 사용

// BioIK 컴포넌트가 있는 게임 오브젝트에 함께 추가해주세요.
public class BioIKInitializer : MonoBehaviour
{
    void Start()
    {
        BioIK.BioIK bioIK = GetComponent<BioIK.BioIK>();
        if (bioIK != null)
        {
            // 컴포넌트를 비활성화 후 즉시 다시 활성화하여
            // OnDisable() -> OnEnable() 사이클을 강제로 실행시킵니다.
            // 이를 통해 IK 시스템이 완전히 재초기화됩니다.
            bioIK.enabled = false;
            bioIK.enabled = true;

            Debug.Log("BioIK component has been re-initialized to ensure all states are synchronized.");
        }
    }
}