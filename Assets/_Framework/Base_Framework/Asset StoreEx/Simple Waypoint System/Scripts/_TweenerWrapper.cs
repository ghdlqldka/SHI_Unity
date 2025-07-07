using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

[System.Serializable]
public class _TweenerWrapper
{
    public Tweener tween;

    [Header("=====> DEBUG <=====")]
    [ReadOnly]
    [SerializeField]
    protected Tweener DEBUG_tween;

    public void Play()
    {
        tween.Play();
    }

    public void Pause()
    {
        tween.Pause();
    }

    public void Kill(bool complete = false)
    {
        tween.Kill();
    }

    public bool IsActive()
    {
        return tween.IsActive();
    }

    public bool IsPlaying()
    {
        return tween.IsPlaying();
    }

    public float Duration()
    {
        return tween.Duration();
    }

    public bool IsComplete()
    {
        return tween.IsComplete();
    }

    public bool IsInitialized()
    {
        return tween.IsInitialized();
    }

    public void ForceInit()
    {
        tween.ForceInit();
    }

    public float ElapsedPercentage(bool includeLoops)
    {
        return tween.ElapsedPercentage(includeLoops);
    }

    public void GotoWaypoint(int waypointIndex, bool andPlay)
    {
        tween.GotoWaypoint(waypointIndex, true);
    }
}
