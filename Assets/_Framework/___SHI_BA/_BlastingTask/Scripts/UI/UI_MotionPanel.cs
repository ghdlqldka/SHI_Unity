using System.Collections.Generic;
using _SHI_BA;
using TMPro;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using UnityEngine.UI;

public class UI_MotionPanel : MonoBehaviour
{
    private static string LOG_FORMAT = "<color=white><b>[UI_MotionPanel]</b></color> {0}";

    [SerializeField]
    protected ViewManager _viewManager;
    [SerializeField]
    protected BA_Main _main;
    [SerializeField]
    protected SystemController systemController;
    [SerializeField]
    protected Plane_and_NV plane_And_NV;

    [Space(10)]
    [SerializeField]
    protected TMP_InputField inputMotionVertical;
    [ReadOnly]
    [SerializeField]
    protected float manualMotionVerticalValue = 8f;
    [SerializeField]
    protected TMP_InputField inputMotionBPS;
    [ReadOnly]
    [SerializeField]
    protected float manualMotionBPSValue = 60f;
    [SerializeField]
    protected TMP_InputField inputField_Acc;
    [SerializeField]
    protected TMP_InputField inputField_Spd;

    [SerializeField]
    protected Toggle gizmoToggle;

    protected void Awake()
    {
        Debug.Assert(_main != null);
        Debug.Assert(systemController != null);
        Debug.Assert(plane_And_NV != null);
    }

    protected void Start()
    {
        manualMotionVerticalValue = _main.manualMotionVerticalValue;
        manualMotionBPSValue = _main.manualMotionBPSValue;

        inputMotionVertical.text = "" + manualMotionVerticalValue;
        inputMotionBPS.text = "" + manualMotionBPSValue;
    }

    public void OnEndEdit_QuadBPS(string _str) // 작업면 거리
    {
        Debug.LogFormat(LOG_FORMAT, "OnEndEdit_QuadBPS(), _str : " + _str);
        _viewManager.OnQuadBPSEdit(_str);
    }

    public void OnEndEdit_MotionVertical(string _str)
    {
        Debug.LogFormat(LOG_FORMAT, "OnEndEdit_MotionVertical(), _str : " + _str);

        if (string.IsNullOrEmpty(_str) == false && float.TryParse(_str, out float val) == true)
        {
            manualMotionVerticalValue = val;
            _main.manualMotionVerticalValue = manualMotionVerticalValue;
        }
        else
        {
            inputMotionVertical.text = "" + manualMotionVerticalValue;
        }
    }

    public void OnEndEdit_MotionBPS(string _str)
    {
        Debug.LogFormat(LOG_FORMAT, "OnEndEdit_MotionBPS(), _str : " + _str);

        if (string.IsNullOrEmpty(_str) == false && float.TryParse(_str, out float val) == true)
        {
            manualMotionBPSValue = val;
            _main.manualMotionBPSValue = manualMotionBPSValue;
            _viewManager.OnMotionBPSEdit(_str);
        }
        else
        {
            inputMotionBPS.text = "" + manualMotionBPSValue;
        }
    }

    public void OnEndEdit_Speed(string _str)
    {
        Debug.LogFormat(LOG_FORMAT, "OnEndEdit_Speed(), _str : " + _str);
        _viewManager.OnSPDEdit(_str);
    }

    public void OnEndEdit_Acc(string _str)
    {
        Debug.LogFormat(LOG_FORMAT, "OnEndEdit_Acc(), _str : " + _str);
        _viewManager.OnACCEdit(_str);
    }


    public void OnClick_CreateCubeButton()
    {
        Debug.LogFormat(LOG_FORMAT, "OnClick_CreateCubeButton()");

        plane_And_NV.LoadDataFromJSONInEditor();
        plane_And_NV.ToggleThickPlaneVisibility();

    }

    public void OnClick_CreateMotionButton()
    {
        Debug.LogFormat(LOG_FORMAT, "OnClick_CreateMotionButton()");
        _main.GenerateAndVisualizePaths();
        systemController.BPSPostRequest(2);
    }

    public void OnClick_UpdateMotionButton()
    {
        Debug.LogFormat(LOG_FORMAT, "OnClick_UpdateMotionButton()");

        systemController.BPSPostRequest(3);
    }

    public void OnClick_DeleteMotionButton()
    {
        Debug.LogFormat(LOG_FORMAT, "OnClick_DeleteMotionButton()");

        systemController.BPSPostRequest(4);
    }

