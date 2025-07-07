using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_WebGL
{
    public class MWGL_Test_GlobalUI : _Magenta_Framework.Test_GlobalUIEx
    {
        private static string LOG_FORMAT = "<color=white><b>[MWGL_Test_GlobalUI]</b></color> {0}";

        protected override void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.Y))
            {
                Debug.LogFormat(LOG_FORMAT, "Y");
                // UI_GlobalHandler.Instance.SubtitleWindow.ShowMessage("Test", 3.0f, OnFinishSubtitles, true);
                MWGL_UI_GlobalHandler.Instance.SubtitleWindow.ShowMessage("Test", 0.0f, OnFinishSubtitles, false);
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                // UI_GlobalHandler.Instance.SubtitleWindow.ShowMessage("Test", 3.0f, OnFinishSubtitles, true);
                MWGL_UI_GlobalHandler.Instance.SubtitleWindow.ShowMessage("", 0.0f);
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                MWGL_UI_GlobalHandler.Instance.PopupDialog.Show("Title~~~", "Message!!~~~~");
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                MWGL_UI_GlobalHandler.Instance.PopupDialog_Portrait.Show("Title~~~", "Message!!~~~~");
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                MWGL_UI_GlobalHandler.Instance.LoadSceneAsync("Test");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                MWGL_UI_GlobalHandler.Instance.ResetApplication();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                MWGL_FadeManager.Instance.FadeIn();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                MWGL_FadeManager.Instance.FadeOut();
            }
#endif
        }

        protected override void OnFinishSubtitles()
        {
            Debug.LogFormat(LOG_FORMAT, "OnFinish<b>Subtitles</b>()");

            MWGL_UI_GlobalHandler.Instance.SubtitleWindow.ShowMessage("Test2", 2.0f);
        }
    }
}