using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
public class  BPSDataClickEvent: MonoBehaviour ,IBpsData
{
    private BPSData BPSData;
    /// <summary>
    /// 데이터 적용
    /// </summary>
    public void SetData(BPSData _data)
    {
        BPSData = _data;

    }
    /// <summary>
    /// 버튼클릭이벤트
    /// </summary>
    public void BtnClick()
    {
        ViewManager.Instance.SetInput(BPSData);
        
    }
}
