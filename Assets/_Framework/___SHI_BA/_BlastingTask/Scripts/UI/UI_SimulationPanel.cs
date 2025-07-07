using _SHI_BA;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SimulationPanel : MonoBehaviour
{
    private static string LOG_FORMAT = "<color=white><b>[UI_SimulationPanel]</b></color> {0}";
    //

    [SerializeField]
    protected TMP_InputField inputSelectedFaceIndex;
    [SerializeField]
    protected int _faceIndex = 1;

    [Space(10)]
    [SerializeField]
    protected BA_Main _main;
    [SerializeField]
    protected ViewManager _viewManager;

    protected virtual void Awake()
    {
        Debug.Assert(_main != null);
        Debug.Assert(_viewManager != null);
    }

    protected void Start()
    {
        inputSelectedFaceIndex.text = "" + _faceIndex;
    }

    public void OnEndEdit_MotionNumber(string _str)
    {
        Debug.LogFormat(LOG_FORMAT, "OnEndEdit_MotionNumber(), _str : " + _str);

        if (int.TryParse(_str, out int faceIndex) == false || faceIndex < 1)
        {
            inputSelectedFaceIndex.text = "" + _faceIndex;
            return;
        }

        _faceIndex = faceIndex;
    }

    public void OnClick_StartSingleMotion()
    {
        Debug.LogFormat(LOG_FORMAT, "OnClick_StartSingleMotion()");
        _viewManager.biodon();
        _main.ExecuteMotionForSelectedFace(_faceIndex);
    }

    public void OnClick_StartMultiMotion()
    {
        Debug.LogFormat(LOG_FORMAT, "OnClick_StartMultiMotion()");
        _main.ExecuteAllMotionsSequentially();
    }

    public void OnClick_ResetMotion()
    {
        Debug.LogFormat(LOG_FORMAT, "OnClick_ResetMotion()");
        _main.RestartScene();
    }
}
