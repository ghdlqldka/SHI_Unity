using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI; // GraphicRaycaster용
public class OrbitCamera : MonoBehaviour
{
    public Transform target;             // 타겟 (0,0,0)
    public float distance = 20f;          // 현재 카메라 거리
    public float zoomSpeed = 5f;         // 휠 줌 속도
    public float minDistance = 2f;       // 최소 줌 거리
    public float maxDistance = 30f;      // 최대 줌 거리

    public float xSpeed = 120f;          // 좌우 회전 속도
    public float ySpeed = 120f;          // 상하 회전 속도
    public float yMinLimit = -20f;       // 상하 회전 각도 제한
    public float yMaxLimit = 80f;

    private float x = 0.0f;
    private float y = 0.0f;

    private float desiredDistance;       // 목표 거리
    private float zoomVelocity = 0.0f;   // 부드러운 줌을 위한 속도값
    private float smoothTime = 0.2f;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("Target is not assigned. Creating a dummy target at (0,0,0).");
            GameObject dummy = new GameObject("CameraTarget");
            dummy.transform.position = Vector3.zero;
            target = dummy.transform;
        }
        x = 135f;          // 좌우 각도
        y = 30f;        // 위에서 내려다보는 각도 (Y축 회전)

        desiredDistance = distance;
        UpdateCameraPosition(); // 시작 시 바로 적용
    }
    bool IsPointerOverUI()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;
    }

    void LateUpdate()
    {
        if (IsPointerOverUI())
            return;

        if (!target) return;

        // 1. 좌클릭 드래그로 카메라 회전
        if (Input.GetMouseButton(0))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
        }

        // 2. 우클릭 드래그로 타겟의 Y 위치 조절
        if (Input.GetMouseButton(1))
        {
            float moveY = Input.GetAxis("Mouse Y") * ySpeed * 0.01f;
            Vector3 pos = target.position;
            pos.y += moveY;
            pos.y = Mathf.Clamp(pos.y, 0f, 10f);
            target.position = pos;
        }

        // 3. 마우스 휠 줌
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            desiredDistance -= scroll * zoomSpeed;
            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        }

        if (Input.GetMouseButton(2))
        {
            float panSpeed = distance * 0.002f; // 줌 거리 기반으로 속도 자동 조절
            float moveX = -Input.GetAxis("Mouse X") * panSpeed * xSpeed * 0.5f;
            float moveY = -Input.GetAxis("Mouse Y") * panSpeed * ySpeed * 0.5f;

            // 현재 카메라 방향 기준으로 이동 축 계산
            Vector3 right = transform.right;
            Vector3 up = transform.up;

            // 타겟 위치 이동
            target.position += right * moveX + up * moveY;
        }

        // 4. 부드러운 거리 변화
        distance = Mathf.SmoothDamp(distance, desiredDistance, ref zoomVelocity, smoothTime);

        // 5. 위치/회전 적용
        UpdateCameraPosition();

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1))
            return;
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.position = target.position + offset;
        transform.rotation = rotation;
    }
}