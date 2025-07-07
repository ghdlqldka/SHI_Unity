using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace _Base_Framework
{
    public class _DinosaursHandler : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=magenta><b>[_DinosaursHandler]</b></color> {0}";

        [Space(10)]
#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected _AnimatorHandler _animator;
        public enum ClipType
        {
            attack_01,
            attack_02,
            butting_heads,
            death,
            ducking,
            idle_01,
            idle_02,
            idle_03,
            idle_eat,
            injured,
            no,
            roar,
            run,
            run_fast,
            sleep,
            stand_pose,
            surprised,
            turn_left,
            turn_right,
            walk,
            walk_back,
            yes,
        }

        public enum AnimStatus
        {
            None, // stand_pose

            Attack, // attack_01, attack_02, butting_heads
            Death, // death
            Ducking, // ducking
            Idle, // idle_01, idle_02, idle_03
            Eat, // idle_eat
            Injured, // injured

            Yes, // yes
            No, // no
            Roar, // roar

            Run, // run
            RunFast, // run_fast
            Walk, // walk
            WalkBack, // walk_back

            Sleep, // sleep
            StandPose, // stand_pose
            Surprised, // surprised
            TurnLeft, // turn_left
            TurnRight, // turn_right
        }
        [ReadOnly]
        [SerializeField]
        protected AnimStatus _status;
        public AnimStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    Debug.LogWarningFormat(LOG_FORMAT, "Status : " + value);
                    if (_status == AnimStatus.None)
                    {
                        StandPose();
                    }
                    else if (_status == AnimStatus.Attack)
                    {
                        Attack();
                    }
                    else if (value == AnimStatus.Death)
                    {
                        Death();
                    }
                    else if (value == AnimStatus.Ducking)
                    {
                        Death();
                    }
                    else if (value == AnimStatus.Idle)
                    {
                        Idle();
                    }
                    else if (value == AnimStatus.Eat)
                    {
                        Eat();
                    }
                    else if (value == AnimStatus.Injured)
                    {
                        Injured();
                    }
                    else if (value == AnimStatus.Yes)
                    {
                        Yes();
                    }
                    else if (value == AnimStatus.No)
                    {
                        No();
                    }
                    else if (value == AnimStatus.Roar)
                    {
                        Roar();
                    }
                    else if (value == AnimStatus.Run)
                    {
                        Run();
                    }
                    else if (value == AnimStatus.RunFast)
                    {
                        RunFast();
                    }
                    else if (value == AnimStatus.Walk)
                    {
                        Walk();
                    }
                    else if (value == AnimStatus.WalkBack)
                    {
                        WalkBack();
                    }
                    else if (value == AnimStatus.Sleep)
                    {
                        Sleep();
                    }
                    else if (value == AnimStatus.Surprised)
                    {
                        Surprised();
                    }
                    else if (value == AnimStatus.TurnLeft)
                    {
                        TurnLeft();
                    }
                    else if (value == AnimStatus.TurnRight)
                    {
                        TurnRight();
                    }
                }
            }
        }

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            Debug.Assert(_animator != null);
            _animator.OnStartClip += OnStartClip;
            _animator.OnFinishClip += OnFinishClip;
        }

        protected virtual void OnDestroy()
        {
            _animator.OnFinishClip -= OnFinishClip;
            _animator.OnStartClip -= OnStartClip;
        }

        protected virtual void OnStartClip(AnimationClip clip)
        {
            // throw new System.NotImplementedException();
            // Debug.LogFormat(LOG_FORMAT, "<color=red><b>OnFinishClip</b></color>(), clipName : <b><color=red>" + clip.name + "</color></b>");
        }

        protected virtual void OnFinishClip(AnimationClip clip)
        {
            ClipType clipType = (ClipType)System.Enum.Parse(typeof(ClipType), clip.name);
            // throw new System.NotImplementedException();
            Debug.LogFormat(LOG_FORMAT, "<color=red><b>OnFinishClip</b></color>(), clipName : <b><color=red>" + clip.name + "</color></b>");
            if (clipType == ClipType.stand_pose)
            {
                Status = AnimStatus.Idle;
            }
            else if (clipType == ClipType.attack_01 || clipType == ClipType.attack_02 || 
                clipType == ClipType.butting_heads) // Attack -> Idle
            {
                Status = AnimStatus.Idle;
            }
            else if (clipType == ClipType.ducking) // Ducking -> Idle
            {
                Status = AnimStatus.Idle;
            }
            else if (clipType == ClipType.idle_eat) // Eat -> Idle
            {
                Status = AnimStatus.Idle;
            }
            else if (clipType == ClipType.injured) // Injured -> Idle
            {
                Status = AnimStatus.Death;
            }
            else if (clipType == ClipType.yes || clipType == ClipType.no) // Yes, No -> Idle
            {
                Status = AnimStatus.Idle;
            }
            else if (clipType == ClipType.roar) // Roar -> Idle
            {
                Status = AnimStatus.Idle;
            }
        }

        protected virtual void OnDisable()
        {
            _FxManager.Instance.Stop();
        }

        protected virtual void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                Status = AnimStatus.None;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                Status = AnimStatus.Attack;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                Status = AnimStatus.Eat;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                Status = AnimStatus.Injured;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                Status = AnimStatus.Yes;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                Status = AnimStatus.No;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                Status = AnimStatus.Roar;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                Status = AnimStatus.Sleep;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                Status = AnimStatus.Surprised;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                Status = AnimStatus.TurnLeft;
            }
            else if (Input.GetKeyDown(KeyCode.KeypadMultiply))
            {
                Status = AnimStatus.TurnRight;
            }
            else if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                if (Status == AnimStatus.WalkBack)
                {
                    Status = AnimStatus.Idle;
                }
                else if (Status == AnimStatus.Idle)
                {
                    Status = AnimStatus.Walk;
                }
                else if (Status == AnimStatus.Walk)
                {
                    Status = AnimStatus.Run;
                }
                else if (Status == AnimStatus.Run)
                {
                    Status = AnimStatus.RunFast;
                }
            }
            else if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                if (Status == AnimStatus.Idle)
                {
                    Status = AnimStatus.WalkBack;
                }
                else if (Status == AnimStatus.Walk)
                {
                    Status = AnimStatus.Idle;
                }
                else if (Status == AnimStatus.Run)
                {
                    Status = AnimStatus.Walk;
                }
                else if (Status == AnimStatus.RunFast)
                {
                    Status = AnimStatus.Run;
                }
            }

