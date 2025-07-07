using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI; // GraphicRaycaster��

public class BA_OrbitCamera : MonoBehaviour
{
    public Transform target;             // Ÿ�� (0,0,0)
    public float distance = 20f;          // ���� ī�޶� �Ÿ�
    public float zoomSpeed = 5f;         // �� �� �ӵ�
    public float minDistance = 2f;       // �ּ� �� �Ÿ�
    public float maxDistance = 30f;      // �ִ� �� �Ÿ�
    public float xSpeed = 120f;          // �¿� ȸ�� �ӵ�
    public float ySpeed = 120f;          // ���� ȸ�� �ӵ�
    public float yMinLimit = -20f;       // ���� ȸ�� ���� ����
    public float yMaxLimit = 80f;
    private float x = 0.0f;
    private float y = 0.0f;
    private float desiredDistance;       // ��ǥ �Ÿ�
    private float zoomVelocity = 0.0f;   // �ε巯�� ���� ���� �ӵ���
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
        x = -45f;          // �¿� ����
        y = 20f;        // ������ �����ٺ��� ���� (Y�� ȸ��)
        desiredDistance = distance;
        UpdateCameraPosition(); // ���� �� �ٷ� ����
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

        // ���� �巡�� ���� ���� ī�޶� ���� ��Ȱ��ȭ
        // if (Plane_and_NV.IsEdgeDragging)
        // {
        //     // �ܸ� ��� (�� �Է�)
        //     float dragScroll = Input.GetAxis("Mouse ScrollWheel");
        //     if (Mathf.Abs(dragScroll) > 0.01f)
        //     {
        //         desiredDistance -= dragScroll * zoomSpeed;
        //         desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        //     }

        //     // �ε巯�� �Ÿ� ��ȭ
        //     distance = Mathf.SmoothDamp(distance, desiredDistance, ref zoomVelocity, smoothTime);

        //     // ��ġ/ȸ�� ����
        //     UpdateCameraPosition();
        //     return;
        // }

        if (!target) return;

        // 1. ��Ŭ�� �巡�׷� ī�޶� ȸ��
        if (Input.GetMouseButton(0))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
        }

        // 2. ��Ŭ�� �巡�׷� Ÿ���� Y ��ġ ����
        if (Input.GetMouseButton(1))
        {
            float moveY = Input.GetAxis("Mouse Y") * ySpeed * 0.01f;
            Vector3 pos = target.position;
            pos.y += moveY;
            pos.y = Mathf.Clamp(pos.y, 0f, 10f);
            target.position = pos;
        }

        // 3. ���콺 �� ��
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            desiredDistance -= scroll * zoomSpeed;
            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        }

        if (Input.GetMouseButton(2))
        {
            float panSpeed = distance * 0.002f; // �� �Ÿ� ������� �ӵ� �ڵ� ����
            float moveX = -Input.GetAxis("Mouse X") * panSpeed * xSpeed * 0.5f;
            float moveY = -Input.GetAxis("Mouse Y") * panSpeed * ySpeed * 0.5f;

            // ���� ī�޶� ���� �������� �̵� �� ���
            Vector3 right = transform.right;
            Vector3 up = transform.up;

            // Ÿ�� ��ġ �̵�
            target.position += right * moveX + up * moveY;
        }

        // 4. �ε巯�� �Ÿ� ��ȭ
        distance = Mathf.SmoothDamp(distance, desiredDistance, ref zoomVelocity, smoothTime);

        // 5. ��ġ/ȸ�� ����
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