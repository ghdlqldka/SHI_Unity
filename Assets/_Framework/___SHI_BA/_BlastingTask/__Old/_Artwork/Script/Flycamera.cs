using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    public float lookSpeed = 2.0f;
    public float moveSpeed = 5.0f;
    public float boostMultiplier = 2.0f;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    private bool cursorLocked = true;

    void Start()
    {
        LockCursor(false); // 시작 시 마우스는 보이도록
    }

    void Update()
    {
        // Esc로 마우스 잠금/해제
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLocked = !cursorLocked;
            LockCursor(cursorLocked);
        }

        // 오른쪽 마우스 버튼을 누르고 있는 동안만 시점 회전 및 이동
        if (Input.GetMouseButton(1) && cursorLocked)
        {
            HandleMouseLook();
            HandleMovement();
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        rotationY += mouseX;

        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }

    void HandleMovement()
    {
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
            speed *= boostMultiplier;

        float horizontal = 0f;
        float vertical = 0f;
        float upDown = 0f;

        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;
        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.E)) upDown += 1f;
        if (Input.GetKey(KeyCode.Q)) upDown -= 1f;

        Vector3 move = new Vector3(horizontal, upDown, vertical);

        if (move != Vector3.zero)
            transform.Translate(move.normalized * speed * Time.deltaTime, Space.Self);
    }

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}