    BA_LineRenderer3D_Gizmo currentRenderer = null;
    public void OnValueChanged_GizmoMode(bool on)
    {
        Debug.LogFormat(LOG_FORMAT, "OnValueChanged_GizmoMode(), on : " + on);
        if (currentRenderer != null)
        {
            currentRenderer.GizmoMode = on;
            if (on == false)
            {
                SerializedDictionary<string, BA_LineRenderer3D_Gizmo> dic = BA_PathLineManager.Instance.LineRendererDic;
                // if (currentRenderer != null) // SAVE
                {
                    foreach (KeyValuePair<string, BA_LineRenderer3D_Gizmo> pair in dic)
                    {
                        BA_LineRenderer3D_Gizmo renderer = pair.Value;

                        if (currentRenderer == renderer)
                        {
                            // currentRenderer.PointList;
                            Debug.LogWarningFormat(LOG_FORMAT, "Please Save <b><color=red>" + pair.Key + "</color></b>");
                            if (pair.Key == "StiffBot01")
                            {
                                BA_PathDataManager.Instance.path_stiffnerBot = renderer.PointList;
                            }
                            else if (pair.Key == "StiffEdge01")
                            {
                                BA_PathDataManager.Instance.path_stiffnerEdge = renderer.PointList;
                            }
                            else if (pair.Key == "BilgeKeel01")
                            {
                                BA_PathDataManager.Instance.w1Path.PointList = renderer.PointList;
                            }
                            else if (pair.Key == "Curve01")
                            {
                                BA_PathDataManager.Instance.rPaths[1].PointList = renderer.PointList;
                            }
                            else if (pair.Key == "Curve02")
                            {
                                BA_PathDataManager.Instance.rPaths[2].PointList = renderer.PointList;
                            }
                            else if (pair.Key == "Curve03")
                            {
                                BA_PathDataManager.Instance.rPaths[3].PointList = renderer.PointList;
                            }
                            else if (pair.Key == "Curve04")
                            {
                                BA_PathDataManager.Instance.rPaths[4].PointList = renderer.PointList;
                            }
                            else if (pair.Key == "Curve05")
                            {
                                BA_PathDataManager.Instance.rPaths[5].PointList = renderer.PointList;
                            }
                            else if (pair.Key == "Plane01")
                            {
                                BA_PathDataManager.Instance.rPaths[6].PointList = renderer.PointList;
                            }
                            else if (pair.Key == "StiffSide01")
                            {
                                BA_PathDataManager.Instance.rPaths[7].PointList = renderer.PointList;
                            }
                            else if (pair.Key == "Plane02")
                            {
                                BA_PathDataManager.Instance.rPaths[8].PointList = renderer.PointList;
                            }
                            else if (pair.Key == "Plane03")
                            {
                                BA_PathDataManager.Instance.rPaths[9].PointList = renderer.PointList;
                            }
                            else if (pair.Key == "Plane04")
                            {
                                BA_PathDataManager.Instance.rPaths[10].PointList = renderer.PointList;
                            }
                        }
                    }
                }
            }
        }
    }

