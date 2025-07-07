using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Base_Framework
{
    [RequireComponent(typeof(Animator))]
    public class _AnimatorHandler : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#5E92FF><b>[_AnimatorHandler]</b></color> {0}";

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected Animator _animator;
        protected const int layerIndex = 0;

        protected AnimatorStateInfo prevStateInfo;
        protected AnimatorStateInfo currentStateInfo;

#if UNITY_EDITOR
#if DEBUG
        [Header("=====> For DEBUG <=====")]
        [ReadOnly]
        [SerializeField]
        protected string DEBUG_clipName;

        [Header("AnimatorStateInfo(DEBUG)")]
        [ReadOnly]
        [SerializeField]
        protected bool loop;
        [ReadOnly]
        [SerializeField]
        protected float length;
        [ReadOnly]
        [SerializeField]
        protected float speed;
        [ReadOnly]
        [SerializeField]
        protected float speedMultiplier;
#endif // DEBUG
#endif

        public delegate void FinishClip(AnimationClip clip);
        public event FinishClip OnStartClip;
        public event FinishClip OnFinishClip;
        protected void Invoke_OnStartClip(AnimationClip clip)
        {
            Debug.LogFormat(LOG_FORMAT, "Invoke_OnStartClip(), " + _animator.name + " - <b><color=red>" + clip.name + "</color></b> is <b>STARTED</b>!!!!!!!");
            if (OnStartClip != null)
            {
                OnStartClip(clip);
            }
        }

        protected void Invoke_OnFinishClip(AnimationClip clip)
        {
            Debug.LogFormat(LOG_FORMAT, "Invoke_OnFinishClip(), " + _animator.name + " - <b><color=red>" + clip.name + "</color></b> is <b>FINISHED</b>!!!!!!!");
            if (OnFinishClip != null)
            {
                OnFinishClip(clip);
            }
        }

        protected virtual void Awake()
        {
#if DEBUG
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
#endif

            if (_Base_Framework_Config.Product == _Base_Framework_Config._Product.Base_Framework)
            {
                if (_GlobalObjectUtilities.Instance == null)
                {
                    GameObject prefab = Resources.Load<GameObject>(_GlobalObjectUtilities.prefabPath);
                    Debug.Assert(prefab != null);
                    GameObject obj = Instantiate(prefab);
                    Debug.LogFormat(LOG_FORMAT, "<color=magenta>Instantiate \"<b>GlobalObjectUtilities</b>\"</color>");
                    obj.name = prefab.name;
                }
            }
            else
            {
                Debug.Assert(false);
            }

            _animator = this.GetComponent<Animator>();
            Debug.Assert(_animator != null);
        }

        protected virtual void Start()
        {
            currentStateInfo = _animator.GetCurrentAnimatorStateInfo(layerIndex);
            prevStateInfo = currentStateInfo;
            AnimatorClipInfo[] clipInfos = _animator.GetCurrentAnimatorClipInfo(layerIndex);
#if UNITY_EDITOR
#if DEBUG
            DEBUG_clipName = clipInfos[0].clip.name;
            loop = currentStateInfo.loop;
            length = currentStateInfo.length;
            speed = currentStateInfo.speed;
            speedMultiplier = currentStateInfo.speedMultiplier;
#endif
#endif

            Invoke_OnStartClip(clipInfos[0].clip);
            if (currentStateInfo.loop == false)
            {
                StartCoroutine(CheckEndOfAnimatorClip(clipInfos));
            }
            else
            {
                Debug.LogFormat(LOG_FORMAT, _animator.name + " - Uncheck end-of-animatorClip(" + clipInfos[0].clip.name + ") for \"<color=red><b>LOOP</b></color>\"");
            }
        }

        protected virtual void Update()
        {
            currentStateInfo = _animator.GetCurrentAnimatorStateInfo(layerIndex);
            if (prevStateInfo.fullPathHash != currentStateInfo.fullPathHash)
            {
                Debug.LogFormat(LOG_FORMAT, "" + _animator.name + " - <b>AnimatorStateInfo</b> has <b>Changed!!!!!</b>");

                prevStateInfo = currentStateInfo;
                AnimatorClipInfo[] clipInfos = _animator.GetCurrentAnimatorClipInfo(layerIndex);

                Invoke_OnStartClip(clipInfos[0].clip);

                if (currentStateInfo.loop == false)
                {
                    StartCoroutine(CheckEndOfAnimatorClip(clipInfos));
                }
                else
                {
                    Debug.LogFormat(LOG_FORMAT, _animator.name + " - Uncheck end-of-animatorClip(" + clipInfos[0].clip.name + ") for \"<color=red><b>LOOP</b></color>\"");
                }

#if UNITY_EDITOR
#if DEBUG
                DEBUG_clipName = clipInfos[0].clip.name;
                loop = currentStateInfo.loop;
                length = currentStateInfo.length;
                speed = currentStateInfo.speed;
                speedMultiplier = currentStateInfo.speedMultiplier;
#endif
#endif
            }
        }

        protected virtual IEnumerator CheckEndOfAnimatorClip(AnimatorClipInfo[] clipInfos)
        {
#if DEBUG
            Debug.LogFormat(LOG_FORMAT, "Check<color=red><b>EndOfAnimatorClip</b></color>(), clipName : <b><color=red>" + clipInfos[0].clip.name + "</color></b>");
#endif

            AnimatorStateInfo animatorStateInfo = _animator.GetCurrentAnimatorStateInfo(layerIndex);
            while (animatorStateInfo.normalizedTime < 1.0f)
            {
                yield return null;
                animatorStateInfo = _animator.GetCurrentAnimatorStateInfo(layerIndex);
            }

            // clipInfos = _animator.GetCurrentAnimatorClipInfo(layerIndex);
            Invoke_OnFinishClip(clipInfos[0].clip);
        }

        public virtual void Play()
        {
            _animator.speed = 1f;
        }

        public virtual void Stop()
        {
            _animator.speed = 0f;
        }

        public virtual bool GetBool(string parameter)
        {
            return _animator.GetBool(parameter);
        }

        public virtual void SetBool(string parameter, bool value)
        {
            _animator.SetBool(parameter, value);
        }

        public virtual void SetTrigger(string trigger)
        {
            Debug.LogFormat(LOG_FORMAT, "SetTrigger(), trigger : " + trigger);
            _animator.SetTrigger(trigger);
        }

#if DEBUG
        protected GUIStyle _textStyle;
        
        protected virtual void OnGUI()
        {
            if (_textStyle == null)
            {
                _textStyle = new GUIStyle(GUI.skin.label);
                _textStyle.fontSize = 30;
                _textStyle.fontStyle = FontStyle.Bold;
            }

            float x = Screen.width / 2 + 100;
            float y = 10;

            AnimatorStateInfo animatorStateInfo = currentStateInfo;

            string debugStr = "_animator : <color=red>" + _animator.name + "</color>";
            debugStr += ("\nanimator.<b>layerCount : <color=red>" + _animator.layerCount + "</color></b>");
            debugStr += ("\n\n_animator.parameters.<b><color=red>Length : " + _animator.parameters.Length + "</color></b>");

            // debugStr += ("\n\nanimatorStateInfo.<b>fullPathHash : <color=red>" + animatorStateInfo.fullPathHash + "</color></b>");
            // debugStr += ("\nanimatorStateInfo.state : <b><color=red>" + animatorStateInfo.shortNameHash + "</color></b>");

            // The fractional part is the % (0-1) of progress in the current loop
            debugStr += ("\nanimatorStateInfo.<b><color=red>normalizedTime : " + animatorStateInfo.normalizedTime + "</color></b>");
            // debugStr += ("\nanimatorStateInfo.<b><color=red>tagHash : " + animatorStateInfo.tagHash + "</color></b>");
            // debugStr += ("\nAR Session.<b><color=red>frameRate : " + animatorStateInfo.IsName + "</color></b>");
            //When entering the Jump state in the Animator, output the message in the console

            string stateName = "JUMP00";
            if (animatorStateInfo.IsName(stateName))
            {
                debugStr += ("\nanimatorStateInfo.<b><color=red>IsName(\"" + stateName + "\")</color></b>");
            }

            GUI.Label(new Rect(x, y, Screen.width / 2, 800), debugStr, _textStyle);
        }
#endif
    }
}