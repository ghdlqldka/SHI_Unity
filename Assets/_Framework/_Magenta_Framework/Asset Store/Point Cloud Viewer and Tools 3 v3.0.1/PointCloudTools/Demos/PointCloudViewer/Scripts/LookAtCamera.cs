// example script for making object always look towards camera

using UnityEngine;

namespace pointcloudviewer.examples
{
    public class LookAtCamera : MonoBehaviour
    {
        protected Transform cam;

        protected virtual void Start()
        {
            cam = Camera.main.transform;
        }

        protected virtual void LateUpdate()
        {
            transform.LookAt(cam);
        }
    }
}