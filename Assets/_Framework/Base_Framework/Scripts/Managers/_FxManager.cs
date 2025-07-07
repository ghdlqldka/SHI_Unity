using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Base_Framework
{
    public class _FxManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#A4D298><b>[_FxManager]</b></color> {0}";

        protected static _FxManager _instance;
        public static _FxManager Instance
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

        [ReadOnly]
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
                Debug.Assert(audioSource.loop == false);
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

        public virtual void PlayOneShot(AudioClip clip, float volumeScale = 1.0f)
        {
            Debug.Assert(clip != null);
            audioSource.PlayOneShot(clip, volumeScale);
        }

        public virtual void Play(AudioClip clip)
        {
            Debug.Assert(clip != null);
            if (audioSource.isPlaying == true)
            {
                audioSource.Stop();
            }
            audioSource.volume = 1;
            audioSource.clip = clip;
            audioSource.Play();
        }

        public virtual void Play(AudioClip clip, float volumeScale, ulong delay = 0)
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

        public virtual void Pause(bool pause)
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

        public virtual void Stop()
        {
            if (audioSource.isPlaying == true)
            {
                //
            }
            audioSource.Stop();
        }

        public bool IsPlaying()
        {
            return audioSource.isPlaying;
        }
    }
}