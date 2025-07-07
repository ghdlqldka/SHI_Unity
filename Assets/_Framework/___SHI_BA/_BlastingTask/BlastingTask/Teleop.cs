using UnityEngine;

public class GantryTeleop : MonoBehaviour
{
    public Transform target; // 움직일 타겟 오브젝트
    public float moveStep = 0.01f; // 10mm
    public Gauntry_or_6Joint_or_all jointController; // 인스펙터에서 할당

    private Vector3 originalTargetPosition;
    private Quaternion originalTargetRotation;
    private bool savedOriginal = false;

    void Start()
    {
        // if (jointController != null)
        // {
        //     jointController.EnableOnlyGauntry();
        // }

        // // 타겟의 원래 위치/회전 저장 (한 번만)
        // if (target != null && !savedOriginal)
        // {
        //     originalTargetPosition = target.position;
        //     originalTargetRotation = target.rotation;
        //     savedOriginal = true;
        // }
    }

    void Update()
    {
        Vector3 move = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.A)) move.x -= moveStep;
        if (Input.GetKeyDown(KeyCode.D)) move.x += moveStep;
        if (Input.GetKeyDown(KeyCode.W)) move.y += moveStep;
        if (Input.GetKeyDown(KeyCode.S)) move.y -= moveStep;
        if (Input.GetKeyDown(KeyCode.E)) move.z += moveStep;
        if (Input.GetKeyDown(KeyCode.Q)) move.z -= moveStep;

        if (target != null && move != Vector3.zero)
        {
            target.position += move;
        }
    }

    void OnDisable()
    {
        RestoreTargetTransform();
    }

    void OnApplicationQuit()
    {
        RestoreTargetTransform();
    }

    private void RestoreTargetTransform()
    {
        if (target != null && savedOriginal)
        {
            target.position = originalTargetPosition;
            target.rotation = originalTargetRotation;
        }
    }
}