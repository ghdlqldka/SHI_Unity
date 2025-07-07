using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using _SHI_BA;

public class UI_RobotJointController : MonoBehaviour
{
    private static string LOG_FORMAT = "<color=white><b>[UI_RobotJointController]</b></color> {0}";

    [Header("UI References")]
    [SerializeField] 
    private Slider[] jointSliders = new Slider[6];

    [SerializeField]
    private TMP_InputField[] jointInputFields = new TMP_InputField[6];

    [Header("Checkbox References")]
    [SerializeField] 
    private Toggle autoIKCheckbox; // Auto IK üũ�ڽ� (BioIK�� autoIK)
    [SerializeField]
    private Toggle robotCheckbox; // Checkbox_Robot üũ�ڽ�
    [SerializeField] 
    private Toggle moveSCheckbox; // Checkbox_MoveS üũ�ڽ�

    [Header("Pose Dropdown Reference")]
    [SerializeField] 
    private RobotPoseDropdown poseDropdown; // ���� ��Ӵٿ� ��Ʈ�ѷ�

    // �� ����Ʈ�� �ּ�/�ִ� ���� (�� ����)
    private readonly float[] minAngles = { -185f, -150f, -75f, -180f, -125f, -350f };
    private readonly float[] maxAngles = { 185f, 85f, 210f, 180f, 125f, 350f };

    // ����Ʈ �̸� (������)
    private readonly string[] jointNames = { "A1", "A2", "A3", "A4", "A5", "A6" };

    [SerializeField]
    private BA_BioIK bioIKComponent;
    private BA_BioIK _bioIKComponent
    {
        get
        {
            return bioIKComponent as BA_BioIK;
        }
    }
    private bool isUpdatingFromCode = false; // ���� ���� ����

    // �ʱ� TargetValue ����� (������ �� ����)
    private double[] initialTargetValues = new double[6];
    private bool initialValuesSaved = false;

    [SerializeField]
    protected ViewManager viewManager;
    [SerializeField]
    protected MousePathController mousePathController;

    protected virtual void Awake()
    {
#if DEBUG
        for (int i = 0; i < 6; i++)
        {
            Debug.Assert(jointSliders[i] != null);
            Debug.Assert(jointInputFields[i] != null);
        }

        Debug.Assert(bioIKComponent != null);
        Debug.Assert(autoIKCheckbox != null);
        Debug.Assert(robotCheckbox != null);
        Debug.Assert(moveSCheckbox != null);
        Debug.Assert(poseDropdown != null);
#endif
    }

    void Start()
    {
        Debug.LogFormat(LOG_FORMAT, "Start()");

        InitializeJointControls();

        // BioIK���� ���� Target Value�� �о�ͼ� UI �ʱ�ȭ
        Invoke("UpdateUIFromBioIK", 0.1f); // �ణ�� ������ �ΰ� ���� (BioIK �ʱ�ȭ ���)

        // �ʱ� TargetValue ����
        Invoke("SaveInitialTargetValues", 0.2f); // UI ������Ʈ �� �ʱⰪ ����
    }

    void InitializeJointControls()
    {
        for (int i = 0; i < 6; i++)
        {
            // �����̴� ���� ����
            jointSliders[i].minValue = minAngles[i];
            jointSliders[i].maxValue = maxAngles[i];
            // �ʱⰪ�� 0���� ����������, Start()���� ���� Target Value�� ������Ʈ��
            jointSliders[i].value = 0f;
            jointSliders[i].wholeNumbers = false; // �Ҽ��� ���

            // �Է� �ʵ� �ʱⰪ ���� (���� ���� Start()���� ������Ʈ��)
            jointInputFields[i].text = "0";
            jointInputFields[i].contentType = TMP_InputField.ContentType.DecimalNumber;
        }
    }

    public void OnSliderChanged(int jointIndex, float value)
    {
        if (isUpdatingFromCode)
            return;

        // �� ���� üũ
        value = Mathf.Clamp(value, minAngles[jointIndex], maxAngles[jointIndex]);

        isUpdatingFromCode = true;

        // �Է� �ʵ� ������Ʈ
        if (jointInputFields[jointIndex] != null)
        {
            jointInputFields[jointIndex].text = value.ToString("F1");
        }

        // üũ�ڽ� ���� ����
        UpdateCheckboxStates();

        // �κ� ����Ʈ ������Ʈ
        UpdateRobotJoint(jointIndex, value);

        isUpdatingFromCode = false;

        Debug.Log($"{jointNames[jointIndex]} �����̴�: {value:F1}��");
    }

    public void OnValueChanged_A1Slider(float value)
    {
        OnSliderChanged(0, value);
        viewManager.biodoff();
        viewManager.ChangeA1(value);
    }

    public void OnValueChanged_A2Slider(float value)
    {
        OnSliderChanged(1, value);
        viewManager.biodoff();
        viewManager.ChangeA2(value);
    }

    public void OnValueChanged_A3Slider(float value)
    {
        OnSliderChanged(2, value);
        viewManager.biodoff();
        viewManager.ChangeA3(value);
    }

