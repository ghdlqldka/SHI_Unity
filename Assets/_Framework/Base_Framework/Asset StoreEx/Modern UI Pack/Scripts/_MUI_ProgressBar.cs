using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using static _Base_Framework._MUI_Timer;

namespace Michsky.MUIP
{
    public class _MUI_ProgressBar : ProgressBar
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_ProgressBar]</b></color> {0}";

        protected virtual void Awake()
        {
#if DEBUG
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
#endif
        }

        protected virtual void OnEnable()
        {
            //
        }

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            // base.Start();
            UpdateUI();
            InitializeEvents();
        }

        protected override void Update()
        {
            // base.Update();
            if (isOn == false)
            {
                return;
            }

            if (currentPercent <= maxValue && !invert)
            { 
                currentPercent += speed * Time.unscaledDeltaTime;
            }
            else if (currentPercent >= minValue && invert) 
            { 
                currentPercent -= speed * Time.unscaledDeltaTime;
            }

            if (currentPercent >= maxValue && speed != 0 && restart && !invert) 
            { 
                currentPercent = 0;
            }
            else if (currentPercent <= minValue && speed != 0 && restart && invert) 
            { 
                currentPercent = maxValue;
            }
            else if (currentPercent >= maxValue && speed != 0 && !restart && !invert)
            {
                currentPercent = maxValue;
            }
            else if (currentPercent <= minValue && speed != 0 && !restart && invert) 
            {
                currentPercent = minValue;
            }

            UpdateUI();
        }
    }
}