using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Base_Framework
{
    public class _BGMManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#CF9F19><b>[_BGMManager]</b></color> {0}";

        protected static _BGMManager _instance;
        public static _BGMManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected AudioSource audioSource;
        public AudioSource _AudioSource
        {
            get
            {
                return audioSource;
            }
        }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "Awake()");

                Instance = this;

                Debug.Assert(audioSource != null);
                Debug.Assert(audioSource.playOnAwake == false);
                Debug.Assert(audioSource.loop == true);
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        protected virtual void OnEnable()
        {
            //
        }

        public void Play(AudioClip clip, ulong delay = 0)
        {
            Debug.LogFormat(LOG_FORMAT, "Play(), clip : <color=yellow>" + clip.name + "</color>");

            Debug.Assert(clip != null);
            if (audioSource.isPlaying == true)
            {
                audioSource.Stop();
            }
            audioSource.volume = 1;
            audioSource.clip = clip;
            audioSource.Play(delay);
        }

        public void Player(AudioClip clip, float volumeScale, ulong delay = 0)
        {
            Debug.Assert(clip != null);
            if (audioSource.isPlaying == true)
            {
                audioSource.Stop();
            }
            audioSource.volume = volumeScale;
            audioSource.clip = clip;
            audioSource.Play(delay);
        }

        public void Pause(bool pause)
        {
            if (pause == true)
            {
                audioSource.Pause();
            }
            else
            {
                audioSource.UnPause();
            }
        }

        public void Stop()
        {
            if (audioSource.isPlaying == true)
            {
                //
            }
            audioSource.Stop();
        }
    }
}