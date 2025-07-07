using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

namespace Michsky.MUIP
{
    [RequireComponent(typeof(Slider))]
    public class _MUI_Slider : SliderManager
    {
        // private static string LOG_FORMAT = "<color=#EFC6FB><b>[_MUI_Slider]</b></color> {0}";

        protected string Key_string_SliderValue = "Key_string__MUIPSliderValue";

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

        public override void UpdateUI()
        {
            if (useRoundValue == true)
            {
                if (usePercent == true)
                {
                    if (valueText != null)
                    { 
                        valueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString() + "%"; 
                    }
                    if (popupValueText != null)
                    { 
                        popupValueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString() + "%"; 
                    }
                }

                else
                {
                    if (valueText != null) 
                    { 
                        valueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString();
                    }
                    if (popupValueText != null) 
                    { 
                        popupValueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString();
                    }
                }
            }

            else
            {
                if (usePercent == true)
                {
                    if (valueText != null)
                    { 
                        valueText.text = mainSlider.value.ToString(decimalFormat.ToString()) + "%";
                    }
                    if (popupValueText != null) 
                    { 
                        popupValueText.text = mainSlider.value.ToString(decimalFormat.ToString()) + "%";
                    }
                }

                else
                {
                    if (valueText != null) 
                    { 
                        valueText.text = mainSlider.value.ToString(decimalFormat.ToString());
                    }
                    if (popupValueText != null) 
                    { 
                        popupValueText.text = mainSlider.value.ToString(decimalFormat.ToString());
                    }
                }
            }
        }
    }
}