    public void OnValueChanged_A4Slider(float value)
    {
        OnSliderChanged(3, value);
        viewManager.biodoff();
        viewManager.ChangeA4(value);
    }

    public void OnValueChanged_A5Slider(float value)
    {
        OnSliderChanged(4, value);
        viewManager.biodoff();
        viewManager.ChangeA5(value);
    }

    public void OnValueChanged_A6Slider(float value)
    {
        OnSliderChanged(5, value);
        viewManager.biodoff();
        viewManager.ChangeA6(value);
    }


    public void OnInputFieldChanged(int jointIndex, string text)
    {
        if (isUpdatingFromCode)
            return;

        if (float.TryParse(text, out float value))
        {
            // �� ���� üũ
            value = Mathf.Clamp(value, minAngles[jointIndex], maxAngles[jointIndex]);

            isUpdatingFromCode = true;

            // �����̴� ������Ʈ
            if (jointSliders[jointIndex] != null)
            {
                jointSliders[jointIndex].value = value;
            }

            // �Է� �ʵ� �� ���� (������ ��� ���)
            if (jointInputFields[jointIndex] != null)
            {
                jointInputFields[jointIndex].text = value.ToString("F1");
            }

            // üũ�ڽ� ���� ����
            UpdateCheckboxStates();

            // �κ� ����Ʈ ������Ʈ
            UpdateRobotJoint(jointIndex, value);

            isUpdatingFromCode = false;

            Debug.Log($"{jointNames[jointIndex]} �Է�: {value:F1}��");
        }
        else
        {
            // �߸��� �Է��� ��� �����̴� ������ ����
            if (jointInputFields[jointIndex] != null && jointSliders[jointIndex] != null)
            {
                jointInputFields[jointIndex].text = jointSliders[jointIndex].value.ToString("F1");
            }
        }
    }

    public void OnValueChanged_A1InputField(string text)
    {
        OnInputFieldChanged(0, text);
    }

    public void OnValueChanged_A2InputField(string text)
    {
        OnInputFieldChanged(1, text);
    }

    public void OnValueChanged_A3InputField(string text)
    {
        OnInputFieldChanged(2, text);
    }

    public void OnValueChanged_A4InputField(string text)
    {
        OnInputFieldChanged(3, text);
    }

    public void OnValueChanged_A5InputField(string text)
    {
        OnInputFieldChanged(4, text);
    }

    public void OnValueChanged_A6InputField(string text)
    {
        OnInputFieldChanged(5, text);
    }

    void UpdateRobotJoint(int jointIndex, float angleDegrees)
    {
        string jointName = jointNames[jointIndex]; // "A1", "A2", etc.

        foreach (var segment in bioIKComponent.Segments)
        {
            if (segment.Joint != null && segment.Joint.enabled && segment.Transform.name == jointName)
            {
                double targetValue = (double)angleDegrees;

                if (segment.Joint.Z.Enabled)
                {
                    segment.Joint.Z.TargetValue = targetValue; // ���� �ʵ� ����!
                }
                else if (segment.Joint.X.Enabled)
                {
                    segment.Joint.X.TargetValue = targetValue; // ���� �ʵ� ����!
                }
                else if (segment.Joint.Y.Enabled)
                {
                    segment.Joint.Y.TargetValue = targetValue; // ���� �ʵ� ����!
                }

                // �߿�: �� ���� �� BioIK ������Ʈ Ʈ����
                ForceUpdateBioIK(segment);

                return;
            }
        }
    }

    void ForceUpdateBioIK(BioIK.BioSegment segment)
    {
        if (segment.Joint != null)
        {
            // BioJoint�� ������Ʈ �޼���� ȣ��
            segment.Joint.PrecaptureAnimation();
            segment.Joint.PostcaptureAnimation();
            segment.Joint.UpdateData();
            segment.Joint.ProcessMotion();
        }
    }

    // ���� BioIK���� ����Ʈ Target Value �о����
    public void UpdateUIFromBioIK()
    {
        int updatedJoints = 0;
        for (int i = 0; i < 6; i++)
        {
            string jointName = jointNames[i]; // "A1", "A2", etc.

            // �ش� �̸��� ����Ʈ ã��
            foreach (var segment in bioIKComponent.Segments)
            {
                if (segment.Joint != null && segment.Joint.enabled && segment.Transform.name == jointName)
                {
                    double currentValue = 0.0;

                    // Ȱ��ȭ�� �࿡�� ���� TargetValue �ʵ� ���� �б�
                    if (segment.Joint.Z.Enabled)
                    {
                        currentValue = segment.Joint.Z.TargetValue; // ���� �ʵ� ����
                    }
                    else if (segment.Joint.X.Enabled)
                    {
                        currentValue = segment.Joint.X.TargetValue; // ���� �ʵ� ����
                    }
                    else if (segment.Joint.Y.Enabled)
                    {
                        currentValue = segment.Joint.Y.TargetValue; // ���� �ʵ� ����
                    }

                    // double�� float�� ��ȯ
                    float currentValueFloat = (float)currentValue;

                    // ���� üũ
                    currentValueFloat = Mathf.Clamp(currentValueFloat, minAngles[i], maxAngles[i]);

                    isUpdatingFromCode = true;

                    // UI ������Ʈ
                    if (jointSliders[i] != null)
                    {
                        jointSliders[i].value = currentValueFloat;
                    }
                    if (jointInputFields[i] != null)
                    {
                        jointInputFields[i].text = currentValueFloat.ToString("F1");
                    }

                    isUpdatingFromCode = false;

                    updatedJoints++;
                    break; // �ش� ����Ʈ�� ã������ ���� ����
                }
            }
        }
    }

