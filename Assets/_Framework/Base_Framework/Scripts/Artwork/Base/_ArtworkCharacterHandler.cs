using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace _Base_Framework
{
    public class _ArtworkCharacterHandler : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=magenta><b>[_ArtworkCharacterHandler]</b></color> {0}";

        [Space(10)]
#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected _AnimatorHandler _animator;
        public enum __AnimatorTrigger
        {
            _1_appear,
            _1_diorama_talk01,
            _1_talk01,
            _1_talk02,
            _1_talk03,
            _1_talk04,
            _1_talk05,
            _2_come_and_see,
            _2_appear,
            _2_move,
            _2_talk01,
            _2_talk02,
            _2_talk03,
            _2_talk04,
            _3_talk01,
            _3_Where_are_you_going,
            _4_Questions,

            idle,
        }

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected bool isTalking = false;
        public bool IsTalking
        {
            get
            {
                return isTalking;
            }
        }

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            Debug.Assert(_animator != null);
        }

        protected virtual void OnDisable()
        {
            _FxManager.Instance.Stop();
            isTalking = false;
        }

        public virtual void _Reset()
        {
            //
        }

        public virtual void Talk(AudioClip clip, UnityAction finishAction = null)
        {
            Debug.Assert(isTalking == false);
            Debug.LogFormat(LOG_FORMAT, "Talk(), clip : <b>" + clip + "</b>, finishAction : " + finishAction);

            StartCoroutine(StartTalk(clip, finishAction));
        }

        protected virtual IEnumerator StartTalk(AudioClip clip, UnityAction finishAction = null)
        {
            isTalking = true;

            _FxManager.Instance.PlayOneShot(clip);

            yield return new WaitForSeconds(clip.length);

            if (finishAction != null)
            {
                finishAction.Invoke();
            }

            isTalking = false;
        }

        public virtual void Talk(AudioClip[] clips, UnityAction<int> finishAction = null)
        {
            Debug.Assert(isTalking == false);
            StartCoroutine(StartTalk(clips, finishAction));
        }

        protected virtual IEnumerator StartTalk(AudioClip[] clips, UnityAction<int> finishAction = null)
        {
            isTalking = true;
            int index = 0;

            while (index < clips.Length)
            {
                _FxManager.Instance.PlayOneShot(clips[index]);

                yield return new WaitForSeconds(clips[index].length);

                if (finishAction != null)
                {
                    finishAction.Invoke(index);
                }

                index++;
            }

            isTalking = false;
        }

        public virtual void SetAnimatorTrigger(__AnimatorTrigger animTrigger)
        {
            _animator.SetTrigger(animTrigger.ToString());
        }
    }
}