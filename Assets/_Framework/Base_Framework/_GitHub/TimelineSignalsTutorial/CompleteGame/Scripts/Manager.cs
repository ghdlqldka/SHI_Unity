using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Manager : MonoBehaviour
{
    public PlayableDirector gameDirector;
    public PlayableDirector deathDirector;
    public PlayableDirector successTimeline;
    public Text keyUI;

    static Tuple<KeyCode, string>[] keys =
    {
        Tuple.Create(KeyCode.UpArrow, "↑"),
        Tuple.Create(KeyCode.DownArrow, "↓"),
        Tuple.Create(KeyCode.LeftArrow, "←"),
        Tuple.Create(KeyCode.RightArrow, "→")
    };

    protected bool isDisplayingKey;
    protected KeyCode currentKey;

    public virtual void ShowRandomKey()
    {
        if (keyUI == null)
            return;

        if (!isDisplayingKey)
        {
            ShowKeyObject(true);
            DisplayRandomKey();
            isDisplayingKey = true;
        }
        else //player lost
        {
            ShowKeyObject(false);
            gameDirector.Stop();
            deathDirector.Play();
        }
    }

    public virtual void BeginGame()
    {
        if (keyUI != null)
            ShowKeyObject(false);

        isDisplayingKey = false;
    }

    protected virtual void Update()
    {
        if (isDisplayingKey)
        {
            if (Input.GetKeyDown(currentKey))
            {
                ShowKeyObject(false);
                isDisplayingKey = false;
                PlayTimeline(successTimeline);
            }
        }
    }

    protected static void PlayTimeline(PlayableDirector director)
    {
        director.Stop();
        director.time = 0;
        director.Play();
    }

    protected void ShowKeyObject(bool show)
    {
        var parent = keyUI.transform.parent.gameObject;
        parent.SetActive(show);
    }

    protected void DisplayRandomKey()
    {
        var keyIndex = Random.Range(0, keys.Length - 1);
        var (keyCode, keyString) = keys[keyIndex];
        currentKey = keyCode;
        keyUI.text = keyString;
    }
}
