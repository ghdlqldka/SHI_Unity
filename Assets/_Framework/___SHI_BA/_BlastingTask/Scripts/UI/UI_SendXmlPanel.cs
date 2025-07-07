using _SHI_BA;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SendXmlPanel : MonoBehaviour
{
    private static string LOG_FORMAT = "<color=white><b>[UI_SendXmlPanel]</b></color> {0}";
    //

    [SerializeField]
    protected SystemController _systemController;
    [SerializeField]
    protected XMLSender _xmlSender;

    [SerializeField]
    protected TMP_InputField Input_XMLname;
    [SerializeField]
    protected string _ui_fileName = "Motion_01";

    protected virtual void Awake()
    {
        Debug.LogFormat(LOG_FORMAT, "Awake()");

        Debug.Assert(_systemController != null);
        Debug.Assert(_xmlSender != null);
    }

    protected void Start()
    {
        Input_XMLname.text = _ui_fileName;
    }

    public void OnEndEdit_MotionFileName(string _str)
    {
        _ui_fileName = _str;
        _xmlSender._ui_fileName = _ui_fileName;
    }

    public void OnClick_SendXmlCurrentPose()
    {
        _systemController.SingleXMLPostRequest(8);
    }

    public void OnClick_SendXmlSimulationData()
    {
        _systemController.XMLPostRequest(8);
    }
}
