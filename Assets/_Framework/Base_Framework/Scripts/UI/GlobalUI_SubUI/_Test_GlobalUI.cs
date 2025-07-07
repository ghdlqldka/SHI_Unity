using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Base_Framework
{
    public class _Test_GlobalUI : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=white><b>[_Test_GlobalUI]</b></color> {0}";

        protected virtual void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.Y))
            {
                // UI_GlobalHandler.Instance.SubtitleWindow.ShowMessage("Test", 3.0f, OnFinishSubtitles, true);
                _UI_GlobalHandler.Instance.SubtitleWindow.ShowMessage("Test", 0.0f, OnFinishSubtitles, false);
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                // UI_GlobalHandler.Instance.SubtitleWindow.ShowMessage("Test", 3.0f, OnFinishSubtitles, true);
                _UI_GlobalHandler.Instance.SubtitleWindow.ShowMessage("", 0.0f);
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                _UI_GlobalHandler.Instance.PopupDialog.Show("Title~~~", "Message!!~~~~");
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                _UI_GlobalHandler.Instance.PopupDialog_Portrait.Show("Title~~~", "Message!!~~~~");
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                _UI_GlobalHandler.Instance.LoadSceneAsync("Test");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _UI_GlobalHandler.Instance.ResetApplication();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _FadeManager.Instance.FadeIn();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _FadeManager.Instance.FadeOut();
            }
#endif
        }

        protected virtual void OnFinishSubtitles()
        {
            Debug.LogFormat(LOG_FORMAT, "OnFinish<b>Subtitles</b>()");
            _UI_GlobalHandler.Instance.SubtitleWindow.ShowMessage("Test2", 2.0f);
        }

    }
}