    public void SyncUIFromBioIK()
    {
        UpdateUIFromBioIK();
    }

    // �ʱ� TargetValue ����
    void SaveInitialTargetValues()
    {
        for (int i = 0; i < 6; i++)
        {
            string jointName = jointNames[i];

            foreach (var segment in bioIKComponent.Segments)
            {
                if (segment.Joint != null && segment.Joint.enabled && segment.Transform.name == jointName)
                {
                    double initialValue = 0.0;

                    // Ȱ��ȭ�� �࿡�� ���� TargetValue ����
                    if (segment.Joint.Z.Enabled)
                    {
                        initialValue = segment.Joint.Z.TargetValue;
                    }
                    else if (segment.Joint.X.Enabled)
                    {
                        initialValue = segment.Joint.X.TargetValue;
                    }
                    else if (segment.Joint.Y.Enabled)
                    {
                        initialValue = segment.Joint.Y.TargetValue;
                    }

                    initialTargetValues[i] = initialValue;
                    break;
                }
            }
        }

        initialValuesSaved = true;
    }

    public void ResetAllJointsToZero()
    {
        for (int i = 0; i < 6; i++)
        {
            SetJointToValue(i, 0f);
        }
    }

    public void OnClick_ResetAllJointsToInitial()
    {
        if (!initialValuesSaved)
        {
            Debug.LogWarning("�ʱⰪ�� ������� �ʾҽ��ϴ�. ���� ������ �����ϼ���.");
            return;
        }

        for (int i = 0; i < 6; i++)
        {
            float initialValue = (float)initialTargetValues[i];
            SetJointToValue(i, initialValue);
        }
    }

    public void OnClick_TargetToClickPosition()
    {
        mousePathController.TargetMove();
    }

    void SetJointToValue(int jointIndex, float value)
    {
        isUpdatingFromCode = true;

        if (jointSliders[jointIndex] != null)
        {
            jointSliders[jointIndex].value = value;
        }
        if (jointInputFields[jointIndex] != null)
        {
            jointInputFields[jointIndex].text = value.ToString("F1");
        }

        UpdateRobotJoint(jointIndex, value);

        isUpdatingFromCode = false;
    }

    public void OnResetToZeroButtonClick()
    {
        ResetAllJointsToZero();
    }

    void UpdateCheckboxStates()
    {
        if (autoIKCheckbox != null)
        {
            autoIKCheckbox.isOn = false;
        }

        if (_bioIKComponent != null)
        {
            _bioIKComponent.autoIK = false;
        }

        if (robotCheckbox != null)
        {
            robotCheckbox.isOn = true;
        }

        if (moveSCheckbox != null)
        {
            moveSCheckbox.isOn = true;
        }

        if (poseDropdown != null)
        {
            poseDropdown.SetPoseToFreePose();
        }
    }

    public void ManualUpdateCheckboxStates()
    {
        UpdateCheckboxStates();
    }

    public void SetAutoIKCheckbox(Toggle toggle)
    {
        autoIKCheckbox = toggle;
    }

    public void SetRobotCheckbox(Toggle toggle)
    {
        robotCheckbox = toggle;
    }

    public void SetMoveSCheckbox(Toggle toggle)
    {
        moveSCheckbox = toggle;
    }

    public void SetPoseDropdown(RobotPoseDropdown dropdown)
    {
        poseDropdown = dropdown;
    }

    // Ư�� ����Ʈ ���� ���� (�ܺο��� ȣ�� ����)
    public void SetJointAngle(int jointIndex, float angleDegrees)
    {
        if (jointIndex < 0 || jointIndex >= 6)
            return;

        angleDegrees = Mathf.Clamp(angleDegrees, minAngles[jointIndex], maxAngles[jointIndex]);

        if (jointSliders[jointIndex] != null)
        {
            jointSliders[jointIndex].value = angleDegrees;
        }
    }

    // ���� ����Ʈ ���� ��������
    public float GetJointAngle(int jointIndex)
    {
        if (jointIndex < 0 || jointIndex >= 6 || jointSliders[jointIndex] == null)
            return 0f;

        return jointSliders[jointIndex].value;
    }

    // ����Ʈ ���� ���� ���� ��������
    public Vector2 GetJointRange(int jointIndex)
    {
        if (jointIndex < 0 || jointIndex >= 6)
            return Vector2.zero;

        return new Vector2(minAngles[jointIndex], maxAngles[jointIndex]);
    }

}