#endif
        }

        public virtual void _Reset()
        {
            //
        }

        public virtual void PlayAnimation(ClipType clipType)
        {
            _animator.SetTrigger(clipType.ToString());
        }

        /*
        public virtual bool GetAnimatorTrigger(ClipType animTrigger)
        {
            return _animator.GetBool(animTrigger.ToString());
        }
        */

        protected virtual void StandPose()
        {
            PlayAnimation(ClipType.stand_pose);
        }

        protected virtual void Attack()
        {
            int minInclusive = (int)ClipType.attack_01;
            int maxExclusive = (int)ClipType.butting_heads + 1;
            int index = Random.Range(minInclusive, maxExclusive);

            PlayAnimation((ClipType)index);
        }

        protected virtual void Death()
        {
            PlayAnimation(ClipType.death);
        }

        protected virtual void Ducking()
        {
            PlayAnimation(ClipType.ducking);
        }

        protected virtual void Idle()
        {
            int minInclusive = (int)ClipType.idle_01;
            int maxExclusive = (int)ClipType.idle_03 + 1;
            int index = Random.Range(minInclusive, maxExclusive);

            PlayAnimation((ClipType)index);
        }

        protected virtual void Eat()
        {
            PlayAnimation(ClipType.idle_eat);
        }

        protected virtual void Injured()
        {
            int minInclusive = 0;
            int maxExclusive = 2;
            int index = Random.Range(minInclusive, maxExclusive);

            if (index == 0)
            {
                PlayAnimation(ClipType.injured);
            }
            else
            {
                PlayAnimation(ClipType.ducking);
            }
        }

        protected virtual void Yes()
        {
            PlayAnimation(ClipType.yes);
        }

        protected virtual void No()
        {
            PlayAnimation(ClipType.no);
        }

        protected virtual void Roar()
        {
            PlayAnimation(ClipType.roar);
        }

        protected virtual void Run()
        {
            PlayAnimation(ClipType.run);
        }
        protected virtual void RunFast()
        {
            PlayAnimation(ClipType.run_fast);
        }

        protected virtual void Walk()
        {
            PlayAnimation(ClipType.walk);
        }

        protected virtual void WalkBack()
        {
            PlayAnimation(ClipType.walk_back);
        }

        protected virtual void Sleep()
        {
            PlayAnimation(ClipType.sleep);
        }

        protected virtual void Surprised()
        {
            PlayAnimation(ClipType.surprised);
        }

        protected virtual void TurnLeft()
        {
            PlayAnimation(ClipType.turn_left);
        }

        protected virtual void TurnRight()
        {
            PlayAnimation(ClipType.turn_right);
        }
    }
}