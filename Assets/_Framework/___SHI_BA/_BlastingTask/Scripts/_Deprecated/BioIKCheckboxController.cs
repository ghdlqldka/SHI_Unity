using _SHI_BA;
using UnityEngine;
using UnityEngine.UI;

public class BioIKCheckboxController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Toggle checkboxBioIK; // UI Toggle (üũ�ڽ�)
    [SerializeField] private GameObject xObject; // BioIK�� ����ִ� x ������Ʈ

    private BioIK.BioIK bioIKComponent; // BioIK ������Ʈ ����
    private BA_BioIK _bioIKComponent
    {
        get 
        { 
            return bioIKComponent as BA_BioIK;
        }
    }

    void Start()
    {
        if (xObject != null)
        {
            bioIKComponent = xObject.GetComponent<BioIK.BioIK>();
        }

        if (checkboxBioIK == null)
        {
            checkboxBioIK = GameObject.Find("Checkbox_BioIk")?.GetComponent<Toggle>();
        }

        if (xObject == null)
        {
            xObject = GameObject.Find("x");
            if (xObject != null)
            {
                bioIKComponent = xObject.GetComponent<BioIK.BioIK>();
            }
        }

        InitializeCheckbox();

        if (checkboxBioIK != null)
        {
            checkboxBioIK.onValueChanged.AddListener(OnCheckboxChanged);
        }
    }

    void InitializeCheckbox()
    {
        if (checkboxBioIK != null && bioIKComponent != null)
        {
            checkboxBioIK.isOn = true;

            _bioIKComponent.autoIK = true;
        }
    }

    public void OnCheckboxChanged(bool isChecked)
    {
        if (_bioIKComponent != null)
        {
            _bioIKComponent.autoIK = isChecked;

        }
        else
        {
            //
        }
    }


    public void SetCheckbox(Toggle toggle)
    {
        if (checkboxBioIK != null && checkboxBioIK.onValueChanged != null)
        {
            checkboxBioIK.onValueChanged.RemoveListener(OnCheckboxChanged);
        }

        checkboxBioIK = toggle;

        if (checkboxBioIK != null)
        {
            checkboxBioIK.onValueChanged.AddListener(OnCheckboxChanged);
        }
    }

    public void SetXObject(GameObject obj)
    {
        xObject = obj;
        if (xObject != null)
        {
            bioIKComponent = xObject.GetComponent<BioIK.BioIK>();
        }
    }

    void OnDestroy()
    {
        if (checkboxBioIK != null && checkboxBioIK.onValueChanged != null)
        {
            checkboxBioIK.onValueChanged.RemoveListener(OnCheckboxChanged);
        }
    }

}