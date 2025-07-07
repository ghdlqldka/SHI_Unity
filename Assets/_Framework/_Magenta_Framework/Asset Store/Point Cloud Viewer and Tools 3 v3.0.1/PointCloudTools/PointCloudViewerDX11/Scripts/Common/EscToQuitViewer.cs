using UnityEngine;

namespace pointcloudviewer.extras
{

    public class EscToQuitViewer : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown("escape"))
            {
                Application.Quit();
            }

        }
    }
}