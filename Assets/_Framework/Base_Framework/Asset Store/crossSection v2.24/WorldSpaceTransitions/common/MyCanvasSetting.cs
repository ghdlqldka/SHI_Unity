using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MyCanvasSetting : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
#if UNITY_ANDROID      
        mySetting();
#endif
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
#if UNITY_ANDROID      
        mySetting();
#endif
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void mySetting()
    {
#pragma warning disable 0618
        CanvasScaler[] canv = FindObjectsOfType(typeof(CanvasScaler)) as CanvasScaler[];
#pragma warning restore 0618
        foreach (CanvasScaler cns in canv) cns.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    }

}
