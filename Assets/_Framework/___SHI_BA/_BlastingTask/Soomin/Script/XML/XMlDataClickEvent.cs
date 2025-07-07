using UnityEngine;

public class XMlDataClickEvent : MonoBehaviour
{
    XMLData XMLData;
    public void SetData(XMLData _data)
    {
        XMLData = _data;

    }

    public void BtnClick()
    {
        ViewManager.Instance.SetInput(XMLData);
        SystemController.Instance.LineLoad(XMLData.motionXmlSeq);
    }
}
