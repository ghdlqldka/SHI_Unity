using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

namespace _Magenta_Framework
{
    [RequireComponent(typeof(Slider))]
    public class MUI_SliderEx : Michsky.MUIP._MUI_Slider
    {
        // private static string LOG_FORMAT = "<color=#EFC6FB><b>[MUI_SliderEx]</b></color> {0}";

        protected override void Awake()
        {
            // Debug.LogFormat(LOG_FORMAT, "Awake(), enableSaving : " + enableSaving);

            if (enableSaving == true)
            {
                Key_string_SliderValue = "Key_string__" + sliderTag + "_MUIPSliderValue";
                if (PlayerPrefs.HasKey(Key_string_SliderValue) == false) 
                { 
                    saveValue = mainSlider.value;
                }
                else 
                { 
                    saveValue = PlayerPrefs.GetFloat(Key_string_SliderValue);
                }

                mainSlider.value = saveValue;
                mainSlider.onValueChanged.AddListener(delegate
                {
                    saveValue = mainSlider.value;
                    PlayerPrefs.SetFloat(Key_string_SliderValue, saveValue);
                });
            }

            mainSlider.onValueChanged.AddListener(delegate
            {
                sliderEvent.Invoke(mainSlider.value);
                UpdateUI();
            });

            if (sliderAnimator == null && showPopupValue == true)
            {
                try 
                { 
                    sliderAnimator = gameObject.GetComponent<Animator>();
                }
                catch 
                { 
                    showPopupValue = false;
                }
            }

            if (invokeOnAwake == true) 
            { 
                sliderEvent.Invoke(mainSlider.value);
            }
            UpdateUI();
        }

    }
}