using UnityEngine;
using UnityEngine.UI;
using static UI_MoveMode;

public class JointModeController : MonoBehaviour
{
    [Header("üũ�ڽ� UI")]
    public Toggle checkboxAll;
    public Toggle checkboxGantry;
    public Toggle checkboxRobot;

    [Header("IK ��Ʈ�ѷ� ����")]
    public Gauntry_or_6Joint_or_all ikController;

    /*
    public enum JointMode
    {
        All,
        Gantry,
        Robot
    }
    */

    private JointMode currentMode = JointMode.All;

    void Start()
    {
        SetupInitialState();

        RegisterToggleEvents();
    }

    void SetupInitialState()
    {
        checkboxAll.isOn = true;
        checkboxGantry.isOn = false;
        checkboxRobot.isOn = false;

        currentMode = JointMode.All;

        ApplyCurrentMode();
    }

    void RegisterToggleEvents()
    {
        if (checkboxAll != null)
            checkboxAll.onValueChanged.AddListener((bool value) => OnToggleChanged(JointMode.All, value));

        if (checkboxGantry != null)
            checkboxGantry.onValueChanged.AddListener((bool value) => OnToggleChanged(JointMode.Gantry, value));

        if (checkboxRobot != null)
            checkboxRobot.onValueChanged.AddListener((bool value) => OnToggleChanged(JointMode.Robot, value));
    }

    void OnToggleChanged(JointMode mode, bool isChecked)
    {
        if (isChecked == false && currentMode == mode)
        {
            SetToggleWithoutEvent(mode, true);
            return;
        }

        if (isChecked == true)
        {
            SwitchToMode(mode);
        }
    }

    void SwitchToMode(JointMode newMode)
    {
        if (currentMode == newMode) return;

        SetToggleWithoutEvent(JointMode.All, false);
        SetToggleWithoutEvent(JointMode.Gantry, false);
        SetToggleWithoutEvent(JointMode.Robot, false);

        SetToggleWithoutEvent(newMode, true);

        currentMode = newMode;

        ApplyCurrentMode();

    }

    void SetToggleWithoutEvent(JointMode mode, bool value)
    {
        Toggle targetToggle = null;

        switch (mode)
        {
            case JointMode.All:
                targetToggle = checkboxAll;
                break;
            case JointMode.Gantry:
                targetToggle = checkboxGantry;
                break;
            case JointMode.Robot:
                targetToggle = checkboxRobot;
                break;
        }

        if (targetToggle != null)
        {
            targetToggle.SetIsOnWithoutNotify(value);
        }
    }

    void ApplyCurrentMode()
    {
        if (ikController == null)
        {
            return;
        }

        switch (currentMode)
        {
            // case JointMode.All:
            //     ikController.EnableAllJoints();
            //     Debug.Log("All Joints ��� ����");
            //     break;

            case JointMode.Gantry:
                ikController.EnableOnlyGauntry();
                break;

            case JointMode.Robot:
                ikController.EnableOnlyA1toA6();
                break;
        }
    }

    public void SetMode(JointMode mode)
    {
        SwitchToMode(mode);
    }

    public JointMode GetCurrentMode()
    {
        return currentMode;
    }
}