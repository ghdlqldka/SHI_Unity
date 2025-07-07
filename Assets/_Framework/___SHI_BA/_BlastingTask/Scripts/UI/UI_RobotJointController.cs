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
    private Toggle autoIKCheckbox; // Auto IK 체크박스 (BioIK의 autoIK)
    [SerializeField]
    private Toggle robotCheckbox; // Checkbox_Robot 체크박스
    [SerializeField] 
    private Toggle moveSCheckbox; // Checkbox_MoveS 체크박스

    [Header("Pose Dropdown Reference")]
    [SerializeField] 
    private RobotPoseDropdown poseDropdown; // 포즈 드롭다운 컨트롤러

    // 각 조인트의 최소/최대 각도 (도 단위)
    private readonly float[] minAngles = { -185f, -150f, -75f, -180f, -125f, -350f };
    private readonly float[] maxAngles = { 185f, 85f, 210f, 180f, 125f, 350f };

    // 조인트 이름 (디버깅용)
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
    private bool isUpdatingFromCode = false; // 무한 루프 방지

    // 초기 TargetValue 저장용 (시작할 때 저장)
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

        // BioIK에서 현재 Target Value를 읽어와서 UI 초기화
        Invoke("UpdateUIFromBioIK", 0.1f); // 약간의 지연을 두고 실행 (BioIK 초기화 대기)

        // 초기 TargetValue 저장
        Invoke("SaveInitialTargetValues", 0.2f); // UI 업데이트 후 초기값 저장
    }

    void InitializeJointControls()
    {
        for (int i = 0; i < 6; i++)
        {
            // 슬라이더 범위 설정
            jointSliders[i].minValue = minAngles[i];
            jointSliders[i].maxValue = maxAngles[i];
            // 초기값은 0도로 설정하지만, Start()에서 실제 Target Value로 업데이트됨
            jointSliders[i].value = 0f;
            jointSliders[i].wholeNumbers = false; // 소수점 허용

            // 입력 필드 초기값 설정 (실제 값은 Start()에서 업데이트됨)
            jointInputFields[i].text = "0";
            jointInputFields[i].contentType = TMP_InputField.ContentType.DecimalNumber;
        }
    }

    public void OnSliderChanged(int jointIndex, float value)
    {
        if (isUpdatingFromCode)
            return;

        // 값 범위 체크
        value = Mathf.Clamp(value, minAngles[jointIndex], maxAngles[jointIndex]);

        isUpdatingFromCode = true;

        // 입력 필드 업데이트
        if (jointInputFields[jointIndex] != null)
        {
            jointInputFields[jointIndex].text = value.ToString("F1");
        }

        // 체크박스 상태 변경
        UpdateCheckboxStates();

        // 로봇 조인트 업데이트
        UpdateRobotJoint(jointIndex, value);

        isUpdatingFromCode = false;

        Debug.Log($"{jointNames[jointIndex]} 슬라이더: {value:F1}°");
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
            // 값 범위 체크
            value = Mathf.Clamp(value, minAngles[jointIndex], maxAngles[jointIndex]);

            isUpdatingFromCode = true;

            // 슬라이더 업데이트
            if (jointSliders[jointIndex] != null)
            {
                jointSliders[jointIndex].value = value;
            }

            // 입력 필드 값 보정 (범위를 벗어난 경우)
            if (jointInputFields[jointIndex] != null)
            {
                jointInputFields[jointIndex].text = value.ToString("F1");
            }

            // 체크박스 상태 변경
            UpdateCheckboxStates();

            // 로봇 조인트 업데이트
            UpdateRobotJoint(jointIndex, value);

            isUpdatingFromCode = false;

            Debug.Log($"{jointNames[jointIndex]} 입력: {value:F1}°");
        }
        else
        {
            // 잘못된 입력인 경우 슬라이더 값으로 복원
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
                    segment.Joint.Z.TargetValue = targetValue; // 직접 필드 접근!
                }
                else if (segment.Joint.X.Enabled)
                {
                    segment.Joint.X.TargetValue = targetValue; // 직접 필드 접근!
                }
                else if (segment.Joint.Y.Enabled)
                {
                    segment.Joint.Y.TargetValue = targetValue; // 직접 필드 접근!
                }

                // 중요: 값 변경 후 BioIK 업데이트 트리거
                ForceUpdateBioIK(segment);

                return;
            }
        }
    }

    void ForceUpdateBioIK(BioIK.BioSegment segment)
    {
        if (segment.Joint != null)
        {
            // BioJoint의 업데이트 메서드들 호출
            segment.Joint.PrecaptureAnimation();
            segment.Joint.PostcaptureAnimation();
            segment.Joint.UpdateData();
            segment.Joint.ProcessMotion();
        }
    }

    // 현재 BioIK에서 조인트 Target Value 읽어오기
    public void UpdateUIFromBioIK()
    {
        int updatedJoints = 0;
        for (int i = 0; i < 6; i++)
        {
            string jointName = jointNames[i]; // "A1", "A2", etc.

            // 해당 이름의 조인트 찾기
            foreach (var segment in bioIKComponent.Segments)
            {
                if (segment.Joint != null && segment.Joint.enabled && segment.Transform.name == jointName)
                {
                    double currentValue = 0.0;

                    // 활성화된 축에서 현재 TargetValue 필드 직접 읽기
                    if (segment.Joint.Z.Enabled)
                    {
                        currentValue = segment.Joint.Z.TargetValue; // 직접 필드 접근
                    }
                    else if (segment.Joint.X.Enabled)
                    {
                        currentValue = segment.Joint.X.TargetValue; // 직접 필드 접근
                    }
                    else if (segment.Joint.Y.Enabled)
                    {
                        currentValue = segment.Joint.Y.TargetValue; // 직접 필드 접근
                    }

                    // double을 float로 변환
                    float currentValueFloat = (float)currentValue;

                    // 범위 체크
                    currentValueFloat = Mathf.Clamp(currentValueFloat, minAngles[i], maxAngles[i]);

                    isUpdatingFromCode = true;

                    // UI 업데이트
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
                    break; // 해당 조인트를 찾았으니 루프 종료
                }
            }
        }
    }

    public void SyncUIFromBioIK()
    {
        UpdateUIFromBioIK();
    }

    // 초기 TargetValue 저장
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

                    // 활성화된 축에서 현재 TargetValue 저장
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
            Debug.LogWarning("초기값이 저장되지 않았습니다. 먼저 게임을 시작하세요.");
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

    // 특정 조인트 각도 설정 (외부에서 호출 가능)
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

    // 현재 조인트 각도 가져오기
    public float GetJointAngle(int jointIndex)
    {
        if (jointIndex < 0 || jointIndex >= 6 || jointSliders[jointIndex] == null)
            return 0f;

        return jointSliders[jointIndex].value;
    }

    // 조인트 각도 범위 정보 가져오기
    public Vector2 GetJointRange(int jointIndex)
    {
        if (jointIndex < 0 || jointIndex >= 6)
            return Vector2.zero;

        return new Vector2(minAngles[jointIndex], maxAngles[jointIndex]);
    }

}