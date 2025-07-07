using _SHI_BA;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_LoadXmlPanel : MonoBehaviour
{
    private static string LOG_FORMAT = "<color=white><b>[UI_LoadXmlPanel]</b></color> {0}";
    //

    [SerializeField]
    protected GameObject xmlCreateFormObj;
    [SerializeField]
    protected SystemController _systemController;
    [SerializeField]
    protected ViewManager _viewManager;
    [SerializeField]
    protected XMLSender _xmlSender;

    [Header("UI 참조")]
    [SerializeField]
    private TextMeshProUGUI xmlNameText; // XMLname Text (TMP)

    [Header("현재 선택된 XML")]
    [SerializeField]
    private string currentSelectedXmlName = ""; // 현재 선택된 XML 이름

    public string CurrentSelectedXmlName
    {
        get
        {
            return currentSelectedXmlName;
        }
        private set
        {
            currentSelectedXmlName = value;
            xmlNameText.text = value;
            Debug.Log($"선택된 XML: {currentSelectedXmlName}");
        }
    }

    protected virtual void Awake()
    {
        Debug.LogFormat(LOG_FORMAT, "Awake()");

        Debug.Assert(xmlCreateFormObj != null);
        Debug.Assert(_systemController != null);
        Debug.Assert(_viewManager != null);
        Debug.Assert(xmlNameText != null);
    }

    protected void Start()
    {
        xmlNameText.text = CurrentSelectedXmlName;
    }

    public void OnClick_Create()
    {
        xmlCreateFormObj.SetActive(true);
    }

    public void OnClick_Load()
    {
        _systemController.XMLPostRequest(5);
        // _viewManager.OnXML();
    }

    public void OnClick_Update()
    {
        _systemController.XMLPostRequest(6);
    }

    public void OnEndEdit_XmlRemark(string _str)
    {
        _viewManager.OnXmlRemarkEdit(_str);
    }

    public void OnClick_XmlDataSimulation()
    {
        _xmlSender.SendCurrentPoseXML();
    }
}