    public void OnValueChanged_LineRenderer(int index)
    {
        Debug.LogFormat(LOG_FORMAT, "OnValueChanged_GizmoMode(), index : " + index);

        gizmoToggle.SetIsOnWithoutNotify(false);

        SerializedDictionary<string, BA_LineRenderer3D_Gizmo> dic = BA_PathLineManager.Instance.LineRendererDic;
        if (currentRenderer != null) // SAVE
        {
            foreach (KeyValuePair<string, BA_LineRenderer3D_Gizmo> pair in dic)
            {
                BA_LineRenderer3D_Gizmo renderer = pair.Value;

                if (currentRenderer == renderer)
                {
                    // currentRenderer.PointList;
                    Debug.LogWarningFormat(LOG_FORMAT, "Please Save <b><color=red>" + pair.Key + "</color></b>");
                    if (pair.Key == "StiffBot01")
                    {
                        BA_PathDataManager.Instance.path_stiffnerBot = renderer.PointList;
                    }
                    else if (pair.Key == "StiffEdge01")
                    {
                        BA_PathDataManager.Instance.path_stiffnerEdge = renderer.PointList;
                    }
                    else if (pair.Key == "BilgeKeel01")
                    {
                        BA_PathDataManager.Instance.w1Path.PointList = renderer.PointList;
                    }
                    else if (pair.Key == "Curve01")
                    {
                        BA_PathDataManager.Instance.rPaths[1].PointList = renderer.PointList;
                    }
                    else if (pair.Key == "Curve02")
                    {
                        BA_PathDataManager.Instance.rPaths[2].PointList = renderer.PointList;
                    }
                    else if (pair.Key == "Curve03")
                    {
                        BA_PathDataManager.Instance.rPaths[3].PointList = renderer.PointList;
                    }
                    else if (pair.Key == "Curve04")
                    {
                        BA_PathDataManager.Instance.rPaths[4].PointList = renderer.PointList;
                    }
                    else if (pair.Key == "Curve05")
                    {
                        BA_PathDataManager.Instance.rPaths[5].PointList = renderer.PointList;
                    }
                    else if (pair.Key == "Plane01")
                    {
                        BA_PathDataManager.Instance.rPaths[6].PointList = renderer.PointList;
                    }
                    else if (pair.Key == "StiffSide01")
                    {
                        BA_PathDataManager.Instance.rPaths[7].PointList = renderer.PointList;
                    }
                    else if (pair.Key == "Plane02")
                    {
                        BA_PathDataManager.Instance.rPaths[8].PointList = renderer.PointList;
                    }
                    else if (pair.Key == "Plane03")
                    {
                        BA_PathDataManager.Instance.rPaths[9].PointList = renderer.PointList;
                    }
                    else if (pair.Key == "Plane04")
                    {
                        BA_PathDataManager.Instance.rPaths[10].PointList = renderer.PointList;
                    }
                }
            }
        }

        foreach (KeyValuePair<string, BA_LineRenderer3D_Gizmo> pair in dic)
        {
            BA_LineRenderer3D_Gizmo renderer = pair.Value;

            renderer.GizmoMode = false;
            renderer.gameObject.SetActive(false);
        }
        currentRenderer = null;

        if (index == 0)
        {
            //
        }
        else if (index == 1)
        {
            BA_PathLineManager.Instance.LineRendererDic["BilgeKeel01"].gameObject.SetActive(true);
            currentRenderer = BA_PathLineManager.Instance.LineRendererDic["BilgeKeel01"];
        }
        else if (index == 2)
        {
            BA_PathLineManager.Instance.LineRendererDic["Curve01"].gameObject.SetActive(true);
            currentRenderer = BA_PathLineManager.Instance.LineRendererDic["Curve01"];
        }
        else if (index == 3)
        {
            BA_PathLineManager.Instance.LineRendererDic["Curve02"].gameObject.SetActive(true);
            currentRenderer = BA_PathLineManager.Instance.LineRendererDic["Curve02"];
        }
        else if (index == 4)
        {
            BA_PathLineManager.Instance.LineRendererDic["Curve03"].gameObject.SetActive(true);
            currentRenderer = BA_PathLineManager.Instance.LineRendererDic["Curve03"];
        }
        else if (index == 5)
        {
            BA_PathLineManager.Instance.LineRendererDic["Curve04"].gameObject.SetActive(true);
            currentRenderer = BA_PathLineManager.Instance.LineRendererDic["Curve04"];
        }
        else if (index == 6)
        {
            BA_PathLineManager.Instance.LineRendererDic["Curve05"].gameObject.SetActive(true);
            currentRenderer = BA_PathLineManager.Instance.LineRendererDic["Curve05"];
        }
        else if (index == 7)
        {
            BA_PathLineManager.Instance.LineRendererDic["Plane01"].gameObject.SetActive(true);
            currentRenderer = BA_PathLineManager.Instance.LineRendererDic["Plane01"];
        }
        else if (index == 8)
        {
            BA_PathLineManager.Instance.LineRendererDic["StiffBot01"].gameObject.SetActive(true);
            currentRenderer = BA_PathLineManager.Instance.LineRendererDic["StiffBot01"];
        }
        else if (index == 9)
        {
            BA_PathLineManager.Instance.LineRendererDic["StiffEdge01"].gameObject.SetActive(true);
            currentRenderer = BA_PathLineManager.Instance.LineRendererDic["StiffEdge01"];
        }
        else if (index == 10)
        {
            BA_PathLineManager.Instance.LineRendererDic["StiffSide01"].gameObject.SetActive(true);
            currentRenderer = BA_PathLineManager.Instance.LineRendererDic["StiffSide01"];
        }
        else if (index == 11)
        {
            BA_PathLineManager.Instance.LineRendererDic["Plane02"].gameObject.SetActive(true);
            currentRenderer = BA_PathLineManager.Instance.LineRendererDic["Plane02"];
        }
        else if (index == 12)
        {
            BA_PathLineManager.Instance.LineRendererDic["Plane03"].gameObject.SetActive(true);
            currentRenderer = BA_PathLineManager.Instance.LineRendererDic["Plane03"];
        }
        else if (index == 13)
        {
            BA_PathLineManager.Instance.LineRendererDic["Plane04"].gameObject.SetActive(true);
            currentRenderer = BA_PathLineManager.Instance.LineRendererDic["Plane04"];
        }
        else
        {
            Debug.Assert(false);
        }

    }
}
