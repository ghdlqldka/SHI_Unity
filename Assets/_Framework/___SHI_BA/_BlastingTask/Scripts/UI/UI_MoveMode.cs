using _SHI_BA;
using BioIK;
using UnityEngine;
using UnityEngine.UI;

public class UI_MoveMode : MonoBehaviour
{
    private static string LOG_FORMAT = "<color=white><b>[UI_MoveMode]</b></color> {0}";

    [SerializeField]
    protected Toggle checkboxAll;
    [SerializeField]
    protected Toggle checkboxGantry;
    [SerializeField]
    protected Toggle checkboxRobot;

    public enum JointMode
    {
        All,
        Gantry,
        Robot
    }

    private JointMode currentMode = JointMode.All;

    [SerializeField]
    protected Gauntry_or_6Joint_or_all ikController;

    protected virtual void Awake()
    {
        Debug.LogFormat(LOG_FORMAT, "Awake()");
    }

    protected virtual void Start()
    {
        checkboxAll.isOn = true;
        checkboxGantry.isOn = false;
        checkboxRobot.isOn = false;

        currentMode = JointMode.All;

        Checkbox_MoveR.isOn = false;
        Checkbox_MoveS.isOn = true;
    }

    public void OnValueChanged_CheckboxAll(bool isOn)
    {
        Debug.LogFormat(LOG_FORMAT, "OnValueChanged_CheckboxAll(), isOn : " + isOn);

        if (isOn == false && currentMode == JointMode.All)
        {
            checkboxAll.SetIsOnWithoutNotify(true);
            return;
        }

        currentMode = JointMode.All;
        // ikController.EnableAllJoints();

        checkboxGantry.SetIsOnWithoutNotify(false);
        checkboxRobot.SetIsOnWithoutNotify(false);
    }

    public void OnValueChanged_CheckboxGantry(bool isOn)
    {
        Debug.LogFormat(LOG_FORMAT, "OnValueChanged_CheckboxGantry(), isOn : " + isOn);

        if (isOn == false && currentMode == JointMode.Gantry)
        {
            checkboxGantry.SetIsOnWithoutNotify(true);
            return;
        }

        currentMode = JointMode.Gantry;
        ikController.EnableOnlyGauntry();

        checkboxAll.SetIsOnWithoutNotify(false);
        // checkboxGantry.SetIsOnWithoutNotify(false);
        checkboxRobot.SetIsOnWithoutNotify(false);
    }

    public void OnValueChanged_CheckboxRobot(bool isOn)
    {
        Debug.LogFormat(LOG_FORMAT, "OnValueChanged_CheckboxRobot(), isOn : " + isOn);

        if (isOn == false && currentMode == JointMode.Robot)
        {
            checkboxRobot.SetIsOnWithoutNotify(true);
            return;
        }

        currentMode = JointMode.Robot;
        ikController.EnableOnlyA1toA6();

        checkboxAll.SetIsOnWithoutNotify(false);
        checkboxGantry.SetIsOnWithoutNotify(false);
        // checkboxRobot.SetIsOnWithoutNotify(false);
    }

    public void SetMode(JointMode mode)
    {
        checkboxAll.SetIsOnWithoutNotify(false);
        checkboxGantry.SetIsOnWithoutNotify(false);
        checkboxRobot.SetIsOnWithoutNotify(false);

        if (mode == JointMode.All) 
        {
            checkboxAll.SetIsOnWithoutNotify(true);
            OnValueChanged_CheckboxAll(true);
        }
        else if (mode == JointMode.Gantry)
        {
            checkboxGantry.SetIsOnWithoutNotify(true);
            OnValueChanged_CheckboxGantry(true);
        }
        else if (mode == JointMode.Robot)
        {
            checkboxRobot.SetIsOnWithoutNotify(true);
            OnValueChanged_CheckboxRobot(true);
        }
        else
        {
            Debug.Assert(false);
        }
    }








    /// <summary>
    /// //////////////////////////////////////////////////////////////
    /// </summary>



    [Space(20)]
    [SerializeField]
    protected Toggle Checkbox_MoveR;
    [SerializeField]
    protected Toggle Checkbox_MoveS;
    [SerializeField]
    private BA_BioIK bioIKScript; // BioIK 스크립트 참조

    public void OnValueChanged_RealisticToggle(bool isOn)
    {
        Debug.LogFormat(LOG_FORMAT, "OnValueChanged_RealisticToggle(), isOn : " + isOn);

        if (isOn)
        {
            // Instantaneous 체크박스 해제
            if (Checkbox_MoveS != null && Checkbox_MoveS.isOn)
            {
                Checkbox_MoveS.SetIsOnWithoutNotify(false);
            }

            // BioIK Motion Type을 Realistic으로 변경
            bioIKScript.MotionType = MotionType.Realistic;
        }
    }

    public void OnValueChanged_InstantaneousToggle(bool isOn)
    {
        Debug.LogFormat(LOG_FORMAT, "OnValueChanged_InstantaneousToggle(), isOn : " + isOn);

        if (isOn)
        {
            // Realistic 체크박스 해제
            if (Checkbox_MoveR != null && Checkbox_MoveR.isOn)
            {
                // Checkbox_MoveR.isOn = false;
                Checkbox_MoveR.SetIsOnWithoutNotify(false);
            }

            // BioIK Motion Type을 Instantaneous로 변경
            bioIKScript.MotionType = MotionType.Instantaneous;
        }
    }





}
