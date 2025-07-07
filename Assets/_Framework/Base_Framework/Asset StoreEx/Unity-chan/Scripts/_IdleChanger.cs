using UnityEngine;
using System.Collections;

namespace UnityChan
{
	[RequireComponent(typeof(Animator))]

	public class _IdleChanger : IdleChanger
    {
        private static string LOG_FORMAT = "<color=#00FF72><b>[_IdleChanger]</b></color> {0}";

        [ReadOnly]
        [SerializeField]
        protected Animator _animator;

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            anim = null; // Not used!!!!!
        }

        protected override void Start()
        {
            _animator = GetComponent<Animator>();
            currentState = _animator.GetCurrentAnimatorStateInfo(0);
            previousState = currentState;

            StartCoroutine(RandomChange());
        }

        protected override void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetButton("Jump"))
            {
                _animator.SetBool("Next", true);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _animator.SetBool("Back", true);
            }

            if (_animator.GetBool("Next") == true)
            {
                currentState = _animator.GetCurrentAnimatorStateInfo(0);
#pragma warning disable 0618
                if (previousState.nameHash != currentState.nameHash)
#pragma warning restore 0618
                {
                    _animator.SetBool("Next", false);
                    previousState = currentState;
                }
            }

            if (_animator.GetBool("Back") == true)
            {
                currentState = _animator.GetCurrentAnimatorStateInfo(0);
#pragma warning disable 0618
                if (previousState.nameHash != currentState.nameHash)
#pragma warning restore 0618
                {
                    _animator.SetBool("Back", false);
                    previousState = currentState;
                }
            }
        }

        protected override IEnumerator RandomChange()
        {
            while (true)
            {
                if (_random == true)
                {
                    float _seed = Random.Range(0.0f, 1.0f);
                    if (_seed < _threshold)
                    {
                        _animator.SetBool("Back", true);
                    }
                    else if (_seed >= _threshold)
                    {
                        _animator.SetBool("Next", true);
                    }
                }

                yield return new WaitForSeconds(_interval);
            }
        }

        protected override void OnGUI()
        {
            GUI.Box(new Rect(Screen.width - 110, 10, 100, 90), "Change Motion");

            if (GUI.Button(new Rect(Screen.width - 100, 40, 80, 20), "Next"))
                _animator.SetBool("Next", true);
            if (GUI.Button(new Rect(Screen.width - 100, 70, 80, 20), "Back"))
                _animator.SetBool("Back", true);
        }
    }
}
