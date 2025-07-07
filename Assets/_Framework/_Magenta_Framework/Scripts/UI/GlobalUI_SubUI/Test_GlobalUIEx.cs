using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_Framework
{
    public class Test_GlobalUIEx : _Base_Framework._Test_GlobalUI
    {
        private static string LOG_FORMAT = "<color=white><b>[Test_GlobalUIEx]</b></color> {0}";

        protected override void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.Y))
            {
                // UI_GlobalHandler.Instance.SubtitleWindow.ShowMessage("Test", 3.0f, OnFinishSubtitles, true);
                UI_GlobalHandlerEx.Instance.SubtitleWindow.ShowMessage("Test", 2.0f, OnFinishSubtitles, false);
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                // UI_GlobalHandler.Instance.SubtitleWindow.ShowMessage("Test", 3.0f, OnFinishSubtitles, true);
                UI_GlobalHandlerEx.Instance.SubtitleWindow.ShowMessage("", 0.0f);
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                UI_GlobalHandlerEx.Instance.PopupDialog.Show("Title~~~", "Message!!~~~~");
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                UI_GlobalHandlerEx.Instance.PopupDialog_Portrait.Show("Title~~~", "Message!!~~~~");
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                UI_GlobalHandlerEx.Instance.LoadSceneAsync("Test");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UI_GlobalHandlerEx.Instance.ResetApplication();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                FadeManagerEx.Instance.FadeIn();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                FadeManagerEx.Instance.FadeOut();
            }
#endif
        }

        protected override void OnFinishSubtitles()
        {
            Debug.LogFormat(LOG_FORMAT, "OnFinish<b>Subtitles</b>()");

            UI_GlobalHandlerEx.Instance.SubtitleWindow.ShowMessage("Test2", 2.0f);
        }
    }
}