using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Michsky.MUIP
{
    public class _MUI_TabControl : WindowManager
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_TabControl]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + 
                "</b>, windows.Count : <b>" + windows.Count + "</b>");

            Debug.Assert(windows.Count > 0);
            /*
            if (windows.Count == 0)
            {
                return;
            }
            */

            InitializeWindows();
        }

        public override void InitializeWindows()
        {
            // Debug.LogFormat(LOG_FORMAT, "InitializeWindows()");
            // base.InitializeWindows();

            if (windows[currentWindowIndex].firstSelected != null)
            { 
                EventSystem.current.firstSelectedGameObject = windows[currentWindowIndex].firstSelected;
            }
            if (windows[currentWindowIndex].buttonObject != null)
            {
                currentButton = windows[currentWindowIndex].buttonObject;
                currentButtonAnimator = currentButton.GetComponent<Animator>();
                currentButtonAnimator.Play(buttonFadeIn);
            }

            currentWindow = windows[currentWindowIndex].windowObject;
            currentWindowAnimator = currentWindow.GetComponent<Animator>();
            currentWindowAnimator.Play(windowFadeIn);
            onWindowChange.Invoke(currentWindowIndex);

            if (altMode == true)
            { 
                cachedStateLength = 0.3f; 
            }
            else 
            { 
                cachedStateLength = MUIPInternalTools.GetAnimatorClipLength(currentWindowAnimator, MUIPInternalTools.windowManagerStateName); 
            }

            isInitialized = true;

            for (int i = 0; i < windows.Count; i++)
            {
                if (i != currentWindowIndex && cullWindows == true) 
                { 
                    windows[i].windowObject.SetActive(false);
                }
                if (windows[i].buttonObject != null && initializeButtons == true)
                {
                    string tempName = windows[i].windowName;
                    // ButtonManager tempButton = windows[i].buttonObject.GetComponent<ButtonManager>();
                    _MUI_Button tempButton = windows[i].buttonObject.GetComponent<_MUI_Button>();

                    if (tempButton != null)
                    {
                        tempButton.onClick.RemoveAllListeners();
                        // tempButton.onClick.AddListener(() => OpenPanel(tempName));
                        tempButton.onClick.AddListener(() => OnClickTabButton(tempName));
                    }
                }
            }
        }

        public override void OpenPanel(string newPanel)
        {
            // base.OpenPanel(newPanel);
            throw new System.NotSupportedException("");
        }

        public virtual void OnClickTabButton(string newPanel)
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickTabButton(), newPanel : <b><color=yellow>" + newPanel + "</color></b>");

            OpenWindow(newPanel);
        }
    }
}