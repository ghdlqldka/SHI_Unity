using UnityEngine;
using TMPro;

public class TargetMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;          // 이동 속도
    public float verticalSpeed = 3f;      // 수직 이동 속도

    [Header("Rotation Settings")]
    public float rotationSpeed = 90f;     // Y축 회전 속도 (도/초)

    [Header("UI Input Fields")]
    public TMP_InputField Input_X;        // X 좌표 표시용 InputField
    public TMP_InputField Input_Y;        // Y 좌표 표시용 InputField
    public TMP_InputField Input_Z;        // Z 좌표 표시용 InputField
    public TMP_InputField Input_Targetrot; // Y축 회전값 표시용 InputField
    
    [Header("Display Settings")]
    public int decimalPlaces = 2;         // 소수점 자리수

    private Transform target;             // Target 오브젝트의 Transform
    private Vector3 lastPosition;         // 이전 프레임의 위치
    private float lastRotationY;          // 이전 프레임의 Y축 회전값

    void Start()
    {
        // 현재 스크립트가 있는 오브젝트를 Target으로 설정
        target = transform;
        lastPosition = target.position;
        lastRotationY = target.eulerAngles.y;
        
        // 초기 위치와 회전을 InputField에 설정
        UpdatePositionDisplay();
        UpdateRotationDisplay();
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        
        // 위치가 변경되었는지 확인하고 UI 업데이트
        if (target.position != lastPosition)
        {
            UpdatePositionDisplay();
            lastPosition = target.position;
        }
        
        // 회전이 변경되었는지 확인하고 UI 업데이트
        float currentRotationY = target.eulerAngles.y;
        if (Mathf.Abs(currentRotationY - lastRotationY) > 0.01f) // 작은 오차 무시
        {
            UpdateRotationDisplay();
            lastRotationY = currentRotationY;
        }
    }

    void HandleMovement()
    {
        // 현재 위치
        Vector3 currentPosition = target.position;
        // 이동 벡터 초기화
        Vector3 movement = Vector3.zero;

        // X, Z 축 이동 (WASD)
        if (Input.GetKey(KeyCode.W))        // 앞으로 (Z+)
        {
            movement.z += 1f;
        }
        if (Input.GetKey(KeyCode.S))        // 뒤로 (Z-)
        {
            movement.z -= 1f;
        }
        if (Input.GetKey(KeyCode.A))        // 왼쪽 (X-)
        {
            movement.x -= 1f;
        }
        if (Input.GetKey(KeyCode.D))        // 오른쪽 (X+)
        {
            movement.x += 1f;
        }

        // Y 축 이동 (QE)
        if (Input.GetKey(KeyCode.Q))        // 아래로 (Y-)
        {
            movement.y -= 1f;
        }
        if (Input.GetKey(KeyCode.E))        // 위로 (Y+)
        {
            movement.y += 1f;
        }

        // 대각선 이동 시 속도 정규화 (X, Z축)
        Vector3 horizontalMovement = new Vector3(movement.x, 0, movement.z);
        if (horizontalMovement.magnitude > 1f)
        {
            horizontalMovement = horizontalMovement.normalized;
            movement.x = horizontalMovement.x;
            movement.z = horizontalMovement.z;
        }

        // 실제 이동 적용
        Vector3 horizontalMove = new Vector3(movement.x, 0, movement.z) * moveSpeed * Time.deltaTime;
        Vector3 verticalMove = new Vector3(0, movement.y, 0) * verticalSpeed * Time.deltaTime;
        target.position = currentPosition + horizontalMove + verticalMove;
    }

    void HandleRotation()
    {
        float rotationInput = 0f;

        // Y축 회전 (Z, C)
        if (Input.GetKey(KeyCode.Z))        // 왼쪽 회전 (반시계 방향)
        {
            rotationInput -= 1f;
        }
        if (Input.GetKey(KeyCode.C))        // 오른쪽 회전 (시계 방향)
        {
            rotationInput += 1f;
        }

        // 회전 적용
        if (rotationInput != 0f)
        {
            float rotationAmount = rotationInput * rotationSpeed * Time.deltaTime;
            target.Rotate(0, rotationAmount, 0, Space.Self);
        }
    }
    
    // 위치 정보를 InputField에 업데이트하는 함수
    void UpdatePositionDisplay()
    {
        Vector3 pos = target.position;
        
        // 각 InputField가 할당되어 있는지 확인하고 업데이트
        if (Input_X != null)
        {
            Input_X.text = pos.x.ToString("F" + decimalPlaces);
        }
        
        if (Input_Y != null)
        {
            Input_Y.text = pos.y.ToString("F" + decimalPlaces);
        }
        
        if (Input_Z != null)
        {
            Input_Z.text = pos.z.ToString("F" + decimalPlaces);
        }
    }
    
    // 회전 정보를 InputField에 업데이트하는 함수
    void UpdateRotationDisplay()
    {
        if (Input_Targetrot != null)
        {
            float rotationY = target.eulerAngles.y;
            Input_Targetrot.text = rotationY.ToString("F" + decimalPlaces);
        }
    }
    
    // 수동으로 위치와 회전 업데이트를 강제하는 공용 함수 (필요시 사용)
    public void ForceUpdateDisplay()
    {
        UpdatePositionDisplay();
        UpdateRotationDisplay();
    }
    
    // InputField에서 좌표를 읽어서 타겟 위치를 설정하는 함수 (옵션)
    public void SetPositionFromInputFields()
    {
        Vector3 newPosition = target.position;
        
        if (Input_X != null && float.TryParse(Input_X.text, out float x))
        {
            newPosition.x = x;
        }
        
        if (Input_Y != null && float.TryParse(Input_Y.text, out float y))
        {
            newPosition.y = y;
        }
        
        if (Input_Z != null && float.TryParse(Input_Z.text, out float z))
        {
            newPosition.z = z;
        }
        
        target.position = newPosition;
        lastPosition = newPosition;
    }
    
    // InputField에서 회전값을 읽어서 타겟 회전을 설정하는 함수 (옵션)
    public void SetRotationFromInputField()
    {
        if (Input_Targetrot != null && float.TryParse(Input_Targetrot.text, out float rotY))
        {
            Vector3 newRotation = target.eulerAngles;
            newRotation.y = rotY;
            target.eulerAngles = newRotation;
            lastRotationY = rotY;
        }
    }

    // Inspector에서 키 확인용
    // void OnGUI()
    // {
    //     GUILayout.BeginArea(new Rect(10, 10, 300, 200));
    //     GUILayout.Label("키 조작:");
    //     GUILayout.Label("W/S: 앞/뒤 이동 (Z축)");
    //     GUILayout.Label("A/D: 왼쪽/오른쪽 이동 (X축)");
    //     GUILayout.Label("Q/E: 아래/위 이동 (Y축)");
    //     GUILayout.Label("Z/C: 왼쪽/오른쪽 회전 (Y축)");
    //     GUILayout.Label($"현재 위치: {target.position}");
    //     GUILayout.Label($"현재 회전: {target.eulerAngles}");
    //     GUILayout.EndArea();
    // }
}