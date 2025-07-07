using _SHI_BA;
using BioIK;
using UnityEngine;
using UnityEngine.UI;

public class UI_TopLeftPanel : MonoBehaviour
{
    private static string LOG_FORMAT = "<color=white><b>[UI_TopLeftPanel]</b></color> {0}";

    [SerializeField]
    protected SimpleBlockShow simpleBlockShow;
    [SerializeField]
    protected CubeStruct cubeStruct;
    [SerializeField]
    protected SystemController systemController;

    [SerializeField]
    protected GameObject Panel_CubeCustom;

    protected virtual void Awake()
    {
        Debug.LogFormat(LOG_FORMAT, "Awake()");

        Debug.Assert(simpleBlockShow != null);
        Debug.Assert(cubeStruct != null);
        Debug.Assert(systemController != null);
    }

    public void OnClick_LoadCAD()
    {
        simpleBlockShow.ShowBlock();
    }

    public void OnClick_RemoveCAD()
    {
        simpleBlockShow.HideBlock();
    }

    public void OnClick_CreateWorkspace()
    {
        cubeStruct.CreateMeshBox();
    }

    public void OnClick_CreateCustomWorkspace()
    {
        // cubeStruct.ShowCustomCubePanel();
        Panel_CubeCustom.SetActive(true);
    }

    public void OnClick_RemoveWorkspace()
    {
        cubeStruct.RemoveMeshBox();
    }

    public void OnClick_BlastingParticle()
    {
        systemController.ChangeToWorking();
    }
}
