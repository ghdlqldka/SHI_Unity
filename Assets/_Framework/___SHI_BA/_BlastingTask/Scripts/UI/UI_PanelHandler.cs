using System.Collections.Generic;
using UnityEngine;

public class UI_PanelHandler : MonoBehaviour
{
    private static string LOG_FORMAT = "<color=white><b>[UI_PanelHandler]</b></color> {0}";

    [SerializeField] 
    protected GameObject panel_00;
    [SerializeField] 
    protected GameObject panel_01;
    [SerializeField] 
    protected GameObject panel_02;
    [SerializeField] 
    protected GameObject panel_03;
    [SerializeField] 
    protected GameObject panel_04;

    [Space(10)]
    [SerializeField] 
    private int defaultPanelIndex = 0; // 기본으로 보여줄 패널 (1~5)

    private List<GameObject> panelObjList = new List<GameObject>();

    protected virtual void Awake()
    {
        panelObjList.Add(panel_00);
        panelObjList.Add(panel_01);
        panelObjList.Add(panel_02);
        panelObjList.Add(panel_03);
        panelObjList.Add(panel_04);

#if DEBUG
        for (int i = 0; i < panelObjList.Count; i++)
        {
            Debug.Assert(panelObjList[i] != null);
        }
#endif
    }

    protected virtual void Start()
    {
        ShowPanel(defaultPanelIndex);
    }

    protected void ShowPanel(int index)
    {
        for (int i = 0; i < panelObjList.Count; i++)
        {
            if (index != i)
            {
                panelObjList[i].SetActive(false);
            }
            else
            {
                panelObjList[i].SetActive(true);
            }
        }
    }

    public void OnClickButton_ShowRobotPanel()
    {
        Debug.LogFormat(LOG_FORMAT, "OnClickButton_<b>ShowRobotPanel</b>()");
        ShowPanel(0);
    }

    public void OnClickButton_ShowMotionPanel()
    {
        Debug.LogFormat(LOG_FORMAT, "OnClickButton_<b>ShowMotionPanel</b>()");
        ShowPanel(1);
    }

    public void OnClickButton_ShowSimulationPanel()
    {
        ShowPanel(2);
    }

    public void OnClickButton_ShowSendXmlPanel()
    {
        ShowPanel(3);
    }

    public void OnClickButton_ShowLoadXmlPanel()
    {
        ShowPanel(4);
    }
}
