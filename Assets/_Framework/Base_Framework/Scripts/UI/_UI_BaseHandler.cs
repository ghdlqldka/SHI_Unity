using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Base_Framework
{
    public abstract class _UI_BaseHandler : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#FFF4D6><b>[UI_BaseHandler]</b></color> {0}";

        protected virtual void Awake()
        {
            if (_Base_Framework_Config.Product == _Base_Framework_Config._Product.Base_Framework)
            {
                if (_GlobalObjectUtilities.Instance == null)
                {
                    GameObject prefab = Resources.Load<GameObject>(_GlobalObjectUtilities.prefabPath);
                    Debug.Assert(prefab != null);
                    GameObject obj = Instantiate(prefab);
                    Debug.LogFormat(LOG_FORMAT, "<color=magenta>Instantiate \"<b>GlobalObjectUtilities</b>\"</color>");
                    obj.name = prefab.name;
                }
            }
            else
            {
                Debug.Assert(false);
            }
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            HandleKeyCode();
        }

#if true
        protected abstract void HandleKeyCode();
#else
        protected string oneButtonText = "";
        protected virtual void HandleKeyCode()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                string activeSceneName = SceneManager.GetActiveScene().name;
                // Debug.LogWarningFormat(LOG_FORMAT, "activeSceneName : " + activeSceneName);
                if (SceneManager.GetActiveScene().name == "HttpPostRequest")
                {
                    // UI_GlobalHandler.Instance.QuitApplication();
                    oneButtonText = "Yes";
                    UI_GlobalHandler.Instance.PopupDialog.Show("Confirm", "Go To Main?", oneButtonText, "No", BackKeyDialogResult);
                }
            }
        }

        protected virtual void BackKeyDialogResult(Button button)
        {
            string buttonText = button.GetComponent<ModernUIButton>().buttonText;
            Debug.LogWarningFormat(LOG_FORMAT, "<color=red><b>BackKeyDialogResult()</b></color>, button : " + button.gameObject.name + ", buttonText : " + buttonText);

            if (string.Compare(buttonText, oneButtonText) == 0) // Yes
            {
                UI_GlobalHandler.Instance.LoadSceneAsync("MainEx");
            }
            else
            {
                //
            }
        }

#endif
    }
}