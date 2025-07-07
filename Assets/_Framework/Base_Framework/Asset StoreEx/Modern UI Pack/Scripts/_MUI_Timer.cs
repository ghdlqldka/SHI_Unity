using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.MUIP;

namespace _Base_Framework
{
    public class _MUI_Timer : _MUI_ProgressBar
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_Timer]</b></color> {0}";

        // public float maxValue = 100;
        public float TimerTime
        {
            get
            {
                return maxValue;
            }
            set
            {
                maxValue = value;
                _Reset();
                IsPlay = false;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected float remaingTime;

        [SerializeField]
        protected bool _isPlay = false; // play, pause, resume
        public bool IsPlay
        {
            get
            {
                return _isPlay;
            }
            set
            {
                if (_isPlay != value)
                {
                    _isPlay = value;
                }
            }
        }

        public delegate void ReachEnd();
        public event ReachEnd OnReachEnd;

        protected override void Awake()
        {
#if DEBUG
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : " + this.gameObject.name);
#endif

            Debug.Assert(maxValue > 0);
            currentPercent = 0; // Don't use this variable!!!!!
            speed = 0; // Don't use this variable!!!!!

            isOn = true;  // Don't use this variable!!!!!
            restart = true; // Not used
            invert = false; // Not used
        }

        protected override void OnEnable()
        {
            //
        }

        protected override void Start()
        {
            // base.Start();

            Debug.LogFormat(LOG_FORMAT, "Start(), IsPlay : " + IsPlay);

            // UpdateUI();
            // InitializeEvents();
#if true
            _Reset();
#else
            remaingTime = maxValue;
            // if (isOn == false)
            {
                loadingBar.fillAmount = remaingTime / maxValue;
                textPercent.text = ((int)remaingTime).ToString("F0") + "Sec";
            }
#endif
        }

        protected override void Update()
        {
            if (IsPlay == true)
            {
                if (remaingTime > 0)
                {
                    remaingTime -= Time.deltaTime;

                    if (remaingTime <= 0)
                    {
                        remaingTime = 0;
                        IsPlay = false;
                        Debug.LogFormat(LOG_FORMAT, "<b><color=red>Reach End</color></b>!!!!!");
                        // _Reset(); // test
                        if (OnReachEnd != null)
                        {
                            OnReachEnd();
                        }
                    }

                    loadingBar.fillAmount = remaingTime / maxValue;
                    textPercent.text = ((int)remaingTime).ToString("F0") + "Sec";
                }
            }
        }

        public virtual void _Reset()
        {
            remaingTime = maxValue;

            loadingBar.fillAmount = remaingTime / maxValue;
            textPercent.text = ((int)remaingTime).ToString("F0") + "Sec";
        }

        public override void UpdateUI()
        {
            // base.UpdateUI();
            throw new System.NotSupportedException("");
        }
    }
}