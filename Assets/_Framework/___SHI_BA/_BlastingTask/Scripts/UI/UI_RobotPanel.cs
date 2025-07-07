using _SHI_BA;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RobotPanel : MonoBehaviour
{
    private static string LOG_FORMAT = "<color=white><b>[UI_RobotPanel]</b></color> {0}";

    [SerializeField]
    protected UI_MoveMode UI_MoveMode;

    [Space(10)]
    [SerializeField]
    protected RobotStartPose robotStartPose;

    [SerializeField]
    private Toggle checkboxBioIK;
    [SerializeField]
    private BA_BioIK _bioIK;

    [SerializeField]
    protected Go_to_One_Point_UI go_To_One_Point_UI;

    [Space(10)]
    [SerializeField]
    protected GameObject targetObject;
    // [SerializeField]
    // protected targetrot _targetRot;
    [SerializeField]
    protected TMP_InputField inputYRotation;

    [Space(10)]
    [SerializeField]
    protected UI_RobotJointController uI_RobotJointController;

    protected virtual void Awake()
    {
        Debug.Assert(robotStartPose != null);
    }

    protected virtual void Start()
    {
        checkboxBioIK.isOn = _bioIK.autoIK;
    }

    public void OnValueChanged_RobotPose(int index)
    {
        Debug.LogFormat(LOG_FORMAT, "OnValueChanged_RobotPose(), index : " + index);

        robotStartPose.ApplyPoseByName("");
    }

    public void OnValueChanged_AutoIK(bool isChecked)
    {
        Debug.LogFormat(LOG_FORMAT, "OnValueChanged_AutoIK(), isChecked : " + isChecked);

        _bioIK.autoIK = isChecked;
    }

    public void OnClick_GotoRobotButton()
    {
        Debug.LogFormat(LOG_FORMAT, "OnClick_GotoRobotButton");
        go_To_One_Point_UI.GoToOnePointAll_UI();
    }

    public void OnClick_RotateTargetButton()
    {
        Debug.LogFormat(LOG_FORMAT, "OnClick_RotateTargetButton");
        // _targetRot.ApplyYRotation();

        if (targetObject == null)
        {
            Debug.LogWarning("[targetrot] targetObject�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        float targetYRotation = 0f;
        // TMP_InputField���� �� �б�
        if (inputYRotation != null && !string.IsNullOrEmpty(inputYRotation.text))
        {
            float parsedValue;
            if (float.TryParse(inputYRotation.text, out parsedValue))
            {
                targetYRotation = parsedValue;
            }
            else
            {
                Debug.LogWarning("[targetrot] �Է°��� �ùٸ� ���ڰ� �ƴմϴ�.");
            }
        }

        Vector3 currentEuler = targetObject.transform.eulerAngles;
        targetObject.transform.eulerAngles = new Vector3(currentEuler.x, targetYRotation, currentEuler.z);
        Debug.Log($"[targetrot] {targetObject.name}�� Y�� ȸ���� {targetYRotation}���� �����߽��ϴ�.");
    }
}
