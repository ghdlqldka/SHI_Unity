using System;
using UnityEngine;
using UnityEngine.Playables;

namespace _Base_Framework
{
    public class _TimelineHandler : Manager
    {
        private static string LOG_FORMAT = "<color=#00AEEF><b>[Test_TimelineHandlerEx]</b></color> {0}";

        protected virtual void Awake()
        {
            gameDirector.played += OnPlayableDirector_played;
            gameDirector.paused += OnPlayableDirector_paused;
            gameDirector.stopped += OnPlayableDirector_stopped;

            deathDirector.played += OnPlayableDirector_played;
            deathDirector.paused += OnPlayableDirector_paused;
            deathDirector.stopped += OnPlayableDirector_stopped;

            successTimeline.played += OnPlayableDirector_played;
            successTimeline.paused += OnPlayableDirector_paused;
            successTimeline.stopped += OnPlayableDirector_stopped;
        }

        protected virtual void OnDestroy()
        {
            gameDirector.played -= OnPlayableDirector_played;
            gameDirector.paused -= OnPlayableDirector_paused;
            gameDirector.stopped -= OnPlayableDirector_stopped;

            deathDirector.played -= OnPlayableDirector_played;
            deathDirector.paused -= OnPlayableDirector_paused;
            deathDirector.stopped -= OnPlayableDirector_stopped;

            successTimeline.played -= OnPlayableDirector_played;
            successTimeline.paused -= OnPlayableDirector_paused;
            successTimeline.stopped -= OnPlayableDirector_stopped;
        }

        protected virtual void OnPlayableDirector_played(PlayableDirector playableDirector)
        {
            // throw new NotImplementedException();
            Debug.LogWarningFormat(LOG_FORMAT, "OnPlayableDirector_<b><color=red>played</color></b>(), playableDirector.name : <b><color=yellow>" + playableDirector.name + "</color></b>");
        }

        protected virtual void OnPlayableDirector_paused(PlayableDirector playableDirector)
        {
            // throw new NotImplementedException();
            Debug.LogWarningFormat(LOG_FORMAT, "OnPlayableDirector_<b><color=red>paused</color></b>(), playableDirector.name : <b><color=yellow>" + playableDirector.name + "</color></b>");
        }

        protected virtual void OnPlayableDirector_stopped(PlayableDirector playableDirector)
        {
            // throw new NotImplementedException();
            Debug.LogWarningFormat(LOG_FORMAT, "OnPlayableDirector_<b><color=red>stopped</color></b>(), playableDirector.name : <b><color=yellow>" + playableDirector.name + "</color></b>");
        }

        protected override void Update()
        {
            if (isDisplayingKey == true)
            {
                if (Input.GetKeyDown(currentKey))
                {
                    ShowKeyObject(false);
                    isDisplayingKey = false;
                    PlayTimeline(successTimeline);
                }
            }
        }

        public override void BeginGame()
        {
            throw new System.NotSupportedException("");
        }

        public virtual void OnBeginGame()
        {
            Debug.LogFormat(LOG_FORMAT, "OnBeginGame()");

            if (keyUI != null)
            {
                ShowKeyObject(false);
            }
            isDisplayingKey = false;
        }

        public override void ShowRandomKey()
        {
            throw new System.NotSupportedException("");
        }

        public virtual void OnShowRandomKey()
        {
            Debug.LogFormat(LOG_FORMAT, "OnShowRandomKey()");

            if (keyUI == null)
            {
                return;
            }

            if (isDisplayingKey == false)
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
    }
}