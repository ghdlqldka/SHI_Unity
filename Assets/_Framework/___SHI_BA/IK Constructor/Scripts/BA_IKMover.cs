using UnityEngine;

[ExecuteInEditMode]
public class BA_IKMover : _IKMover
{
    private static string LOG_FORMAT = "<color=#00FF1C><b>[BA_IKMover]</b></color> {0}";

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
        // Update the object automatically if set
        if (AutoUpdate == true)
            ManualUpdate();

        // Draws a debug visual if set
        if (DrawVisual == true)
            DrawDebug();
    }
}
