using UnityEngine;

[ExecuteInEditMode]
public class _IKAxis : IKAxis
{
    private static string LOG_FORMAT = "<color=#FFEB04><b>[_IKAxis]</b></color> {0}";

    protected override void Start()
    {
        Debug.LogFormat(LOG_FORMAT, "Start(), this.gameObject.name : <b>" + this.gameObject.name + "</b>");

        if (Application.isPlaying == false)
            return;

        if (IsInit == false)
            Init();
    }

    protected override void LateUpdate()
    {
        // Updates Axis automatically is set
        if (AutoUpdate == true)
            ManualUpdate();

        // Draw a debug visual if set
        if (DrawVisual == true)
            DrawDebug();
    }
}
