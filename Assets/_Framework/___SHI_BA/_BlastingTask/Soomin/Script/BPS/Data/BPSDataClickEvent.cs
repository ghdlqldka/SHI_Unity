using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
public class  BPSDataClickEvent: MonoBehaviour ,IBpsData
{
    private BPSData BPSData;
    /// <summary>
    /// ������ ����
    /// </summary>
    public void SetData(BPSData _data)
    {
        BPSData = _data;

    }
    /// <summary>
    /// ��ưŬ���̺�Ʈ
    /// </summary>
    public void BtnClick()
    {
        ViewManager.Instance.SetInput(BPSData);
        
    }
}
