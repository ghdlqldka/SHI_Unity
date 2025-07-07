using UnityEngine;

public class GameOBJXYZ : MonoBehaviour
{
    public GameObject targetObject; // ZZZ 오브젝트를 드래그하세요
    public Camera targetCamera;     // 사용할 카메라를 수동으로 드래그하세요

    void OnGUI()
    {
        if (targetObject == null || targetCamera == null)
            return;

        Vector3 worldPos = targetObject.transform.position;
        Vector3 screenPos = targetCamera.WorldToScreenPoint(worldPos);

        // 카메라 뒤에 있는 경우 무시
        if (screenPos.z < 0)
            return;

        screenPos.y = Screen.height - screenPos.y;

        string posText = $"X: {worldPos.x:F2}, Y: {worldPos.y:F2}, Z: {worldPos.z:F2}";

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        style.fontSize = 14;

        GUI.Label(new Rect(screenPos.x, screenPos.y, 200, 20), posText, style);
    }
}