using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Base_Framework
{
    public class _UI_LoadingHandler : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=white><b>[_UI_LoadingHandler]</b></color> {0}";

        public virtual IEnumerator LoadSceneAsyncRoutine(string sceneName)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "LoadSceneAsync(), sceneName : <b><color=yellow>" + sceneName + "</color></b>");

            // Start loading the scene asynchronously
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);

            // Wait until the scene is fully loaded
            while (asyncOp.isDone == false)
            {
                // Optionally, you can track loading progress here
                Debug.LogFormat(LOG_FORMAT, $"Loading progress: {asyncOp.progress}");

                yield return null; // Wait for the next frame
            }

            // Scene has been loaded and activated
            Debug.LogFormat(LOG_FORMAT, "Scene loaded successfully.");
        }
    }
}